using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.EntityFrameworkCore.Query;

namespace BusinessLogic.Services.Implementation
{
    public class GenericDataTableProjectionService<TModel, TDto>
        where TModel : class
        where TDto : class
    {
        private readonly IQueryable<TModel> _query;
        private readonly IMapper _mapper;
        private readonly string[] _searchableColumns;
        private readonly string[] _validSortColumns;
        private readonly Dictionary<string, Type> _enumColumns;

        public GenericDataTableProjectionService(
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
            bool isEfQuery = _query.Provider is IAsyncQueryProvider;

            if (!isEfQuery)
            {
                // Jalankan versi sync (cache)
                return FilterSync(request);
            }
            if (request.Start < 0)
                throw new ArgumentException("Start cannot be negative.");

            if (request.Length == 0)
                throw new ArgumentException("Length cannot be negative.");

            if (request.Length <= 0 || request.Length > 1000)
                request.Length = 1000;

            // ✅ Validasi Sort Column & Direction
            if (string.IsNullOrEmpty(request.SortColumn) || !_validSortColumns.Contains(request.SortColumn))
                request.SortColumn = _validSortColumns.FirstOrDefault() ?? "UpdatedAt";

            if (string.IsNullOrEmpty(request.SortDir) || !new[] { "asc", "desc" }.Contains(request.SortDir.ToLower()))
                request.SortDir = "desc";

            // 🧠 Base Query (no tracking)
            var query = _query.AsNoTracking();

            // =======================================
            // 1️⃣ Total Records (Before Filter)
            // =======================================
            long totalRecords = await query.LongCountAsync();

            // =======================================
            // 2️⃣ Apply TimeReport
            // =======================================
            // ✅ Override: jika DateFilters ada isinya, anggap CustomDate
            if (request.DateFilters != null && request.DateFilters.Any())
            {
                request.TimeReport = "CustomDate";
            }
            if (!string.IsNullOrEmpty(request.TimeReport) && request.TimeReport != "CustomDate")
            {
                var timeRange = GetTimeRange(request.TimeReport);
                if (timeRange.HasValue)
                {
                    var timeColumn = _validSortColumns.Contains(request.SortColumn)
                        ? request.SortColumn
                        : "UpdatedAt";

                    query = query.Where($"{timeColumn} >= @0 && {timeColumn} <= @1",
                        timeRange.Value.from, timeRange.Value.to);
                }
            }

            // =======================================
            // 3️⃣ Apply Search
            // =======================================
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = request.SearchValue.ToLower();
                var predicates = _searchableColumns
                    .Select(col =>
                        col.Contains(".")
                            ? $"{col.Split('.')[0]} != null && {col}.ToLower().Contains(@0)"
                            : $"{col} != null && {col}.ToLower().Contains(@0)")
                    .Aggregate((current, next) => $"{current} || {next}");
                query = query.Where(predicates, search);
            }

            // =======================================
            // 4️⃣ Apply Date & Custom Filters
            // =======================================
            query = ApplyDateAndCustomFilters(query, request);

            // =======================================
            // 5️⃣ Filtered Records (after all filters)
            // =======================================
            long filteredRecords = await query.LongCountAsync();

            if (string.Equals(request.Mode, "count", StringComparison.OrdinalIgnoreCase))
            {
                return new
                {
                    draw = request.Draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = filteredRecords
                };
            }

            // =======================================
            // 6️⃣ Sorting & Paging
            // =======================================
            query = query.OrderBy($"{request.SortColumn} {request.SortDir}");
            query = query.Skip(request.Start).Take(request.Length);

            // =======================================
            // 7️⃣ Hybrid DTO Projection
            // =======================================
            List<TDto> data;
            if (typeof(TModel) == typeof(TDto))
            {
                data = await query.Cast<TDto>().ToListAsync();
            }
            else
            {
                try
                {
                    data = await query.ProjectTo<TDto>(_mapper.ConfigurationProvider).ToListAsync();
                }
                catch
                {
                    var entities = await query.ToListAsync();
                    data = _mapper.Map<List<TDto>>(entities);
                }
            }

            // =======================================
            // 8️⃣ Return
            // =======================================
            return new
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data
            };
        }

        private object FilterSync(DataTablesRequest request)
        {
            var query = _query.AsQueryable();

            // Validasi dasar
            if (request.Length <= 0 || request.Length > 1000)
                request.Length = 1000;

            if (string.IsNullOrEmpty(request.SortColumn) || !_validSortColumns.Contains(request.SortColumn))
                request.SortColumn = _validSortColumns.FirstOrDefault() ?? "UpdatedAt";

            if (string.IsNullOrEmpty(request.SortDir) || !new[] { "asc", "desc" }.Contains(request.SortDir.ToLower()))
                request.SortDir = "desc";

            // 🔍 Apply Search
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = request.SearchValue.ToLower();
                var predicates = _searchableColumns
                    .Select(col => $"{col} != null && {col}.ToLower().Contains(@0)")
                    .Aggregate((a, b) => $"{a} || {b}");
                query = query.Where(predicates, search);
            }

            // Apply Sort
            query = query.OrderBy($"{request.SortColumn} {request.SortDir}");

            // Apply Pagination
            var total = query.Count();
            var data = query.Skip(request.Start).Take(request.Length).ToList();

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = total,
                data
            };
        }




        // =======================================
        // 🧩 Date & Custom Filters
        // =======================================
        private IQueryable<TModel> ApplyDateAndCustomFilters(IQueryable<TModel> query, DataTablesRequest request)
        {
            // 🔹 Date Filters
            if (request.DateFilters != null && request.DateFilters.Any())
            {
                foreach (var dateFilter in request.DateFilters)
                {
                    if (string.IsNullOrEmpty(dateFilter.Key) || !_validSortColumns.Contains(dateFilter.Key))
                        continue;

                    var filter = dateFilter.Value;
                    if (filter.DateFrom.HasValue && filter.DateTo.HasValue)
                        query = query.Where($"{dateFilter.Key} >= @0 && {dateFilter.Key} <= @1",
                            filter.DateFrom.Value, filter.DateTo.Value.AddDays(1).AddTicks(-1));
                    else if (filter.DateFrom.HasValue)
                        query = query.Where($"{dateFilter.Key} >= @0", filter.DateFrom.Value);
                    else if (filter.DateTo.HasValue)
                        query = query.Where($"{dateFilter.Key} <= @0", filter.DateTo.Value.AddDays(1).AddTicks(-1));
                }
            }

            // 🔹 Custom Filters
            if (request.Filters != null && request.Filters.Any())
            {
                foreach (var filter in request.Filters)
                {
                    if (string.IsNullOrEmpty(filter.Key) || filter.Value == null)
                        continue;

                    var key = filter.Key;
                    var value = filter.Value;

                    // ✅ 1️⃣ Handle JsonElement
                    if (value is JsonElement json)
                    {

                        // === 1️⃣ Handle GUID / ID filters ===
                        if (key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                        {
                            // 🔹 Fallback otomatis: "Parent.Id" → "ParentId"
                            string targetKey = key;
                            if (key.Contains('.'))
                            {
                                var candidate = key.Replace(".", "");
                                if (typeof(TModel).GetProperty(candidate) != null)
                                    targetKey = candidate; // pakai FK langsung
                                else
                                {
                                    var parent = key.Split('.')[0];
                                    query = query.Where($"{parent} != null"); // guard
                                }
                            }

                            // 🔹 Handle array GUID
                            if (json.ValueKind == JsonValueKind.Array)
                            {
                                var guids = json.EnumerateArray()
                                    .Select(e => Guid.TryParse(e.GetString(), out var g) ? g : (Guid?)null)
                                    .Where(g => g.HasValue)
                                    .Select(g => g.Value)
                                    .ToArray();

                                if (guids.Any())
                                {
                                    var prop = typeof(TModel).GetProperty(targetKey);
                                    var isNullable = prop != null && prop.PropertyType == typeof(Guid?);
                                    query = isNullable
                                        ? query.Where($"{targetKey} != null && @0.Contains({targetKey}.Value)", guids)
                                        : query.Where($"@0.Contains({targetKey})", guids);
                                }
                            }
                            // 🔹 Handle single GUID string
                            else if (json.ValueKind == JsonValueKind.String && Guid.TryParse(json.GetString(), out var guidVal))
                            {
                                var prop = typeof(TModel).GetProperty(targetKey);
                                var isNullable = prop != null && prop.PropertyType == typeof(Guid?);
                                query = isNullable
                                    ? query.Where($"{targetKey} != null && {targetKey}.Value == @0", guidVal)
                                    : query.Where($"{targetKey} == @0", guidVal);
                            }
                            else
                            {
                                // fallback string search
                                var strVal = json.GetString();
                                if (!string.IsNullOrEmpty(strVal))
                                    query = query.Where($"{targetKey} != null && {targetKey}.ToString().ToLower().Contains(@0)", strVal.ToLower());
                            }

                            continue;
                        }

                        // 🔹 Handle Enum columns
                        if (_enumColumns.ContainsKey(key))
                        {
                            var enumType = _enumColumns[key];
                            if (json.ValueKind == JsonValueKind.Array)
                            {
                                var enumValues = json.EnumerateArray()
                                    .Select(e =>
                                    {
                                        if (e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var intVal))
                                            return Enum.IsDefined(enumType, intVal) ? Enum.ToObject(enumType, intVal) : null;
                                        if (e.ValueKind == JsonValueKind.String && Enum.TryParse(enumType, e.GetString(), true, out var enumObj))
                                            return enumObj;
                                        return null;
                                    })
                                    .Where(v => v != null)
                                    .ToArray();

                                if (enumValues.Any())
                                    query = query.Where($"@0.Contains({key})", enumValues);
                            }
                            else if (json.ValueKind == JsonValueKind.String)
                            {
                                var strVal = json.GetString();
                                if (Enum.TryParse(enumType, strVal, true, out var enumVal))
                                    query = query.Where($"{key} == @0", enumVal);
                            }
                            else if (json.ValueKind == JsonValueKind.Number && json.TryGetInt32(out var intEnum))
                            {
                                var enumVal = Enum.ToObject(enumType, intEnum);
                                query = query.Where($"{key} == @0", enumVal);
                            }
                            continue;
                        }


                        // 🔹 Handle simple JsonElement types
                        switch (json.ValueKind)
                        {
                            case JsonValueKind.String:
                                query = query.Where($"{key} != null && {key}.ToString().ToLower().Contains(@0)", json.GetString().ToLower());
                                break;

                            case JsonValueKind.Number when json.TryGetInt32(out var intVal):
                                query = query.Where($"{key} == @0", intVal);
                                break;

                            case JsonValueKind.Number when json.TryGetSingle(out var floatVal):
                                query = query.Where($"{key} == @0", floatVal);
                                break;

                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                query = query.Where($"{key} == @0", json.GetBoolean());
                                break;

                            default:
                                throw new ArgumentException($"Unsupported JsonElement type for '{key}': {json.ValueKind}");
                        }

                        continue;
                    }

                    // ✅ 2️⃣ Enum Collections
                    if (value is IEnumerable<object> enumCollection && _enumColumns.ContainsKey(key))
                    {
                        var enumType = _enumColumns[key];
                        var enums = enumCollection
                            .Select(e => Enum.TryParse(enumType, e?.ToString(), true, out var parsed) ? parsed : null)
                            .Where(e => e != null)
                            .ToArray();
                        if (enums.Any())
                            query = query.Where($"@0.Contains({key})", enums);
                        continue;
                    }

                    // ✅ 3️⃣ GUID Collections
                    if (value is IEnumerable<Guid> guidCollection)
                    {
                        var guids = guidCollection.ToArray();
                        if (guids.Any())
                            query = query.Where($"@0.Contains({key})", guids);
                        continue;
                    }

                    // ✅ 4️⃣ String Values
                    if (value is string str && !string.IsNullOrEmpty(str))
                    {
                        if (_enumColumns.ContainsKey(key))
                        {
                            var enumType = _enumColumns[key];
                            if (Enum.TryParse(enumType, str, true, out var enumVal))
                                query = query.Where($"{key} == @0", enumVal);
                            else
                                throw new ArgumentException($"Invalid enum value for '{key}': {str}");
                        }
                        else
                        {
                            query = query.Where($"{key} != null && {key}.ToString().ToLower().Contains(@0)", str.ToLower());
                        }
                        continue;
                    }

                    // ✅ 5️⃣ Simple value types
                    switch (value)
                    {
                        case Guid guidVal:
                            query = query.Where($"{key} == @0", guidVal);
                            break;
                        case int intVal:
                            query = query.Where($"{key} == @0", intVal);
                            break;
                        case float floatVal:
                            query = query.Where($"{key} == @0", floatVal);
                            break;
                        case bool boolVal:
                            query = query.Where($"{key} == @0", boolVal);
                            break;
                        default:
                            throw new ArgumentException($"Unsupported filter type for column '{key}': {value.GetType().Name}");
                    }
                }
            }

            return query;
        }

        // =======================================
        // 🧩 JSON Filter Handler (Nested + Enum)
        // =======================================
        private IQueryable<TModel> ApplyJsonFilter(IQueryable<TModel> query, string key, JsonElement json)
        {
            // 🔸 Handle GUID/ID filters
            if (key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
            {
                if (json.ValueKind == JsonValueKind.Array)
                {
                    var guids = json.EnumerateArray()
                        .Select(e => Guid.TryParse(e.GetString(), out var g) ? g : (Guid?)null)
                        .Where(g => g.HasValue)
                        .Select(g => g.Value)
                        .ToArray();
                    if (guids.Any())
                    {
                        var prop = typeof(TModel).GetProperty(key.Contains('.') ? key.Split('.').Last() : key);
                        var isNullableGuid = prop != null && prop.PropertyType == typeof(Guid?);
                        if (key.Contains('.'))
                        {
                            var parent = key.Split('.')[0];
                            query = query.Where($"{parent} != null && @0.Contains({key})", guids);
                        }
                        else
                        {
                            query = isNullableGuid
                                ? query.Where($"{key} != null && @0.Contains({key}.Value)", guids)
                                : query.Where($"@0.Contains({key})", guids);
                        }
                    }
                }
                else if (json.ValueKind == JsonValueKind.String && Guid.TryParse(json.GetString(), out var guidVal))
                {
                    var prop = typeof(TModel).GetProperty(key.Contains('.') ? key.Split('.').Last() : key);
                    var isNullableGuid = prop != null && prop.PropertyType == typeof(Guid?);
                    if (key.Contains('.'))
                    {
                        var parent = key.Split('.')[0];
                        query = query.Where($"{parent} != null && {key} == @0", guidVal);
                    }
                    else
                    {
                        query = isNullableGuid
                            ? query.Where($"{key} != null && {key}.Value == @0", guidVal)
                            : query.Where($"{key} == @0", guidVal);
                    }
                }
                else
                {
                    var strVal = json.GetString();
                    query = query.Where($"{key} != null && {key}.ToLower().Contains(@0)", strVal?.ToLower());
                }
                return query;
            }

            // 🔸 Enum Columns
            if (_enumColumns.ContainsKey(key))
            {
                var enumType = _enumColumns[key];

                if (json.ValueKind == JsonValueKind.Array)
                {
                    var enumValues = json.EnumerateArray()
                        .Select(e =>
                        {
                            if (e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var intVal))
                                return Enum.IsDefined(enumType, intVal) ? Enum.ToObject(enumType, intVal) : null;
                            if (e.ValueKind == JsonValueKind.String && Enum.TryParse(enumType, e.GetString(), true, out var enumObj))
                                return enumObj;
                            return null;
                        })
                        .Where(v => v != null)
                        .ToArray();

                    if (enumValues.Any())
                        query = query.Where($"@0.Contains({key})", enumValues);
                }
                else if (json.ValueKind == JsonValueKind.String)
                {
                    var strVal = json.GetString();
                    if (Enum.TryParse(enumType, strVal, true, out var enumVal))
                        query = query.Where($"{key} == @0", enumVal);
                    else
                        throw new ArgumentException($"Invalid enum value for '{key}': {strVal}");
                }
                else if (json.ValueKind == JsonValueKind.Number && json.TryGetInt32(out var intEnum))
                {
                    var enumVal = Enum.ToObject(enumType, intEnum);
                    query = query.Where($"{key} == @0", enumVal);
                }
                else
                    throw new ArgumentException($"Unsupported JsonElement type for enum '{key}': {json.ValueKind}");

                return query;
            }

            // 🔸 Simple Json Types
            switch (json.ValueKind)
            {
                case JsonValueKind.String:
                    query = query.Where($"{key} != null && {key}.ToLower().Contains(@0)", json.GetString().ToLower());
                    break;
                case JsonValueKind.Number when json.TryGetInt32(out var intVal):
                    query = query.Where($"{key} == @0", intVal);
                    break;
                case JsonValueKind.Number when json.TryGetSingle(out var floatVal):
                    query = query.Where($"{key} == @0", floatVal);
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    query = query.Where($"{key} == @0", json.GetBoolean());
                    break;
                default:
                    throw new ArgumentException($"Unsupported JsonElement type for '{key}': {json.ValueKind}");
            }

            return query;
        }

        // =======================================
        // 🕒 Time Range Helper
        // =======================================
        private (DateTime from, DateTime to)? GetTimeRange(string? timeReport)
        {
            if (string.IsNullOrEmpty(timeReport))
                return null;

            var now = DateTime.UtcNow;
            return timeReport.ToLower() switch
            {
                "daily" => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
                "weekly" => (
                    now.Date.AddDays(-(int)now.DayOfWeek + 1),
                    now.Date.AddDays(7 - (int)now.DayOfWeek).AddDays(1).AddTicks(-1)
                ),
                "monthly" => (
                    new DateTime(now.Year, now.Month, 1),
                    new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)).AddDays(1).AddTicks(-1)
                ),
                "yearly" => (
                    new DateTime(now.Year, 1, 1),
                    new DateTime(now.Year, 12, 31).AddDays(1).AddTicks(-1)
                ),
                _ => null
            };
        }
        
         private string NormalizePropertyName(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;

            var normalized = key.Trim();

            var lower = normalized.ToLowerInvariant();

            // Handle pattern navigasi "floorplan.id" → cari "floorplanid"
            if (lower.EndsWith(".id"))
            {
                var candidate = lower.Replace(".", "");
                var match = typeof(TModel).GetProperties()
                    .FirstOrDefault(p => string.Equals(p.Name, candidate, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                    return match.Name;
            }

            // Remove dots to match joined property names like floorplanid
            if (lower.Contains("."))
                lower = lower.Replace(".", "");

            // Find best match in model
            var propMatch = typeof(TModel).GetProperties()
                .FirstOrDefault(p => string.Equals(p.Name, lower, StringComparison.OrdinalIgnoreCase));

            return propMatch?.Name ?? key;
        }
    }
}






