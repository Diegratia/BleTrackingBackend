using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.Contracts.Reporting;
using System.Reflection;
using System.Globalization;
using BusinessLogic.Services.Interface;

namespace BusinessLogic.Services.Implementation;

public class ReportExportService : IReportExportService
{
    public ReportExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, ReportMetadata metadata)
    {
        return await Task.Run(() =>
        {
            var dataList = data.ToList();
            metadata.TotalRecords = dataList.Count;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(metadata.Orientation == ReportOrientation.Landscape
                        ? PageSizes.A4.Landscape()
                        : PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Header
                    page.Header().Column(column =>
                    {
                        column.Item()
                            .Text(metadata.Title)
                            .SemiBold().FontSize(16).FontColor(Colors.Black).AlignCenter();

                        if (!string.IsNullOrEmpty(metadata.Subtitle))
                        {
                            column.Item()
                                .PaddingTop(5)
                                .Text(metadata.Subtitle)
                                .FontSize(12).FontColor(Colors.Grey.Darken2).AlignCenter();
                        }

                        if (!string.IsNullOrEmpty(metadata.FilterInfo))
                        {
                            column.Item()
                                .PaddingTop(5)
                                .Text(metadata.FilterInfo)
                                .FontSize(10).FontColor(Colors.Grey.Darken2).AlignCenter();
                        }

                        column.Item()
                            .PaddingTop(10)
                            .Text($"Total Records: {metadata.TotalRecords}")
                            .FontSize(11).FontColor(Colors.Blue.Medium).AlignCenter();
                    });

                    // Content Table
                    page.Content().Table(table =>
                    {
                        // Define columns - only call ColumnsDefinition ONCE
                        table.ColumnsDefinition(columns =>
                        {
                            if (metadata.IncludeRowNumbers)
                                columns.ConstantColumn(35);

                            foreach (var column in metadata.Columns)
                            {
                                if (column.Width >= 1)
                                    columns.RelativeColumn(column.Width);
                                else
                                    columns.ConstantColumn(column.Width);
                            }
                        });

                        // Header row
                        table.Header(header =>
                        {
                            if (metadata.IncludeRowNumbers)
                            {
                                header.Cell().Element(CellStyle).Text("#").SemiBold();
                            }

                            foreach (var column in metadata.Columns)
                            {
                                header.Cell().Element(CellStyle).Text(column.Header).SemiBold();
                            }
                        });

                        // Data rows
                        int rowIndex = 1;
                        foreach (var item in dataList)
                        {
                            if (metadata.IncludeRowNumbers)
                            {
                                table.Cell().Element(CellStyle).Text(rowIndex++.ToString());
                            }

                            foreach (var column in metadata.Columns)
                            {
                                var value = GetPropertyValue(item, column.PropertyName);
                                var formattedValue = FormatValue(value, column.Format);
                                table.Cell().Element(CellStyle).Text(formattedValue);
                            }
                        }
                    });

                    // Footer with page numbers
                    if (metadata.IncludePageNumbers)
                    {
                        page.Footer()
                            .AlignCenter()
                            .Text(txt =>
                            {
                                txt.Span("Generated at: ").SemiBold();
                                txt.Span(metadata.GeneratedAtUtc.ToString("yyyy-MM-dd HH:mm:ss") +
                                    $" {metadata.TimeZone}");
                                txt.Span(" | Page ").SemiBold();
                                txt.CurrentPageNumber();
                                txt.Span(" of ").SemiBold();
                                txt.TotalPages();
                            });
                    }
                });
            });

            return document.GeneratePdf();
        });
    }

    public async Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, ReportMetadata metadata)
    {
        return await Task.Run(() =>
        {
            var dataList = data.ToList();
            metadata.TotalRecords = dataList.Count;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Report");

            int currentRow = 1;
            int columnCount = metadata.Columns.Count + (metadata.IncludeRowNumbers ? 1 : 0);

            // Title
            worksheet.Cell(currentRow, 1).Value = metadata.Title;
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 16;
            worksheet.Range(currentRow, 1, currentRow, columnCount).Merge();
            currentRow++;

            // Subtitle
            if (!string.IsNullOrEmpty(metadata.Subtitle))
            {
                worksheet.Cell(currentRow, 1).Value = metadata.Subtitle;
                worksheet.Range(currentRow, 1, currentRow, columnCount).Merge();
                currentRow++;
            }

            // Filter info
            if (!string.IsNullOrEmpty(metadata.FilterInfo))
            {
                worksheet.Cell(currentRow, 1).Value = metadata.FilterInfo;
                worksheet.Range(currentRow, 1, currentRow, columnCount).Merge();
                currentRow++;
            }

            // Total records
            worksheet.Cell(currentRow, 1).Value = $"Total Records: {metadata.TotalRecords}";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontColor = XLColor.Blue;
            worksheet.Range(currentRow, 1, currentRow, columnCount).Merge();
            currentRow++;

            // Empty row
            currentRow++;

            // Header row
            int headerCol = 1;
            if (metadata.IncludeRowNumbers)
            {
                worksheet.Cell(currentRow, headerCol++).Value = "#";
            }

            foreach (var column in metadata.Columns)
            {
                worksheet.Cell(currentRow, headerCol++).Value = column.Header;
            }

            var headerRange = worksheet.Range(currentRow, 1, currentRow, columnCount);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            currentRow++;

            // Data rows
            int rowIndex = 1;
            foreach (var item in dataList)
            {
                int dataCol = 1;

                if (metadata.IncludeRowNumbers)
                {
                    worksheet.Cell(currentRow, dataCol++).Value = rowIndex++;
                }

                foreach (var column in metadata.Columns)
                {
                    var value = GetPropertyValue(item, column.PropertyName);
                    var cell = worksheet.Cell(currentRow, dataCol++);

                    if (value == null)
                    {
                        cell.Value = string.Empty;
                    }
                    else if (value is DateTime dt)
                    {
                        cell.Value = dt;
                        if (!string.IsNullOrEmpty(column.Format))
                        {
                            cell.Style.DateFormat.Format = column.Format.Replace("HH", "hh").Replace("mm", "mm");
                        }
                        else
                        {
                            cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
                        }
                    }
                    else if (value is bool b)
                    {
                        cell.Value = b ? "Yes" : "No";
                    }
                    else if (value is int i)
                    {
                        cell.Value = i;
                    }
                    else if (value is long l)
                    {
                        cell.Value = l;
                    }
                    else if (value is short s)
                    {
                        cell.Value = s;
                    }
                    else if (value is byte by)
                    {
                        cell.Value = by;
                    }
                    else if (value is double d)
                    {
                        cell.Value = d;
                    }
                    else if (value is float f)
                    {
                        cell.Value = f;
                    }
                    else if (value is decimal m)
                    {
                        cell.Value = m;
                    }
                    else
                    {
                        cell.Value = value.ToString();
                    }

                    // Apply alignment
                    if (column.Align == ColumnAlign.Center)
                    {
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    else if (column.Align == ColumnAlign.Right)
                    {
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }
                }

                currentRow++;
            }

            // Apply borders to all data cells
            var dataStartRow = currentRow - dataList.Count - 1;
            var dataRange = worksheet.Range(dataStartRow, 1, currentRow - 1, columnCount);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Auto-adjust column widths
            worksheet.Columns().AdjustToContents();

            // Footer with generation time
            currentRow += 2;
            worksheet.Cell(currentRow, 1).Value = "Generated at:";
            worksheet.Cell(currentRow, 2).Value = metadata.GeneratedAtUtc.ToString("yyyy-MM-dd HH:mm:ss") +
                $" {metadata.TimeZone}";
            worksheet.Cell(currentRow, 2).Style.Font.Italic = true;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        });
    }

    public string GenerateReportTitle(string baseTitle, string? customTitle, string? timeRange)
    {
        if (!string.IsNullOrEmpty(customTitle))
            return customTitle;

        if (!string.IsNullOrEmpty(timeRange))
        {
            return $"{baseTitle} - {timeRange.ToUpper()}";
        }

        return baseTitle;
    }

    public string GenerateFilterInfo(ReportFilterInfo filters)
    {
        var filterParts = new List<string>();

        if (filters.From.HasValue && filters.To.HasValue)
        {
            filterParts.Add($"Period: {filters.From.Value:yyyy-MM-dd} to {filters.To.Value:yyyy-MM-dd}");
        }
        else if (!string.IsNullOrEmpty(filters.TimeRange))
        {
            filterParts.Add($"Time Range: {filters.TimeRange}");
        }

        if (filters.BuildingId.HasValue)
            filterParts.Add("Building Filtered");

        if (filters.FloorId.HasValue)
            filterParts.Add("Floor Filtered");

        if (filters.AreaId.HasValue)
            filterParts.Add("Area Filtered");

        if (filters.CategoryId.HasValue)
            filterParts.Add("Category Filtered");

        if (!string.IsNullOrEmpty(filters.Search))
            filterParts.Add($"Search: '{filters.Search}'");

        if (filters.Status.HasValue)
            filterParts.Add($"Status: {filters.Status.Value}");

        if (!string.IsNullOrEmpty(filters.CustomFilter))
            filterParts.Add(filters.CustomFilter);

        return filterParts.Any() ? string.Join(" | ", filterParts) : "All Data";
    }

    public ReportExportRequest ApplyDefaultPagination(ReportExportRequest? request)
    {
        return new ReportExportRequest
        {
            Page = request?.Page ?? 1,
            PageSize = request?.PageSize ?? 10000,
            SortColumn = request?.SortColumn,
            SortDir = request?.SortDir,
            DateFrom = request?.DateFrom,
            DateTo = request?.DateTo,
            TimeRange = request?.TimeRange,
            Search = request?.Search,
            Status = request?.Status
        };
    }

    private static object? GetPropertyValue(object obj, string propertyName)
    {
        if (obj == null || string.IsNullOrEmpty(propertyName))
            return null;

        var properties = propertyName.Split('.');
        object? current = obj;

        foreach (var property in properties)
        {
            if (current == null) return null;

            var propInfo = current.GetType().GetProperty(property,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (propInfo == null) return null;

            current = propInfo.GetValue(current);

            // Handle nullable enum types
            if (current is Enum enumValue)
            {
                return enumValue.ToString();
            }
        }

        return current;
    }

    private static string FormatValue(object? value, string? format)
    {
        if (value == null) return string.Empty;

        if (value is DateTime dt)
        {
            return !string.IsNullOrEmpty(format)
                ? dt.ToString(format)
                : dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        if (value is DateTimeOffset dto)
        {
            return !string.IsNullOrEmpty(format)
                ? dto.ToString(format)
                : dto.ToString("yyyy-MM-dd HH:mm:ss");
        }

        if (value is bool b)
            return b ? "Yes" : "No";

        if (value is Enum enumValue)
            return enumValue.ToString();

        return value.ToString() ?? string.Empty;
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(4)
            .PaddingHorizontal(6);
    }
}
