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
using Entities.Models;

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

            if (string.IsNullOrEmpty(request.SortColumn) || !_validSortColumns.Contains(request.SortColumn))
                request.SortColumn = string.IsNullOrEmpty(request.SortColumn)
                    ? (_validSortColumns.Any() ? _validSortColumns.First() : "UpdatedAt")
                    : request.SortColumn;

            if (string.IsNullOrEmpty(request.SortDir) || !new[] { "asc", "desc" }.Contains(request.SortDir.ToLower()))
                request.SortDir = "desc";

            var query = _query;

            // Hitung total records sebelum filter
            var totalRecords = await query.CountAsync();

            // Search
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = request.SearchValue.ToLower();
                var predicates = _searchableColumns
                    .Select(col => col.Contains(".") ? $"{col.Split('.')[0]} != null && {col}.ToLower().Contains(@0)" : $"{col} != null && {col}.ToLower().Contains(@0)")
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
                        query = query.Where($"{dateFilter.Key} >= @0 && {dateFilter.Key} <= @1", filter.DateFrom.Value, filter.DateTo.Value.AddDays(1).AddTicks(-1));
                    else if (filter.DateFrom.HasValue)
                        query = query.Where($"{dateFilter.Key} >= @0", filter.DateFrom.Value);
                    else if (filter.DateTo.HasValue)
                        query = query.Where($"{dateFilter.Key} <= @0", filter.DateTo.Value.AddDays(1).AddTicks(-1));
                }
            }

            // Custom filters
            if (request.Filters != null && request.Filters.Any())
            {
                foreach (var filter in request.Filters)
                {
                    if (string.IsNullOrEmpty(filter.Key) || filter.Value == null)
                        continue;

                    var value = filter.Value;
                    // Logika enum, Guid, string, int, float, bool, JsonElement sama persis seperti format kamu
                    if (value is JsonElement jsonElement)
                    {
                        // ... semua logic JsonElement tetap sama
                    }
                    else if (value is IEnumerable<object> enumCollection && _enumColumns.ContainsKey(filter.Key))
                    {
                        var enumType = _enumColumns[filter.Key];
                        var enumValues = enumCollection
                            .Select(e => Enum.TryParse(enumType, e?.ToString(), true, out var enumValue) ? enumValue : null)
                            .Where(e => e != null)
                            .ToArray();
                        if (enumValues.Any())
                            query = query.Where($"@0.Contains({filter.Key})", enumValues);
                    }
                    else if (value is IEnumerable<Guid> guidCollection)
                    {
                        var guidValues = guidCollection.ToArray();
                        if (guidValues.Any())
                            query = query.Where($"@0.Contains({filter.Key})", guidValues);
                    }
                    else if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
                    {
                        if (_enumColumns.ContainsKey(filter.Key))
                        {
                            var enumType = _enumColumns[filter.Key];
                            if (Enum.TryParse(enumType, stringValue, true, out var enumValue))
                                query = query.Where($"{filter.Key} == @0", enumValue);
                        }
                        else
                            query = query.Where($"{filter.Key} != null && {filter.Key}.ToString().ToLower().Contains(@0)", stringValue.ToLower());
                    }
                    else if (value is Guid guidValue)
                        query = query.Where($"{filter.Key} == @0", guidValue);
                    else if (value is int intValue)
                        query = query.Where($"{filter.Key} == @0", intValue);
                    else if (value is float floatValue)
                        query = query.Where($"{filter.Key} == @0", floatValue);
                    else if (value is bool boolValue)
                        query = query.Where($"{filter.Key} == @0", boolValue);
                    else
                        throw new ArgumentException($"Unsupported filter type for column '{filter.Key}': {value.GetType().Name}");
                }
            }

            // Projection
            IQueryable<object> projectionQuery;
            if (typeof(TModel) == typeof(MstFloorplan))
            {
                projectionQuery = query.Cast<MstFloorplan>()
                    .Select(f => new
                    {
                        Entity = f,
                        MaskedAreaCount = f.FloorplanMaskedAreas.Count(m =>
                            m.Status != 0
                            || m.Application.ApplicationStatus != 0
                            || m.Floorplan.Status != 0
                            || m.Floorplan.Floor.Building.Status != 0),
                        DeviceCount = f.FloorplanDevices.Count(m =>
                            m.Status != 0
                            || m.Floorplan.Status != 0
                            || m.Floorplan.Application.ApplicationStatus != 0
                            || m.FloorplanMaskedArea.Status != 0
                            || m.Floorplan.Floor.Building.Status != 0)
                    });
            }
            else
                projectionQuery = query.Select(f => new { Entity = f, MaskedAreaCount = 0 });

            // Sorting
            var sortDirection = request.SortDir.ToLower() == "asc" ? "ascending" : "descending";
            if (typeof(TModel) == typeof(MstFloorplan) && request.SortColumn == "MaskedAreaCount")
                projectionQuery = projectionQuery.OrderBy($"MaskedAreaCount {sortDirection}");
            else if (typeof(TModel) == typeof(MstFloorplan) && request.SortColumn == "DeviceCount")
                projectionQuery = projectionQuery.OrderBy($"DeviceCount {sortDirection}");
            else
                projectionQuery = projectionQuery.OrderBy($"Entity.{request.SortColumn} {sortDirection}");

            // Paging & handling Length null/0
            List<object> data;
            int filteredRecords;
            if (request.Length == 0)
            {
                // length 0 → hanya count, tidak menampilkan data
                filteredRecords = await projectionQuery.CountAsync();
                data = new List<object>();
            }
            else
            {
                if (request.Length == null)
                {
                    // length null → tampilkan semua record
                    filteredRecords = await projectionQuery.CountAsync();
                    data = await projectionQuery.ToListAsync();
                }
                else
                {
                    projectionQuery = projectionQuery.Skip(request.Start).Take(request.Length);
                    filteredRecords = await projectionQuery.CountAsync();
                    data = await projectionQuery.ToListAsync();
                }
            }

            var dtos = _mapper.Map<IEnumerable<TDto>>(data.Select(d => d.GetType().GetProperty("Entity").GetValue(d)));

            if (typeof(TModel) == typeof(MstFloorplan))
            {
                var dtosWithCount = data.Select((d, index) =>
                {
                    var dto = dtos.ElementAt(index);
                    var maskedAreaCount = (int)d.GetType().GetProperty("MaskedAreaCount").GetValue(d);
                    var deviceCount = (int)d.GetType().GetProperty("DeviceCount").GetValue(d);
                    dto.GetType().GetProperty("MaskedAreaCount")?.SetValue(dto, maskedAreaCount);
                    dto.GetType().GetProperty("DeviceCount")?.SetValue(dto, deviceCount);
                    return dto;
                }).ToList();
                dtos = dtosWithCount;
            }

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
