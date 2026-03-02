
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Data.ViewModels;
using System.Text.Json;
using Entities.Models;
using Data.ViewModels.Dto.Helpers.MinimalDto;


namespace BusinessLogic.Services.Implementation
{
    public class MinimalGenericDataTableService<TDto> where TDto : class
    {
        private readonly IQueryable<TDto> _query;
        private readonly IMapper _mapper;
        private readonly string[] _searchableColumns;
        private readonly string[] _validSortColumns;
        private readonly Dictionary<string, Type> _enumColumns;

        public MinimalGenericDataTableService(
            IQueryable<TDto> query,
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
            // if (request.Length == 0)
            // {
            //     request.Length = await _query.CountAsync();
            // }
            if (request.Start < 0)
                throw new ArgumentException("Start cannot be negative.");
            if (string.IsNullOrEmpty(request.SortColumn) || !_validSortColumns.Contains(request.SortColumn))
                request.SortColumn = string.IsNullOrEmpty(request.SortColumn) ? (_validSortColumns.Any() ? _validSortColumns.First() : "UpdatedAt") : request.SortColumn;
            if (string.IsNullOrEmpty(request.SortDir) || !new[] { "asc", "desc" }.Contains(request.SortDir.ToLower()))
                request.SortDir = "desc";

            var query = _query;

            // Calculate total records before filtering
            var totalRecords = await query.CountAsync();

            // Search
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

            // Date filter
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

            // Custom filters
            if (request.Filters != null && request.Filters.Any())
            {
                foreach (var filter in request.Filters)
                {
                    if (string.IsNullOrEmpty(filter.Key))
                        continue;

                    var value = filter.Value;
                    if (value == null)
                        continue;

                    if (value is JsonElement jsonElement)
                    {
                        if (filter.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                        {
                            if (jsonElement.ValueKind == JsonValueKind.Array)
                            {
                                var guidValues = jsonElement.EnumerateArray()
                                    .Select(e => Guid.TryParse(e.GetString(), out var guid) ? guid : (Guid?)null)
                                    .Where(g => g.HasValue)
                                    .Select(g => g.Value)
                                    .ToArray();
                                if (guidValues.Any())
                                {
                                    query = query.Where($"@0.Contains({filter.Key})", guidValues);
                                }
                            }
                            else if (jsonElement.ValueKind == JsonValueKind.String && Guid.TryParse(jsonElement.GetString(), out var guidValue))
                            {
                                query = query.Where($"{filter.Key} == @0", guidValue);
                            }
                            else
                            {
                                var stringValue = jsonElement.GetString();
                                query = query.Where($"{filter.Key} != null && {filter.Key}.ToLower().Contains(@0)", stringValue?.ToLower());
                            }
                        }
                        else if (_enumColumns.ContainsKey(filter.Key))
                        {
                            var enumType = _enumColumns[filter.Key];
                            if (jsonElement.ValueKind == JsonValueKind.Array)
                            {
                                var enumValues = jsonElement.EnumerateArray()
                                    .Select(e =>
                                    {
                                        if (e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var intVal))
                                        {
                                            return Enum.IsDefined(enumType, intVal) ? Enum.ToObject(enumType, intVal) : null;
                                        }
                                        else if (e.ValueKind == JsonValueKind.String && Enum.TryParse(enumType, e.GetString(), true, out var enumObj))
                                        {
                                            return enumObj;
                                        }
                                        return null;
                                    })
                                    .Where(v => v != null)
                                    .ToArray();

                                if (enumValues.Any())
                                {
                                    query = query.Where($"@0.Contains({filter.Key})", enumValues);
                                }
                            }
                            else if (jsonElement.ValueKind == JsonValueKind.String)
                            {
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

                            
                            else if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out var intEnumVal))
                            {
                                var enumValue = Enum.ToObject(enumType, intEnumVal);
                                query = query.Where($"{filter.Key} == @0", enumValue);
                            }
                            else
                            {
                                throw new ArgumentException($"Unsupported JsonElement type for enum column '{filter.Key}': {jsonElement.ValueKind}");
                            }
                        }
                        else if (jsonElement.ValueKind == JsonValueKind.String)
                        {
                            // query = query.Where($"{filter.Key} != null && {filter.Key}.ToString().ToLower().Contains(@0)", jsonElement.GetString()?.ToLower());
                        query = query.Where(
                        $"{filter.Key} != null && {filter.Key}.ToLower().Contains(@0)",
                        jsonElement.GetString()?.ToLower()
                    );

                        }
                        else if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out var intValue))
                        {
                            query = query.Where($"{filter.Key} == @0", intValue);
                        }
                        else if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetSingle(out var floatValue))
                        {
                            query = query.Where($"{filter.Key} == @0", floatValue);
                        }
                        else if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
                        {
                            query = query.Where($"{filter.Key} == @0", jsonElement.GetBoolean());
                        }
                        else
                        {
                            throw new ArgumentException($"Unsupported JsonElement type for column '{filter.Key}': {jsonElement.ValueKind}");
                        }
                    }
                    else if (value is IEnumerable<object> enumCollection && _enumColumns.ContainsKey(filter.Key))
                    {
                        var enumType = _enumColumns[filter.Key];
                        var enumValues = enumCollection
                            .Select(e => Enum.TryParse(enumType, e?.ToString(), true, out var enumValue) ? enumValue : null)
                            .Where(e => e != null)
                            .ToArray();
                        if (enumValues.Any())
                        {
                            query = query.Where($"@0.Contains({filter.Key})", enumValues);
                        }
                    }
                    else if (value is IEnumerable<Guid> guidCollection)
                    {
                        var guidValues = guidCollection.ToArray();
                        if (guidValues.Any())
                        {
                            query = query.Where($"@0.Contains({filter.Key})", guidValues);
                        }
                    }
                    else if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                    {
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

            // Count filtered records
            var filteredRecords = await query.CountAsync();

            // Sorting
            var sortDirection = request.SortDir.ToLower() == "asc" ? "ascending" : "descending";
            query = query.OrderBy($"{request.SortColumn} {sortDirection}");

            // Paging
                if (request.Length == 0)
            {
                request.Length = await _query.CountAsync();
            }
            // query = query.Skip(request.Start).Take(request.Length);

            // Execute query
            var data = await query.ToListAsync();

            return new
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data
            };
        }
    }
}

