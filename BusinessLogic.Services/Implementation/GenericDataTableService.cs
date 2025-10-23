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

namespace BusinessLogic.Services.Implementation
{
    public class GenericDataTableService<TModel, TDto>
        where TModel : class
        where TDto : class
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
            if (request.Start < 0)
                throw new ArgumentException("Start cannot be negative.");

            if (request.Length <= 0 || request.Length > 10000)
            {
                request.Length = 10000; // batas maksimum data sekali fetch
            }

            // 🔹 Sort column validation
            if (string.IsNullOrEmpty(request.SortColumn) || !_validSortColumns.Contains(request.SortColumn))
                request.SortColumn = _validSortColumns.FirstOrDefault() ?? "UpdatedAt";

            if (string.IsNullOrEmpty(request.SortDir) || !new[] { "asc", "desc" }.Contains(request.SortDir.ToLower()))
                request.SortDir = "desc";

            // 🔹 Base query (no tracking untuk hemat memory)
            var query = _query.AsNoTracking();

            // =======================================
            // 🔸 1. Total records (sebelum filter)
            // =======================================
            long totalRecords = await query.LongCountAsync();

            // =======================================
            // 🔸 2. Apply TimeReport
            // =======================================
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
            // 🔸 3. Apply Search
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
            // 🔸 4. Apply DateFilters & Custom Filters
            // =======================================
            query = ApplyDateAndCustomFilters(query, request);

            // =======================================
            // 🔸 5. Count filtered records
            // =======================================
            long filteredRecords = await query.LongCountAsync();

            // === MODE COUNT ONLY ===
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
            // 🔸 6. Sorting & Paging
            // =======================================
            query = query.OrderBy($"{request.SortColumn} {request.SortDir}");
            query = query.Skip(request.Start).Take(request.Length);

            // =======================================
            // 🔸 7. Hybrid Projection / Mapping
            // =======================================
            List<TDto> data;

            if (typeof(TModel) == typeof(TDto))
            {
                // ✅ Query sudah projection ke DTO → langsung gunakan
                data = await query.Cast<TDto>().ToListAsync();
            }
            else
            {
                try
                {
                    // ✅ Gunakan ProjectTo (SQL projection, hemat memory)
                    data = await query.ProjectTo<TDto>(_mapper.ConfigurationProvider).ToListAsync();
                }
                catch
                {
                    // ⚠️ Fallback ke mapping in-memory jika tidak bisa di-ProjectTo
                    var entities = await query.ToListAsync();
                    data = _mapper.Map<List<TDto>>(entities);
                }
            }

            // =======================================
            // 🔸 8. Return Response
            // =======================================
            return new
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data
            };
        }

        // =======================================
        // 🔹 Apply DateFilter & Custom Filter
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

                    if (value is JsonElement json)
                    {
                        query = ApplyJsonFilter(query, key, json);
                        continue;
                    }

                    // 🔸 Enum Collections
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

                    // 🔸 GUID Collections
                    if (value is IEnumerable<Guid> guidCollection)
                    {
                        var guids = guidCollection.ToArray();
                        if (guids.Any())
                            query = query.Where($"@0.Contains({key})", guids);

                        continue;
                    }

                    // 🔸 String
                    if (value is string str && !string.IsNullOrEmpty(str))
                    {
                        if (_enumColumns.ContainsKey(key))
                        {
                            var enumType = _enumColumns[key];
                            if (Enum.TryParse(enumType, str, true, out var enumVal))
                                query = query.Where($"{key} == @0", enumVal);
                        }
                        else
                        {
                            query = query.Where($"{key} != null && {key}.ToString().ToLower().Contains(@0)", str.ToLower());
                        }

                        continue;
                    }

                    // 🔸 Simple types
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
                    }
                }
            }

            return query;
        }

        // =======================================
        // 🔹 Apply JSON Element Filter
        // =======================================
        private IQueryable<TModel> ApplyJsonFilter(IQueryable<TModel> query, string key, JsonElement json)
        {
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
                        query = query.Where($"@0.Contains({key})", guids);
                }
                else if (json.ValueKind == JsonValueKind.String && Guid.TryParse(json.GetString(), out var guidVal))
                {
                    query = query.Where($"{key} == @0", guidVal);
                }
                else
                {
                    var strVal = json.GetString();
                    query = query.Where($"{key} != null && {key}.ToLower().Contains(@0)", strVal?.ToLower());
                }
                return query;
            }

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
                return query;
            }

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
            }

            return query;
        }

        // =======================================
        // 🔹 Time Range Preset
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
    }
}





// using AutoMapper;
// using AutoMapper.QueryableExtensions;
// using Microsoft.EntityFrameworkCore;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Linq.Dynamic.Core;
// using System.Text.Json;
// using System.Threading.Tasks;
// using Data.ViewModels;

// namespace BusinessLogic.Services.Implementation
// {
//     public class GenericDataTableService<TModel, TDto>
//         where TModel : class
//         where TDto : class
//     {
//         private readonly IQueryable<TModel> _query;
//         private readonly IMapper _mapper;
//         private readonly string[] _searchableColumns;
//         private readonly string[] _validSortColumns;
//         private readonly Dictionary<string, Type> _enumColumns;

//         public GenericDataTableService(
//             IQueryable<TModel> query,
//             IMapper mapper,
//             string[] searchableColumns,
//             string[] validSortColumns,
//             Dictionary<string, Type> enumColumns = null)
//         {
//             _query = query ?? throw new ArgumentNullException(nameof(query));
//             _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
//             _searchableColumns = searchableColumns ?? throw new ArgumentNullException(nameof(searchableColumns));
//             _validSortColumns = validSortColumns ?? throw new ArgumentNullException(nameof(validSortColumns));
//             _enumColumns = enumColumns ?? new Dictionary<string, Type>();
//         }

//         public async Task<object> FilterAsync(DataTablesRequest request)
//         {
//             if (request.Start < 0)
//                 throw new ArgumentException("Start cannot be negative.");
//             if (request.Length <= 0 || request.Length > 10000)
//             {
//                 request.Length = 10000; // batasi maksimum data sekali fetch
//             }

//             // 🔹 Sort column validation
//             if (string.IsNullOrEmpty(request.SortColumn) || !_validSortColumns.Contains(request.SortColumn))
//                 request.SortColumn = _validSortColumns.FirstOrDefault() ?? "UpdatedAt";

//             if (string.IsNullOrEmpty(request.SortDir) || !new[] { "asc", "desc" }.Contains(request.SortDir.ToLower()))
//                 request.SortDir = "desc";

//             // 🔹 Base query (no tracking to save memory)
//             var query = _query.AsNoTracking();

//             // =======================================
//             // 🔸 1. Total records (before filtering)
//             // =======================================
//             long totalRecords = await query.LongCountAsync();

//             // =======================================
//             // 🔸 2. Apply TimeReport
//             // =======================================
//             if (!string.IsNullOrEmpty(request.TimeReport) && request.TimeReport != "CustomDate")
//             {
//                 var timeRange = GetTimeRange(request.TimeReport);
//                 if (timeRange.HasValue)
//                 {
//                     var timeColumn = _validSortColumns.Contains(request.SortColumn)
//                         ? request.SortColumn
//                         : "UpdatedAt";

//                     query = query.Where($"{timeColumn} >= @0 && {timeColumn} <= @1",
//                         timeRange.Value.from, timeRange.Value.to);
//                 }
//             }

//             // =======================================
//             // 🔸 3. Apply Search
//             // =======================================
//             if (!string.IsNullOrEmpty(request.SearchValue))
//             {
//                 var search = request.SearchValue.ToLower();
//                 var predicates = _searchableColumns
//                     .Select(col =>
//                         col.Contains(".")
//                             ? $"{col.Split('.')[0]} != null && {col}.ToLower().Contains(@0)"
//                             : $"{col} != null && {col}.ToLower().Contains(@0)")
//                     .Aggregate((current, next) => $"{current} || {next}");

//                 query = query.Where(predicates, search);
//             }

//             // =======================================
//             // 🔸 4. Apply DateFilters & Custom Filters
//             // =======================================
//             query = ApplyDateAndCustomFilters(query, request);

//             // =======================================
//             // 🔸 5. Count filtered records (lightweight)
//             // =======================================
//             long filteredRecords = await query.LongCountAsync();

//             // === MODE COUNT ONLY ===
//             if (string.Equals(request.Mode, "count", StringComparison.OrdinalIgnoreCase))
//             {
//                 return new
//                 {
//                     draw = request.Draw,
//                     recordsTotal = totalRecords,
//                     recordsFiltered = filteredRecords
//                 };
//             }

//             // =======================================
//             // 🔸 6. Sorting & Paging
//             // =======================================
//             query = query.OrderBy($"{request.SortColumn} {request.SortDir}");
//             query = query.Skip(request.Start).Take(request.Length);

//             // =======================================
//             // 🔸 7. Projection langsung ke DTO (hemat memory)
//             // =======================================
//             var data = await query
//                 .ProjectTo<TDto>(_mapper.ConfigurationProvider)
//                 .ToListAsync();

//             // =======================================
//             // 🔸 8. Return Response
//             // =======================================
//             return new
//             {
//                 draw = request.Draw,
//                 recordsTotal = totalRecords,
//                 recordsFiltered = filteredRecords,
//                 data
//             };
//         }

//         // =======================================
//         // 🔹 Apply DateFilter & Custom Filter
//         // =======================================
//         private IQueryable<TModel> ApplyDateAndCustomFilters(IQueryable<TModel> query, DataTablesRequest request)
//         {
//             // 🔹 Date Filters
//             if (request.DateFilters != null && request.DateFilters.Any())
//             {
//                 foreach (var dateFilter in request.DateFilters)
//                 {
//                     if (string.IsNullOrEmpty(dateFilter.Key) || !_validSortColumns.Contains(dateFilter.Key))
//                         continue;

//                     var filter = dateFilter.Value;
//                     if (filter.DateFrom.HasValue && filter.DateTo.HasValue)
//                         query = query.Where($"{dateFilter.Key} >= @0 && {dateFilter.Key} <= @1",
//                             filter.DateFrom.Value, filter.DateTo.Value.AddDays(1).AddTicks(-1));
//                     else if (filter.DateFrom.HasValue)
//                         query = query.Where($"{dateFilter.Key} >= @0", filter.DateFrom.Value);
//                     else if (filter.DateTo.HasValue)
//                         query = query.Where($"{dateFilter.Key} <= @0", filter.DateTo.Value.AddDays(1).AddTicks(-1));
//                 }
//             }

//             // 🔹 Custom Filters
//             if (request.Filters != null && request.Filters.Any())
//             {
//                 foreach (var filter in request.Filters)
//                 {
//                     if (string.IsNullOrEmpty(filter.Key) || filter.Value == null)
//                         continue;

//                     var key = filter.Key;
//                     var value = filter.Value;

//                     if (value is JsonElement json)
//                     {
//                         query = ApplyJsonFilter(query, key, json);
//                         continue;
//                     }

//                     // 🔸 Enum Collections
//                     if (value is IEnumerable<object> enumCollection && _enumColumns.ContainsKey(key))
//                     {
//                         var enumType = _enumColumns[key];
//                         var enums = enumCollection
//                             .Select(e => Enum.TryParse(enumType, e?.ToString(), true, out var parsed) ? parsed : null)
//                             .Where(e => e != null)
//                             .ToArray();

//                         if (enums.Any())
//                             query = query.Where($"@0.Contains({key})", enums);

//                         continue;
//                     }

//                     // 🔸 GUID Collections
//                     if (value is IEnumerable<Guid> guidCollection)
//                     {
//                         var guids = guidCollection.ToArray();
//                         if (guids.Any())
//                             query = query.Where($"@0.Contains({key})", guids);

//                         continue;
//                     }

//                     // 🔸 String
//                     if (value is string str && !string.IsNullOrEmpty(str))
//                     {
//                         if (_enumColumns.ContainsKey(key))
//                         {
//                             var enumType = _enumColumns[key];
//                             if (Enum.TryParse(enumType, str, true, out var enumVal))
//                                 query = query.Where($"{key} == @0", enumVal);
//                         }
//                         else
//                         {
//                             query = query.Where($"{key} != null && {key}.ToString().ToLower().Contains(@0)", str.ToLower());
//                         }

//                         continue;
//                     }

//                     // 🔸 Simple types
//                     switch (value)
//                     {
//                         case Guid guidVal:
//                             query = query.Where($"{key} == @0", guidVal);
//                             break;
//                         case int intVal:
//                             query = query.Where($"{key} == @0", intVal);
//                             break;
//                         case float floatVal:
//                             query = query.Where($"{key} == @0", floatVal);
//                             break;
//                         case bool boolVal:
//                             query = query.Where($"{key} == @0", boolVal);
//                             break;
//                     }
//                 }
//             }

//             return query;
//         }

//         // =======================================
//         // 🔹 Apply JSON Element Filter
//         // =======================================
//         private IQueryable<TModel> ApplyJsonFilter(IQueryable<TModel> query, string key, JsonElement json)
//         {
//             if (key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
//             {
//                 if (json.ValueKind == JsonValueKind.Array)
//                 {
//                     var guids = json.EnumerateArray()
//                         .Select(e => Guid.TryParse(e.GetString(), out var g) ? g : (Guid?)null)
//                         .Where(g => g.HasValue)
//                         .Select(g => g.Value)
//                         .ToArray();
//                     if (guids.Any())
//                         query = query.Where($"@0.Contains({key})", guids);
//                 }
//                 else if (json.ValueKind == JsonValueKind.String && Guid.TryParse(json.GetString(), out var guidVal))
//                 {
//                     query = query.Where($"{key} == @0", guidVal);
//                 }
//                 else
//                 {
//                     var strVal = json.GetString();
//                     query = query.Where($"{key} != null && {key}.ToLower().Contains(@0)", strVal?.ToLower());
//                 }
//                 return query;
//             }

//             if (_enumColumns.ContainsKey(key))
//             {
//                 var enumType = _enumColumns[key];
//                 if (json.ValueKind == JsonValueKind.Array)
//                 {
//                     var enumValues = json.EnumerateArray()
//                         .Select(e =>
//                         {
//                             if (e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var intVal))
//                                 return Enum.IsDefined(enumType, intVal) ? Enum.ToObject(enumType, intVal) : null;
//                             if (e.ValueKind == JsonValueKind.String && Enum.TryParse(enumType, e.GetString(), true, out var enumObj))
//                                 return enumObj;
//                             return null;
//                         })
//                         .Where(v => v != null)
//                         .ToArray();

//                     if (enumValues.Any())
//                         query = query.Where($"@0.Contains({key})", enumValues);
//                 }
//                 else if (json.ValueKind == JsonValueKind.String)
//                 {
//                     var strVal = json.GetString();
//                     if (Enum.TryParse(enumType, strVal, true, out var enumVal))
//                         query = query.Where($"{key} == @0", enumVal);
//                 }
//                 else if (json.ValueKind == JsonValueKind.Number && json.TryGetInt32(out var intEnum))
//                 {
//                     var enumVal = Enum.ToObject(enumType, intEnum);
//                     query = query.Where($"{key} == @0", enumVal);
//                 }
//                 return query;
//             }

//             switch (json.ValueKind)
//             {
//                 case JsonValueKind.String:
//                     query = query.Where($"{key} != null && {key}.ToLower().Contains(@0)", json.GetString().ToLower());
//                     break;
//                 case JsonValueKind.Number when json.TryGetInt32(out var intVal):
//                     query = query.Where($"{key} == @0", intVal);
//                     break;
//                 case JsonValueKind.Number when json.TryGetSingle(out var floatVal):
//                     query = query.Where($"{key} == @0", floatVal);
//                     break;
//                 case JsonValueKind.True:
//                 case JsonValueKind.False:
//                     query = query.Where($"{key} == @0", json.GetBoolean());
//                     break;
//             }

//             return query;
//         }

//         // =======================================
//         // 🔹 Time Range Preset
//         // =======================================
//         private (DateTime from, DateTime to)? GetTimeRange(string? timeReport)
//         {
//             if (string.IsNullOrEmpty(timeReport))
//                 return null;

//             var now = DateTime.UtcNow;
//             return timeReport.ToLower() switch
//             {
//                 "daily" => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
//                 "weekly" => (
//                     now.Date.AddDays(-(int)now.DayOfWeek + 1),
//                     now.Date.AddDays(7 - (int)now.DayOfWeek).AddDays(1).AddTicks(-1)
//                 ),
//                 "monthly" => (
//                     new DateTime(now.Year, now.Month, 1),
//                     new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)).AddDays(1).AddTicks(-1)
//                 ),
//                 "yearly" => (
//                     new DateTime(now.Year, 1, 1),
//                     new DateTime(now.Year, 12, 31).AddDays(1).AddTicks(-1)
//                 ),
//                 _ => null
//             };
//         }
//     }
// }
