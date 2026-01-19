using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using System.Linq.Expressions;

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
                request.SortColumn = _validSortColumns.FirstOrDefault() ?? "UpdatedAt";

            if (string.IsNullOrEmpty(request.SortDir) || !new[] { "asc", "desc" }.Contains(request.SortDir.ToLower()))
                request.SortDir = "desc";

            if (request.Length <= 0 || request.Length > 1000)
                request.Length = 1000;

            // ============================================
            // 1️⃣ Base Query (sudah bisa pakai Include dari Repo)
            // ============================================
            var query = _query.AsNoTracking();

            long totalRecords = await query.LongCountAsync();

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

            // ============================================
            // 2️⃣ Search Global
            // ============================================
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = request.SearchValue.ToLower();
                var predicates = _searchableColumns
                    .Select(col =>
                        col.Contains(".")
                            ? $"{col.Split('.')[0]} != null && {col}.ToLower().Contains(@0)"
                            : $"{col} != null && {col}.ToLower().Contains(@0)")
                    .Aggregate((a, b) => $"{a} || {b}");
                query = query.Where(predicates, search);
            }

            // ============================================
            // 3️⃣ Date Filter
            // ============================================
            if (request.DateFilters != null && request.DateFilters.Any())
            {
                foreach (var df in request.DateFilters)
                {
                    if (string.IsNullOrEmpty(df.Key) || !_validSortColumns.Contains(df.Key))
                        continue;

                    var val = df.Value;
                    if (val.DateFrom.HasValue && val.DateTo.HasValue)
                        query = query.Where($"{df.Key} >= @0 && {df.Key} <= @1",
                            val.DateFrom.Value, val.DateTo.Value.AddDays(1).AddTicks(-1));
                    else if (val.DateFrom.HasValue)
                        query = query.Where($"{df.Key} >= @0", val.DateFrom.Value);
                    else if (val.DateTo.HasValue)
                        query = query.Where($"{df.Key} <= @0", val.DateTo.Value.AddDays(1).AddTicks(-1));
                }
            }

            // ============================================
            // 4️⃣ Custom Filters
            // ============================================
            if (request.Filters != null && request.Filters.Any())
            {
                foreach (var filter in request.Filters)
                {
                    if (string.IsNullOrEmpty(filter.Key) || filter.Value == null)
                        continue;

                    query = ApplyFilter(query, filter.Key, filter.Value);
                }
            }

            long filteredRecords = await query.LongCountAsync();

            // ============================================
            // 5️⃣ Projection (Computed Counts)
            // ============================================
            IQueryable<object> projectionQuery;
            if (typeof(TModel) == typeof(MstFloorplan))
            {
                projectionQuery = query.Cast<MstFloorplan>()
                    .Select(f => new
                    {
                        Entity = f,
                        MaskedAreaCount = f.FloorplanMaskedAreas.Count(m =>
                        m.Status != 0
                        || (m.Application != null && m.Application.ApplicationStatus != 0)
                        || (m.Floorplan != null && m.Floorplan.Status != 0)
                        || (m.Floorplan != null && m.Floorplan.Floor != null && m.Floorplan.Floor.Building.Status != 0)
                    ),
                    DeviceCount = f.FloorplanDevices.Count(m =>
                        m.Status != 0
                        || (m.Floorplan != null && m.Floorplan.Status != 0)
                        || (m.Floorplan != null && m.Floorplan.Application != null && m.Floorplan.Application.ApplicationStatus != 0)
                        || (m.FloorplanMaskedArea != null && m.FloorplanMaskedArea.Status != 0)
                        || (m.Floorplan != null && m.Floorplan.Floor != null && m.Floorplan.Floor.Building.Status != 0)
                    ),
                    PatrolAreaCount = f.PatrolAreas.Count(m =>
                        m.Status != 0
                        || (m.Floorplan != null && m.Floorplan.Status != 0)
                        || (m.Floorplan != null && m.Floorplan.Application != null && m.Floorplan.Application.ApplicationStatus != 0)
                        || (m.Floorplan != null && m.Floorplan.Floor != null && m.Floorplan.Floor.Building.Status != 0)
                    )
                    });
            }
            else
            {
                projectionQuery = query.Select(e => new { Entity = e });
            }

            

            // ============================================
            // 6️⃣ Sorting (Entity / Computed)
            // ============================================
            string sortDir = request.SortDir.ToLower() == "desc" ? "descending" : "ascending";

            try
            {
                if (typeof(TModel) == typeof(MstFloorplan) &&
                    (request.SortColumn == "MaskedAreaCount" || request.SortColumn == "DeviceCount"))
                {
                    projectionQuery = projectionQuery.OrderBy($"{request.SortColumn} {sortDir}");
                }
                else
                {
                    projectionQuery = projectionQuery.OrderBy($"Entity.{request.SortColumn} {sortDir}");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Compare"))
                    projectionQuery = projectionQuery.AsEnumerable().AsQueryable()
                        .OrderBy($"Entity.{request.SortColumn} {sortDir}");
                else
                    throw;
            }

            // ============================================
            // 7️⃣ Paging
            // ============================================
            projectionQuery = projectionQuery.Skip(request.Start).Take(request.Length);

            // ============================================
            // 8️⃣ Eksekusi dan Mapping DTO
            // ============================================
            var data = await projectionQuery.ToListAsync();

            var dtos = _mapper.Map<IEnumerable<TDto>>(
                data.Select(d => d.GetType().GetProperty("Entity")!.GetValue(d))
            );

            // Tambahkan computed property ke DTO
            if (typeof(TModel) == typeof(MstFloorplan))
            {
                var dtosWithCount = data.Select((d, i) =>
                {
                    var dto = dtos.ElementAt(i);
                    var maskedAreaCount = (int)d.GetType().GetProperty("MaskedAreaCount")!.GetValue(d)!;
                    var deviceCount = (int)d.GetType().GetProperty("DeviceCount")!.GetValue(d)!;
                    var patrolAreaCount = (int)d.GetType().GetProperty("PatrolAreaCount")!.GetValue(d)!;
                    dto.GetType().GetProperty("MaskedAreaCount")?.SetValue(dto, maskedAreaCount);
                    dto.GetType().GetProperty("DeviceCount")?.SetValue(dto, deviceCount);
                    dto.GetType().GetProperty("PatrolAreaCount")?.SetValue(dto, patrolAreaCount);
                    return dto;
                }).ToList();
                dtos = dtosWithCount;
            }

            // ============================================
            // 9️⃣ Return
            // ============================================
            return new
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords, // filter dinamis sudah diterapkan sebelumnya
                data = dtos
            };
        }

        // ======================================================
        // 🔹 Helper: Apply Custom Filter (Enum, Guid, JSON, dsb)
        // ======================================================
        // ======================================================
        // 🔹 Filter Handler (termasuk ENUM, fix for enum .ToLower() issue)
        // ======================================================
        private IQueryable<TModel> ApplyFilter(IQueryable<TModel> query, string key, object value)
        {
            key = NormalizePropertyName(key);

            if (string.IsNullOrEmpty(key) || value == null)
                return query;

            var prop = typeof(TModel).GetProperty(key);
            if (prop == null) return query;

            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            // ✅ Enum handler (pakai ApplyEnumFilter)
            if (_enumColumns.ContainsKey(key))
            {
                return ApplyEnumFilter(query, key, _enumColumns[key], value);
            }

            // ✅ JsonElement handler
            if (value is JsonElement json)
                return ApplyJsonFilter(query, key, json);

            // ✅ GUID handler
            if (propType == typeof(Guid))
            {
                if (value is string s && Guid.TryParse(s, out var guidVal))
                    return query.Where($"{key} == @0", guidVal);
                else if (value is Guid g)
                    return query.Where($"{key} == @0", g);
                return query; // skip
            }

            // ✅ Nullable GUID handler
            if (propType == typeof(Guid?))
            {
                if (value is string s && Guid.TryParse(s, out var guidVal))
                    return query.Where($"{key} != null && {key}.Value == @0", guidVal);
                else if (value is Guid g)
                    return query.Where($"{key} != null && {key}.Value == @0", g);
                return query;
            }

            // ✅ String (case-insensitive contains)
            if (propType == typeof(string))
            {
                if (value is string str && !string.IsNullOrEmpty(str))
                    return query.Where($"{key} != null && {key}.ToLower().Contains(@0)", str.ToLower());
                return query;
            }

            // ✅ Integer / Float / Bool langsung == 
            if (value is int i) return query.Where($"{key} == @0", i);
            if (value is float f) return query.Where($"{key} == @0", f);
            if (value is double d) return query.Where($"{key} == @0", d);
            if (value is bool b) return query.Where($"{key} == @0", b);

            return query;
        }


        // ======================================================
        // 🔹 JsonElement Filter Handler (safe against enum type)
        // ======================================================
      private IQueryable<TModel> ApplyJsonFilter(IQueryable<TModel> query, string key, JsonElement json)
{
    key = NormalizePropertyName(key);

    var prop = typeof(TModel).GetProperty(key);
    if (prop == null) return query;

    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
    bool isEnum = propType.IsEnum;

    switch (json.ValueKind)
    {
        // 🔸 STRING VALUE
        case JsonValueKind.String:
            var strVal = json.GetString();
            if (string.IsNullOrEmpty(strVal)) return query;

            // ✅ GUID support
            if (propType == typeof(Guid))
            {
                if (Guid.TryParse(strVal, out var g))
                    return query.Where($"{key} == @0", g);
                return query;
            }
            if (propType == typeof(Guid?))
            {
                if (Guid.TryParse(strVal, out var g2))
                    return query.Where($"{key} != null && {key}.Value == @0", g2);
                return query;
            }

            // ✅ ENUM support
            if (isEnum)
            {
                if (Enum.TryParse(propType, strVal, true, out var parsed))
                    return query.Where($"{key} == @0", parsed);
                throw new ArgumentException($"Invalid enum value for '{key}': {strVal}");
            }

            // ✅ STRING LIKE search
            if (propType == typeof(string))
                return query.Where($"{key} != null && {key}.ToLower().Contains(@0)", strVal.ToLower());

            return query;

        // 🔸 NUMBER VALUE
        case JsonValueKind.Number:
            if (json.TryGetInt32(out var i))
                return query.Where($"{key} == @0", i);
            if (json.TryGetDouble(out var d))
                return query.Where($"{key} == @0", d);
            break;

        // 🔸 BOOL VALUE
        case JsonValueKind.True:
        case JsonValueKind.False:
            return query.Where($"{key} == @0", json.GetBoolean());

        // 🔸 ARRAY VALUE
        case JsonValueKind.Array:
            if (isEnum)
            {
                var enumValues = json.EnumerateArray()
                    .Select(x =>
                    {
                        if (x.ValueKind == JsonValueKind.String && Enum.TryParse(propType, x.GetString(), true, out var e))
                            return e;
                        if (x.ValueKind == JsonValueKind.Number && x.TryGetInt32(out var n) && Enum.IsDefined(propType, n))
                            return Enum.ToObject(propType, n);
                        throw new ArgumentException($"Invalid enum value for '{key}': {x}");
                    })
                    .ToArray();
                return query.Where($"@0.Contains({key})", enumValues);
            }

            // ✅ GUID ARRAY
            if (propType == typeof(Guid) || propType == typeof(Guid?))
            {
                var guids = json.EnumerateArray()
                    .Select(x => Guid.TryParse(x.GetString(), out var g) ? g : (Guid?)null)
                    .Where(g => g.HasValue)
                    .Select(g => g.Value)
                    .ToArray();
                if (guids.Any())
                    return query.Where($"@0.Contains({key})", guids);
            }

            // ✅ STRING ARRAY
            var strs = json.EnumerateArray().Select(x => x.GetString()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            if (propType == typeof(string) && strs.Any())
                return query.Where($"@0.Contains({key})", strs);

            break;
    }

    return query;
}


        // ======================================================
        // 🔹 Enum Filter Handler
        // ======================================================
        private IQueryable<TModel> ApplyEnumFilter(IQueryable<TModel> query, string key, Type enumType, object value)
        {
            if (value == null || value is JsonElement { ValueKind: JsonValueKind.Null })
                return query;

            // --- STEP 1: Parse input ke List<object> ---
            var parsedList = new List<object>();

            void AddEnumValue(object input)
            {
                if (input == null) return;

                if (int.TryParse(input.ToString(), out var intVal))
                {
                    if (Enum.IsDefined(enumType, intVal))
                        parsedList.Add(Enum.ToObject(enumType, intVal));
                    else
                        throw new ArgumentException($"Invalid enum value for '{key}': {intVal}");
                    return;
                }

                if (Enum.TryParse(enumType, input.ToString(), true, out var parsed))
                {
                    parsedList.Add(parsed);
                }
                else
                {
                    throw new ArgumentException($"Invalid enum value for '{key}': {input}");
                }
            }

            // --- STEP 2: Handle berbagai tipe input ---
            switch (value)
            {
                case JsonElement json:
                    if (json.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in json.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String)
                                AddEnumValue(item.GetString());
                            else if (item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out var num))
                                AddEnumValue(num);
                        }
                    }
                    else if (json.ValueKind == JsonValueKind.String)
                        AddEnumValue(json.GetString());
                    else if (json.ValueKind == JsonValueKind.Number && json.TryGetInt32(out var singleNum))
                        AddEnumValue(singleNum);
                    break;

                case string s:
                    AddEnumValue(s);
                    break;

                case int i:
                    AddEnumValue(i);
                    break;

                case IEnumerable<object> arr:
                    foreach (var obj in arr)
                        AddEnumValue(obj);
                    break;

                default:
                    AddEnumValue(value);
                    break;
            }

            if (!parsedList.Any())
                return query;

            // --- STEP 3: Buat array enum dengan tipe kuat ---
            var enumArray = Array.CreateInstance(enumType, parsedList.Count);
            for (int i = 0; i < parsedList.Count; i++)
                enumArray.SetValue(parsedList[i], i);

            // --- STEP 4: Buat Expression Tree untuk: e => enumArray.Contains(e.key) ---
            var parameter = Expression.Parameter(typeof(TModel), "e");
            Expression property = Expression.PropertyOrField(parameter, key);

            // Handle nullable enum (DeviceStatus?)
            if (Nullable.GetUnderlyingType(property.Type) != null)
                property = Expression.Convert(property, enumType);

            var constant = Expression.Constant(enumArray);
            var containsMethod = typeof(Enumerable)
                .GetMethods()
                .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                .MakeGenericMethod(enumType);

            var body = Expression.Call(containsMethod, constant, property);
            var lambda = Expression.Lambda<Func<TModel, bool>>(body, parameter);

            // --- STEP 5: Apply ke IQueryable ---
            return query.Where(lambda);
        }

        private string NormalizePropertyName(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;

            var normalized = key.Trim();

            // Case-insensitive normalization
            var lower = normalized.ToLowerInvariant();

            // 🔹 Handle pattern navigasi "floorplan.id" → cari "floorplanid"
            if (lower.EndsWith(".id"))
            {
                var candidate = lower.Replace(".", "");
                var match = typeof(TModel).GetProperties()
                    .FirstOrDefault(p => string.Equals(p.Name, candidate, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                    return match.Name;
            }

            // 🔹 Remove dots to match joined property names like floorplanid
            if (lower.Contains("."))
                lower = lower.Replace(".", "");

            // 🔹 Find best match in model
            var propMatch = typeof(TModel).GetProperties()
                .FirstOrDefault(p => string.Equals(p.Name, lower, StringComparison.OrdinalIgnoreCase));

            return propMatch?.Name ?? key;
        }

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






//safe generic

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
// using Entities.Models;

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

//             if (string.IsNullOrEmpty(request.SortColumn) || !_validSortColumns.Contains(request.SortColumn))
//                 request.SortColumn = string.IsNullOrEmpty(request.SortColumn)
//                     ? (_validSortColumns.Any() ? _validSortColumns.First() : "UpdatedAt")
//                     : request.SortColumn;

//             if (string.IsNullOrEmpty(request.SortDir) || !new[] { "asc", "desc" }.Contains(request.SortDir.ToLower()))
//                 request.SortDir = "desc";
            
//             if (request.Length <= 0 || request.Length > 1000)
//                 request.Length = 1000;

//             var query = _query.AsNoTracking();

//             // Hitung total records sebelum filter
//             long totalRecords = await query.LongCountAsync();

//             // Apply TimeReport jika tidak CustomDate
//             if (request.DateFilters != null && request.DateFilters.Any())
//                 request.TimeReport = "CustomDate";

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

//             // Search
//             if (!string.IsNullOrEmpty(request.SearchValue))
//             {
//                 var search = request.SearchValue.ToLower();
//                 var predicates = _searchableColumns
//                     .Select(col => col.Contains(".") ? $"{col.Split('.')[0]} != null && {col}.ToLower().Contains(@0)" : $"{col} != null && {col}.ToLower().Contains(@0)")
//                     .Aggregate((current, next) => $"{current} || {next}");
//                 query = query.Where(predicates, search);
//             }

//             // Date filter
//             if (request.DateFilters != null && request.DateFilters.Any())
//             {
//                 foreach (var dateFilter in request.DateFilters)
//                 {
//                     if (string.IsNullOrEmpty(dateFilter.Key) || !_validSortColumns.Contains(dateFilter.Key))
//                         throw new ArgumentException($"Invalid date column: {dateFilter.Key}");

//                     var filter = dateFilter.Value;
//                     if (filter.DateFrom.HasValue && filter.DateTo.HasValue)
//                         query = query.Where($"{dateFilter.Key} >= @0 && {dateFilter.Key} <= @1", filter.DateFrom.Value, filter.DateTo.Value.AddDays(1).AddTicks(-1));
//                     else if (filter.DateFrom.HasValue)
//                         query = query.Where($"{dateFilter.Key} >= @0", filter.DateFrom.Value);
//                     else if (filter.DateTo.HasValue)
//                         query = query.Where($"{dateFilter.Key} <= @0", filter.DateTo.Value.AddDays(1).AddTicks(-1));
//                 }
//             }

//             // Custom filters
//             if (request.Filters != null && request.Filters.Any())
//             {
//                 foreach (var filter in request.Filters)
//                 {
//                     if (string.IsNullOrEmpty(filter.Key) || filter.Value == null)
//                         continue;

//                     var value = filter.Value;
//                     // Logika enum, Guid, string, int, float, bool, JsonElement sama persis seperti format kamu
//                     if (value is JsonElement jsonElement)
//                     {
//                         // ... semua logic JsonElement tetap sama
//                     }
//                     else if (value is IEnumerable<object> enumCollection && _enumColumns.ContainsKey(filter.Key))
//                     {
//                         var enumType = _enumColumns[filter.Key];
//                         var enumValues = enumCollection
//                             .Select(e => Enum.TryParse(enumType, e?.ToString(), true, out var enumValue) ? enumValue : null)
//                             .Where(e => e != null)
//                             .ToArray();
//                         if (enumValues.Any())
//                             query = query.Where($"@0.Contains({filter.Key})", enumValues);
//                     }
//                     else if (value is IEnumerable<Guid> guidCollection)
//                     {
//                         var guidValues = guidCollection.ToArray();
//                         if (guidValues.Any())
//                             query = query.Where($"@0.Contains({filter.Key})", guidValues);
//                     }
//                     else if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
//                     {
//                         if (_enumColumns.ContainsKey(filter.Key))
//                         {
//                             var enumType = _enumColumns[filter.Key];
//                             if (Enum.TryParse(enumType, stringValue, true, out var enumValue))
//                                 query = query.Where($"{filter.Key} == @0", enumValue);
//                         }
//                         else
//                             query = query.Where($"{filter.Key} != null && {filter.Key}.ToString().ToLower().Contains(@0)", stringValue.ToLower());
//                     }
//                     else if (value is Guid guidValue)
//                         query = query.Where($"{filter.Key} == @0", guidValue);
//                     else if (value is int intValue)
//                         query = query.Where($"{filter.Key} == @0", intValue);
//                     else if (value is float floatValue)
//                         query = query.Where($"{filter.Key} == @0", floatValue);
//                     else if (value is bool boolValue)
//                         query = query.Where($"{filter.Key} == @0", boolValue);
//                     else
//                         throw new ArgumentException($"Unsupported filter type for column '{filter.Key}': {value.GetType().Name}");
//                 }
//             }

//             // Projection
//             IQueryable<object> projectionQuery;
//             if (typeof(TModel) == typeof(MstFloorplan))
//             {
//                 projectionQuery = query.Cast<MstFloorplan>()
//                     .Select(f => new
//                     {
//                         Entity = f,
//                         MaskedAreaCount = f.FloorplanMaskedAreas.Count(m =>
//                             m.Status != 0
//                             || m.Application.ApplicationStatus != 0
//                             || m.Floorplan.Status != 0
//                             || m.Floorplan.Floor.Building.Status != 0),
//                         DeviceCount = f.FloorplanDevices.Count(m =>
//                             m.Status != 0
//                             || m.Floorplan.Status != 0
//                             || m.Floorplan.Application.ApplicationStatus != 0
//                             || m.FloorplanMaskedArea.Status != 0
//                             || m.Floorplan.Floor.Building.Status != 0)
//                     });
//             }
//             else
//                 projectionQuery = query.Select(f => new { Entity = f, MaskedAreaCount = 0 });

//             // Sorting
//             var sortDirection = request.SortDir.ToLower() == "asc" ? "ascending" : "descending";
//             if (typeof(TModel) == typeof(MstFloorplan) && request.SortColumn == "MaskedAreaCount")
//                 projectionQuery = projectionQuery.OrderBy($"MaskedAreaCount {sortDirection}");
//             else if (typeof(TModel) == typeof(MstFloorplan) && request.SortColumn == "DeviceCount")
//                 projectionQuery = projectionQuery.OrderBy($"DeviceCount {sortDirection}");
//             else
//                 projectionQuery = projectionQuery.OrderBy($"Entity.{request.SortColumn} {sortDirection}");

//             // Paging & handling Length null/0
//             List<object> data;
//             int filteredRecords;
//             if (request.Length == 0)
//             {
//                 // length 0 → hanya count, tidak menampilkan data
//                 filteredRecords = await projectionQuery.CountAsync();
//                 data = new List<object>();
//             }
//             else
//             {
//                 if (request.Length == null)
//                 {
//                     // length null → tampilkan semua record
//                     filteredRecords = await projectionQuery.CountAsync();
//                     data = await projectionQuery.ToListAsync();
//                 }
//                 else
//                 {
//                     projectionQuery = projectionQuery.Skip(request.Start).Take(request.Length);
//                     filteredRecords = await projectionQuery.CountAsync();
//                     data = await projectionQuery.ToListAsync();
//                 }
//             }

//             var dtos = _mapper.Map<IEnumerable<TDto>>(data.Select(d => d.GetType().GetProperty("Entity").GetValue(d)));

//             if (typeof(TModel) == typeof(MstFloorplan))
//             {
//                 var dtosWithCount = data.Select((d, index) =>
//                 {
//                     var dto = dtos.ElementAt(index);
//                     var maskedAreaCount = (int)d.GetType().GetProperty("MaskedAreaCount").GetValue(d);
//                     var deviceCount = (int)d.GetType().GetProperty("DeviceCount").GetValue(d);
//                     dto.GetType().GetProperty("MaskedAreaCount")?.SetValue(dto, maskedAreaCount);
//                     dto.GetType().GetProperty("DeviceCount")?.SetValue(dto, deviceCount);
//                     return dto;
//                 }).ToList();
//                 dtos = dtosWithCount;
//             }

//             return new
//             {
//                 draw = request.Draw,
//                 recordsTotal = totalRecords,
//                 recordsFiltered = filteredRecords,
//                 data = dtos
//             };
//         }


//          private IQueryable<TModel> ApplyJsonFilter(IQueryable<TModel> query, string key, JsonElement json)
//         {
//             // 🔸 Handle GUID/ID filters
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
//                     {
//                         var prop = typeof(TModel).GetProperty(key.Contains('.') ? key.Split('.').Last() : key);
//                         var isNullableGuid = prop != null && prop.PropertyType == typeof(Guid?);
//                         if (key.Contains('.'))
//                         {
//                             var parent = key.Split('.')[0];
//                             query = query.Where($"{parent} != null && @0.Contains({key})", guids);
//                         }
//                         else
//                         {
//                             query = isNullableGuid
//                                 ? query.Where($"{key} != null && @0.Contains({key}.Value)", guids)
//                                 : query.Where($"@0.Contains({key})", guids);
//                         }
//                     }
//                 }
//                 else if (json.ValueKind == JsonValueKind.String && Guid.TryParse(json.GetString(), out var guidVal))
//                 {
//                     var prop = typeof(TModel).GetProperty(key.Contains('.') ? key.Split('.').Last() : key);
//                     var isNullableGuid = prop != null && prop.PropertyType == typeof(Guid?);
//                     if (key.Contains('.'))
//                     {
//                         var parent = key.Split('.')[0];
//                         query = query.Where($"{parent} != null && {key} == @0", guidVal);
//                     }
//                     else
//                     {
//                         query = isNullableGuid
//                             ? query.Where($"{key} != null && {key}.Value == @0", guidVal)
//                             : query.Where($"{key} == @0", guidVal);
//                     }
//                 }
//                 else
//                 {
//                     var strVal = json.GetString();
//                     query = query.Where($"{key} != null && {key}.ToLower().Contains(@0)", strVal?.ToLower());
//                 }
//                 return query;
//             }

//             // 🔸 Enum Columns
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
//                     else
//                         throw new ArgumentException($"Invalid enum value for '{key}': {strVal}");
//                 }
//                 else if (json.ValueKind == JsonValueKind.Number && json.TryGetInt32(out var intEnum))
//                 {
//                     var enumVal = Enum.ToObject(enumType, intEnum);
//                     query = query.Where($"{key} == @0", enumVal);
//                 }
//                 else
//                     throw new ArgumentException($"Unsupported JsonElement type for enum '{key}': {json.ValueKind}");

//                 return query;
//             }

//             // 🔸 Simple Json Types
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
//                 default:
//                     throw new ArgumentException($"Unsupported JsonElement type for '{key}': {json.ValueKind}");
//             }

//             return query;
//         }

//             private (DateTime from, DateTime to)? GetTimeRange(string? timeReport)
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
