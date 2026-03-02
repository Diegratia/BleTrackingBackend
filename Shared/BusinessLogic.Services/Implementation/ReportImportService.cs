using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Shared.Contracts.Reporting;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using BusinessLogic.Services.Interface;

namespace BusinessLogic.Services.Implementation;

public class ReportImportService : IReportImportService
{
    public async Task<ImportResult<T>> ImportFromExcelAsync<T>(
        IFormFile file,
        List<ImportColumnMapping> mappings,
        int headerRow = 0,
        int dataStartRow = 1)
    {
        var result = new ImportResult<T>();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        var totalRows = worksheet.RowsUsed().Count() - dataStartRow;
        if (totalRows < 0) totalRows = 0;
        result.TotalRows = totalRows;

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        int currentRow = dataStartRow;
        var items = new List<T>();

        foreach (var row in worksheet.RowsUsed().Skip(dataStartRow))
        {
            try
            {
                var item = Activator.CreateInstance<T>();
                bool rowHasError = false;

                foreach (var mapping in mappings)
                {
                    var cell = row.Cell(mapping.ColumnIndex + 1);
                    var cellValue = cell.Value;

                    // Check if cell is empty (XLCellValue is a struct, use TryGetValue or check IsBlank)
                    if (cell.IsEmpty() || cell.DataType == XLDataType.Blank)
                    {
                        if (mapping.Required)
                        {
                            result.Errors.Add(new ImportError
                            {
                                RowNumber = currentRow + 1,
                                Field = mapping.DisplayName ?? mapping.PropertyName,
                                Message = "Required field is missing",
                                RawValue = null
                            });
                            rowHasError = true;
                            result.FailureCount++;
                            break;
                        }
                        continue;
                    }

                    if (!properties.TryGetValue(mapping.PropertyName, out var property))
                    {
                        result.Errors.Add(new ImportError
                        {
                            RowNumber = currentRow + 1,
                            Field = mapping.DisplayName ?? mapping.PropertyName,
                            Message = $"Property '{mapping.PropertyName}' not found on type {typeof(T).Name}",
                            RawValue = cellValue.ToString()
                        });
                        rowHasError = true;
                        result.FailureCount++;
                        break;
                    }

                    try
                    {
                        var convertedValue = ConvertValue(cellValue, property.PropertyType, mapping.Format);
                        property.SetValue(item, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new ImportError
                        {
                            RowNumber = currentRow + 1,
                            Field = mapping.DisplayName ?? mapping.PropertyName,
                            Message = $"Failed to convert value: {ex.Message}",
                            RawValue = cellValue.ToString()
                        });
                        rowHasError = true;
                        result.FailureCount++;
                        break;
                    }
                }

                if (!rowHasError)
                {
                    items.Add(item);
                    result.SuccessCount++;
                }

                currentRow++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportError
                {
                    RowNumber = currentRow + 1,
                    Field = "Row",
                    Message = $"Failed to process row: {ex.Message}",
                    RawValue = null
                });
                result.FailureCount++;
                currentRow++;
            }
        }

        result.ImportedData = items;
        return result;
    }

    private static object? ConvertValue(object value, Type targetType, string? format)
    {
        if (value == null || value is DBNull)
            return null;

        var stringValue = value.ToString();

        if (string.IsNullOrWhiteSpace(stringValue))
            return null;

        targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (targetType == typeof(string))
            return stringValue;

        if (targetType == typeof(bool) || targetType == typeof(bool?))
        {
            if (bool.TryParse(stringValue, out var boolResult))
                return boolResult;

            return stringValue.ToLower() switch
            {
                "yes" => true,
                "y" => true,
                "1" => true,
                "true" => true,
                "no" => false,
                "n" => false,
                "0" => false,
                "false" => false,
                _ => throw new FormatException($"Cannot convert '{stringValue}' to Boolean")
            };
        }

        if (targetType == typeof(byte) || targetType == typeof(byte?))
            return byte.Parse(stringValue, CultureInfo.InvariantCulture);

        if (targetType == typeof(short) || targetType == typeof(short?))
            return short.Parse(stringValue, CultureInfo.InvariantCulture);

        if (targetType == typeof(int) || targetType == typeof(int?))
            return int.Parse(stringValue, CultureInfo.InvariantCulture);

        if (targetType == typeof(long) || targetType == typeof(long?))
            return long.Parse(stringValue, CultureInfo.InvariantCulture);

        if (targetType == typeof(decimal) || targetType == typeof(decimal?))
            return decimal.Parse(stringValue, CultureInfo.InvariantCulture);

        if (targetType == typeof(double) || targetType == typeof(double?))
            return double.Parse(stringValue, CultureInfo.InvariantCulture);

        if (targetType == typeof(float) || targetType == typeof(float?))
            return float.Parse(stringValue, CultureInfo.InvariantCulture);

        if (targetType == typeof(Guid) || targetType == typeof(Guid?))
            return Guid.Parse(stringValue);

        if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
        {
            if (value is DateTime dt)
                return dt;

            if (!string.IsNullOrEmpty(format))
                return DateTime.ParseExact(stringValue, format, CultureInfo.InvariantCulture);

            return DateTime.Parse(stringValue, CultureInfo.InvariantCulture);
        }

        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, stringValue, true);
        }

        // Try type converter as fallback
        var converter = TypeDescriptor.GetConverter(targetType);
        if (converter.CanConvertFrom(typeof(string)))
            return converter.ConvertFromString(null, CultureInfo.InvariantCulture, stringValue);

        throw new InvalidOperationException($"Cannot convert value to type {targetType.Name}");
    }
}
