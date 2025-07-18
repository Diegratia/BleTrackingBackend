
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Data.ViewModels;
using System.Text.Json;

namespace BusinessLogic.Services.Implementation
{
    public class GenericDataTableService<TModel, TDto> where TModel : class where TDto : class
    {
        private readonly IQueryable<TModel> _query;
        private readonly IMapper _mapper;
        private readonly string[] _searchableColumns;
        private readonly string[] _validSortColumns;
        private readonly Dictionary<string, Type> _enumColumns;

        public GenericDataTableService(
            IQueryable<TModel> query,
            IMapper mapper,
            string[] searchableColumns,
            string[] validSortColumns,
            Dictionary<string, Type> enumColumns = null)
        {
            _query = query ?? throw new ArgumentNullException(nameof(query));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _searchableColumns = searchableColumns ?? throw new ArgumentNullException(nameof(searchableColumns));
            _validSortColumns = validSortColumns ?? throw new ArgumentNullException(nameof(validSortColumns));
            _enumColumns = enumColumns ?? new Dictionary<string, Type>();
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            if (request.Length < 1)
                throw new ArgumentException("Length must be greater than or equal to 1.");
            if (request.Start < 0)
                throw new ArgumentException("Start cannot be negative.");
            if (string.IsNullOrEmpty(request.SortColumn) || !_validSortColumns.Contains(request.SortColumn))
                request.SortColumn = _validSortColumns.First();
            if (string.IsNullOrEmpty(request.SortDir) || !new[] { "asc", "desc" }.Contains(request.SortDir.ToLower()))
                request.SortDir = "asc";

            var query = _query;

            var totalRecords = await query.CountAsync();

            // search
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = request.SearchValue.ToLower();
                var predicates = _searchableColumns
                    .Select(col =>
                    {
                        if (col.Contains("."))
                        {
                            var parts = col.Split('.');
                            return $"{parts[0]} != null && {col}.ToLower().Contains(@0)";
                        }
                        return $"{col} != null && {col}.ToLower().Contains(@0)";
                    })
                    .Aggregate((current, next) => $"{current} || {next}");
                query = query.Where(predicates, search);
            }

            // Terapkan filter tanggal
            if (request.DateFilters != null && request.DateFilters.Any())
            {
                foreach (var dateFilter in request.DateFilters)
                {
                    if (string.IsNullOrEmpty(dateFilter.Key) || !_validSortColumns.Contains(dateFilter.Key))
                        throw new ArgumentException($"Invalid date column: {dateFilter.Key}");

                    var filter = dateFilter.Value;
                    if (filter.DateFrom.HasValue && filter.DateTo.HasValue)
                    {
                        query = query.Where($"{dateFilter.Key} >= @0 && {dateFilter.Key} <= @1",
                            filter.DateFrom.Value, filter.DateTo.Value.AddDays(1).AddTicks(-1)); 
                    }
                    else if (filter.DateFrom.HasValue)
                    {
                        query = query.Where($"{dateFilter.Key} >= @0", filter.DateFrom.Value);
                    }
                    else if (filter.DateTo.HasValue)
                    {
                        query = query.Where($"{dateFilter.Key} <= @0", filter.DateTo.Value.AddDays(1).AddTicks(-1));
                    }
                }
            }

            // Terapkan filter properti kustom
            if (request.Filters != null && request.Filters.Any())
            {
                foreach (var filter in request.Filters)
                {
                    if (string.IsNullOrEmpty(filter.Key))
                        continue;

                    var value = filter.Value;
                    if (value == null)
                        continue;

                    // Handle JsonElement untuk mendukung deserialisasi dari System.Text.Json
                    if (value is JsonElement jsonElement)
                    {
                        // Coba konversi ke Guid untuk kolom seperti OrganizationId
                        if (filter.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                            jsonElement.ValueKind == JsonValueKind.String &&
                            Guid.TryParse(jsonElement.GetString(), out var guidValue))
                        {
                            query = query.Where($"{filter.Key} == @0", guidValue);
                        }
                        // Coba konversi ke enum jika kolom adalah enum
                        else if (_enumColumns.ContainsKey(filter.Key) && jsonElement.ValueKind == JsonValueKind.String)
                        {
                            var enumType = _enumColumns[filter.Key];
                            var stringValue = jsonElement.GetString();
                            if (Enum.TryParse(enumType, stringValue, true, out var enumValue))
                            {
                                query = query.Where($"{filter.Key} == @0", enumValue);
                            }
                            else
                            {
                                throw new ArgumentException($"Invalid enum value for column '{filter.Key}': {stringValue}");
                            }
                        }
                        // Coba konversi ke string untuk kolom non-enum
                        else if (jsonElement.ValueKind == JsonValueKind.String)
                        {
                            query = query.Where($"{filter.Key} != null && {filter.Key}.ToString().ToLower().Contains(@0)", jsonElement.GetString().ToLower());
                        }
                        // Coba konversi ke int
                        else if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out var intValue))
                        {
                            query = query.Where($"{filter.Key} == @0", intValue);
                        }
                        // Coba konversi ke float
                        else if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetSingle(out var floatValue))
                        {
                            query = query.Where($"{filter.Key} == @0", floatValue);
                        }
                        // Coba konversi ke bool
                        else if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
                        {
                            query = query.Where($"{filter.Key} == @0", jsonElement.GetBoolean());
                        }
                        else
                        {
                            throw new ArgumentException($"Unsupported JsonElement type for column '{filter.Key}': {jsonElement.ValueKind}");
                        }
                    }
                    // Handle tipe lain seperti biasa
                    else if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                    {
                        // Coba konversi ke enum jika kolom adalah enum
                        if (_enumColumns.ContainsKey(filter.Key))
                        {
                            var enumType = _enumColumns[filter.Key];
                            if (Enum.TryParse(enumType, stringValue, true, out var enumValue))
                            {
                                query = query.Where($"{filter.Key} == @0", enumValue);
                            }
                            else
                            {
                                throw new ArgumentException($"Invalid enum value for column '{filter.Key}': {stringValue}");
                            }
                        }
                        else
                        {
                            query = query.Where($"{filter.Key} != null && {filter.Key}.ToString().ToLower().Contains(@0)", stringValue.ToLower());
                        }
                    }
                    else if (value is Guid guidValue)
                    {
                        query = query.Where($"{filter.Key} == @0", guidValue);
                    }
                    else if (value is int intValue)
                    {
                        query = query.Where($"{filter.Key} == @0", intValue);
                    }
                    else if (value is float floatValue)
                    {
                        query = query.Where($"{filter.Key} == @0", floatValue);
                    }
                    else if (value is bool boolValue)
                    {
                        query = query.Where($"{filter.Key} == @0", boolValue);
                    }
                    else
                    {
                        throw new ArgumentException($"Unsupported filter type for column '{filter.Key}': {value.GetType().Name}");
                    }
                }
            }

            // Total setelah filter
            var filteredRecords = await query.CountAsync();

            // Pengurutan
            var sortDirection = request.SortDir.ToLower() == "asc" ? "ascending" : "descending";
            query = query.OrderBy($"{request.SortColumn} {sortDirection}");

            // Paging
            query = query.Skip(request.Start).Take(request.Length);

            // Ambil data
            var data = await query.ToListAsync();
            var dtos = _mapper.Map<IEnumerable<TDto>>(data);

            return new
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = dtos
            };
        }
    }
}