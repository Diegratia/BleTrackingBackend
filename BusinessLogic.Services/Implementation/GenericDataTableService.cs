
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
            //comment ini untuk 0 record
            if (request.Length == 0)
            {
                request.Length = await _query.CountAsync();
            }
            // if (request.Length < 1)
            //     throw new ArgumentException("Length must be greater than or equal to 1.");
            if (request.Start < 0)
                throw new ArgumentException("Start cannot be negative.");
            //         if (string.IsNullOrEmpty(request.SortColumn) || !_validSortColumns.Contains(request.SortColumn, StringComparer.OrdinalIgnoreCase))
            // request.SortColumn = string.IsNullOrEmpty(request.SortColumn) ? (_validSortColumns.Any() ? _validSortColumns.First() : "UpdatedAt") : request.SortColumn;
            if (string.IsNullOrEmpty(request.SortColumn) || !_validSortColumns.Contains(request.SortColumn))
                request.SortColumn = string.IsNullOrEmpty(request.SortColumn) ? (_validSortColumns.Any() ? _validSortColumns.First() : "UpdatedAt") : request.SortColumn;
            if (string.IsNullOrEmpty(request.SortDir) || !new[] { "asc", "desc" }.Contains(request.SortDir.ToLower()))
                request.SortDir = "desc";

            var query = _query;

            // var totalRecords = await query.CountAsync();

            // Calculate total records before filtering - comment ini untuk 0 record
            var totalRecords = await query.CountAsync();
            // Console.WriteLine($"Total records before filtering: {totalRecords}");

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



            // 🧩 Apply Custom Filters
            if (request.Filters != null && request.Filters.Any())
            {
                foreach (var filter in request.Filters)
                {
                    // ⛔ Skip jika key kosong
                    if (string.IsNullOrEmpty(filter.Key))
                        continue;

                    var key = filter.Key;
                    var value = filter.Value;

                    // ⛔ Skip jika value null
                    if (value == null)
                        continue;

                    // =========================
                    // 1️⃣ Handle JsonElement Value
                    // =========================
                    if (value is JsonElement json)
                    {
                        // 🔹 Handle GUID / ID filters
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
                                    var prop = typeof(TModel).GetProperty(key);
                                    var isNullableGuid = prop != null && prop.PropertyType == typeof(Guid?);

                                    if (key.Contains('.'))
                                    {
                                        // Navigasi property
                                        var parent = key.Split('.')[0];
                                        query = query.Where($"{parent} != null && @0.Contains({key})", guids);
                                    }
                                    else
                                    {
                                        // Field langsung
                                        query = isNullableGuid
                                            ? query.Where($"{key} != null && @0.Contains({key}.Value)", guids)
                                            : query.Where($"@0.Contains({key})", guids);
                                    }
                                }
                            }

                            // if (json.ValueKind == JsonValueKind.Array)
                            // {
                            //     var guids = json.EnumerateArray()
                            //         .Select(e => Guid.TryParse(e.GetString(), out var g) ? g : (Guid?)null)
                            //         .Where(g => g.HasValue)
                            //         .Select(g => g.Value)
                            //         .ToArray();

                            //     if (guids.Any())
                            //         query = query.Where($"@0.Contains({key})", guids);
                            // }
                            // else if (json.ValueKind == JsonValueKind.String && Guid.TryParse(json.GetString(), out var guidVal))
                            // {
                            //     query = query.Where($"{key} == @0", guidVal);
                            // }
                            // else
                            // {
                            //     var strVal = json.GetString();
                            //     query = query.Where($"{key} != null && {key}.ToLower().Contains(@0)", strVal.ToLower());
                            // }

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

                                        if (e.ValueKind == JsonValueKind.String &&
                                            Enum.TryParse(enumType, e.GetString(), true, out var enumObj))
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
                            {
                                throw new ArgumentException($"Unsupported JsonElement type for enum column '{key}': {json.ValueKind}");
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

                    // =========================
                    // 2️⃣ Handle Non-JsonElement Value
                    // =========================

                    // 🔹 Enum Collections
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

                    // 🔹 GUID Collections
                    if (value is IEnumerable<Guid> guidCollection)
                    {
                        var guids = guidCollection.ToArray();
                        if (guids.Any())
                            query = query.Where($"@0.Contains({key})", guids);

                        continue;
                    }

                    // 🔹 String values
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

                    // 🔹 Simple value types
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



            // Total after filter
            // var filteredRecords = await query.CountAsync();




            // Proyeksi sementara
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
            {
                projectionQuery = query.Select(f => new { Entity = f, MaskedAreaCount = 0 });
            }

            // Hitung recordsTotal dan recordsFiltered dari projectionQuery sebelum paging - uncomment ini untuk 0 recordsFiltered
            // var totalRecords = await projectionQuery.CountAsync();

            // Sorting
            var sortDirection = request.SortDir.ToLower() == "asc" ? "ascending" : "descending";
            if (typeof(TModel) == typeof(MstFloorplan) && request.SortColumn == "MaskedAreaCount")
            {
                projectionQuery = projectionQuery.OrderBy($"MaskedAreaCount {sortDirection}");
            }
            if (typeof(TModel) == typeof(MstFloorplan) && request.SortColumn == "DeviceCount")
            {
                projectionQuery = projectionQuery.OrderBy($"DeviceCount {sortDirection}");
            }
            else
            {
                projectionQuery = projectionQuery.OrderBy($"Entity.{request.SortColumn} {sortDirection}");
            }


            projectionQuery = projectionQuery.Skip(request.Start).Take(request.Length);
            // var filteredRecords = await projectionQuery.CountAsync();
            var filteredRecords = query.Count();

            // === MODE COUNT ===
            if (string.Equals(request.Mode, "count", StringComparison.OrdinalIgnoreCase))
            {
                return new
                {
                    draw = request.Draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = filteredRecords
                };
            }

            // === MODE TABLE (default) ===
            query = query.OrderBy($"{request.SortColumn} {sortDirection}");

            // Paging
            if (request.Length > 0)
            {
                projectionQuery = projectionQuery.Skip(request.Start).Take(request.Length);
            }

            // Execute query
            var data = await projectionQuery.ToListAsync();
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

// public async Task<object> FilterAsync(DataTablesRequest request)
// {
//     if (request.Start < 0)
//         throw new ArgumentException("Start cannot be negative.");

//     if (string.IsNullOrEmpty(request.SortColumn) || !_validSortColumns.Contains(request.SortColumn))
//         request.SortColumn = string.IsNullOrEmpty(request.SortColumn)
//             ? (_validSortColumns.Any() ? _validSortColumns.First() : "UpdatedAt")
//             : request.SortColumn;

//     if (string.IsNullOrEmpty(request.SortDir) || !new[] { "asc", "desc" }.Contains(request.SortDir.ToLower()))
//         request.SortDir = "desc";

//     var query = _query;

//     // Hitung total records sebelum filter
//     var totalRecords = await query.CountAsync();

//     // Search
//     if (!string.IsNullOrEmpty(request.SearchValue))
//     {
//         var search = request.SearchValue.ToLower();
//         var predicates = _searchableColumns
//             .Select(col => col.Contains(".") ? $"{col.Split('.')[0]} != null && {col}.ToLower().Contains(@0)" : $"{col} != null && {col}.ToLower().Contains(@0)")
//             .Aggregate((current, next) => $"{current} || {next}");
//         query = query.Where(predicates, search);
//     }

//     // Date filter
//     if (request.DateFilters != null && request.DateFilters.Any())
//     {
//         foreach (var dateFilter in request.DateFilters)
//         {
//             if (string.IsNullOrEmpty(dateFilter.Key) || !_validSortColumns.Contains(dateFilter.Key))
//                 throw new ArgumentException($"Invalid date column: {dateFilter.Key}");

//             var filter = dateFilter.Value;
//             if (filter.DateFrom.HasValue && filter.DateTo.HasValue)
//                 query = query.Where($"{dateFilter.Key} >= @0 && {dateFilter.Key} <= @1", filter.DateFrom.Value, filter.DateTo.Value.AddDays(1).AddTicks(-1));
//             else if (filter.DateFrom.HasValue)
//                 query = query.Where($"{dateFilter.Key} >= @0", filter.DateFrom.Value);
//             else if (filter.DateTo.HasValue)
//                 query = query.Where($"{dateFilter.Key} <= @0", filter.DateTo.Value.AddDays(1).AddTicks(-1));
//         }
//     }

//     // Custom filters
//     if (request.Filters != null && request.Filters.Any())
//     {
//         foreach (var filter in request.Filters)
//         {
//             if (string.IsNullOrEmpty(filter.Key) || filter.Value == null)
//                 continue;

//             var value = filter.Value;
//             // Logika enum, Guid, string, int, float, bool, JsonElement sama persis seperti format kamu
//             if (value is JsonElement jsonElement)
//             {
//                 // ... semua logic JsonElement tetap sama
//             }
//             else if (value is IEnumerable<object> enumCollection && _enumColumns.ContainsKey(filter.Key))
//             {
//                 var enumType = _enumColumns[filter.Key];
//                 var enumValues = enumCollection
//                     .Select(e => Enum.TryParse(enumType, e?.ToString(), true, out var enumValue) ? enumValue : null)
//                     .Where(e => e != null)
//                     .ToArray();
//                 if (enumValues.Any())
//                     query = query.Where($"@0.Contains({filter.Key})", enumValues);
//             }
//             else if (value is IEnumerable<Guid> guidCollection)
//             {
//                 var guidValues = guidCollection.ToArray();
//                 if (guidValues.Any())
//                     query = query.Where($"@0.Contains({filter.Key})", guidValues);
//             }
//             else if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
//             {
//                 if (_enumColumns.ContainsKey(filter.Key))
//                 {
//                     var enumType = _enumColumns[filter.Key];
//                     if (Enum.TryParse(enumType, stringValue, true, out var enumValue))
//                         query = query.Where($"{filter.Key} == @0", enumValue);
//                 }
//                 else
//                     query = query.Where($"{filter.Key} != null && {filter.Key}.ToString().ToLower().Contains(@0)", stringValue.ToLower());
//             }
//             else if (value is Guid guidValue)
//                 query = query.Where($"{filter.Key} == @0", guidValue);
//             else if (value is int intValue)
//                 query = query.Where($"{filter.Key} == @0", intValue);
//             else if (value is float floatValue)
//                 query = query.Where($"{filter.Key} == @0", floatValue);
//             else if (value is bool boolValue)
//                 query = query.Where($"{filter.Key} == @0", boolValue);
//             else
//                 throw new ArgumentException($"Unsupported filter type for column '{filter.Key}': {value.GetType().Name}");
//         }
//     }

//     // Projection
//     IQueryable<object> projectionQuery;
//     if (typeof(TModel) == typeof(MstFloorplan))
//     {
//         projectionQuery = query.Cast<MstFloorplan>()
//             .Select(f => new
//             {
//                 Entity = f,
//                 MaskedAreaCount = f.FloorplanMaskedAreas.Count(m =>
//                     m.Status != 0
//                     || m.Application.ApplicationStatus != 0
//                     || m.Floorplan.Status != 0
//                     || m.Floorplan.Floor.Building.Status != 0),
//                 DeviceCount = f.FloorplanDevices.Count(m =>
//                     m.Status != 0
//                     || m.Floorplan.Status != 0
//                     || m.Floorplan.Application.ApplicationStatus != 0
//                     || m.FloorplanMaskedArea.Status != 0
//                     || m.Floorplan.Floor.Building.Status != 0)
//             });
//     }
//     else
//         projectionQuery = query.Select(f => new { Entity = f, MaskedAreaCount = 0 });

//     // Sorting
//     var sortDirection = request.SortDir.ToLower() == "asc" ? "ascending" : "descending";
//     if (typeof(TModel) == typeof(MstFloorplan) && request.SortColumn == "MaskedAreaCount")
//         projectionQuery = projectionQuery.OrderBy($"MaskedAreaCount {sortDirection}");
//     else if (typeof(TModel) == typeof(MstFloorplan) && request.SortColumn == "DeviceCount")
//         projectionQuery = projectionQuery.OrderBy($"DeviceCount {sortDirection}");
//     else
//         projectionQuery = projectionQuery.OrderBy($"Entity.{request.SortColumn} {sortDirection}");

//     // Paging & handling Length null/0
//     List<object> data;
//     int filteredRecords;
//     if (request.Length == 0)
//     {
//         // length 0 → hanya count, tidak menampilkan data
//         filteredRecords = await projectionQuery.CountAsync();
//         data = new List<object>();
//     }
//     else
//     {
//         if (request.Length == null)
//         {
//             // length null → tampilkan semua record
//             filteredRecords = await projectionQuery.CountAsync();
//             data = await projectionQuery.ToListAsync();
//         }
//         else
//         {
//             projectionQuery = projectionQuery.Skip(request.Start).Take(request.Length);
//             filteredRecords = await projectionQuery.CountAsync();
//             data = await projectionQuery.ToListAsync();
//         }
//     }

//     var dtos = _mapper.Map<IEnumerable<TDto>>(data.Select(d => d.GetType().GetProperty("Entity").GetValue(d)));

//     if (typeof(TModel) == typeof(MstFloorplan))
//     {
//         var dtosWithCount = data.Select((d, index) =>
//         {
//             var dto = dtos.ElementAt(index);
//             var maskedAreaCount = (int)d.GetType().GetProperty("MaskedAreaCount").GetValue(d);
//             var deviceCount = (int)d.GetType().GetProperty("DeviceCount").GetValue(d);
//             dto.GetType().GetProperty("MaskedAreaCount")?.SetValue(dto, maskedAreaCount);
//             dto.GetType().GetProperty("DeviceCount")?.SetValue(dto, deviceCount);
//             return dto;
//         }).ToList();
//         dtos = dtosWithCount;
//     }

//     return new
//     {
//         draw = request.Draw,
//         recordsTotal = totalRecords,
//         recordsFiltered = filteredRecords,
//         data = dtos
//     };
// }

















/// NOTES GENERIC
/// 
///             // // Custom filters
            // if (request.Filters != null && request.Filters.Any())
            // {
            //     foreach (var filter in request.Filters)
            //     {
            //         if (string.IsNullOrEmpty(filter.Key))
            //             continue;


            //         var value = filter.Value;
            //         // if (value == null || value.ToString() == "empty")

            //         // return new
            //         // {
            //         //     draw = request.Draw,
            //         //     recordsTotal = totalRecords,
            //         //     recordsFiltered = 0,
            //         //     data = "",
            //         // };
            //         if (value == null)
            //             continue;

            //         if (value is JsonElement jsonElement)
            //         {
            //             if (filter.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
            //             {
            //                 // Handle single Guid or array of Guids
            //                 if (jsonElement.ValueKind == JsonValueKind.Array)
            //                 {
            //                     var guidValues = jsonElement.EnumerateArray()
            //                         .Select(e => Guid.TryParse(e.GetString(), out var guid) ? guid : (Guid?)null)
            //                         .Where(g => g.HasValue)
            //                         .Select(g => g.Value)
            //                         .ToArray();
            //                     if (guidValues.Any())
            //                     {
            //                         query = query.Where($"@0.Contains({filter.Key})", guidValues);
            //                     }
            //                 }
            //                 else if (jsonElement.ValueKind == JsonValueKind.String && Guid.TryParse(jsonElement.GetString(), out var guidValue))
            //                 {
            //                     query = query.Where($"{filter.Key} == @0", guidValue);
            //                 }
            //                 else
            //                 {
            //                     var stringValue = jsonElement.GetString();
            //                     query = query.Where($"{filter.Key} != null && {filter.Key}.ToLower().Contains(@0)", stringValue.ToLower());
            //                 }
            //             }

            //             else if (_enumColumns.ContainsKey(filter.Key))
            //             {
            //                 var enumType = _enumColumns[filter.Key];
            //                 if (jsonElement.ValueKind == JsonValueKind.Array)
            //                 {
            //                     var enumValues = jsonElement.EnumerateArray()
            //                         .Select(e =>
            //                         {
            //                             if (e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var intVal))
            //                             {
            //                                 return Enum.IsDefined(enumType, intVal) ? Enum.ToObject(enumType, intVal) : null;
            //                             }
            //                             else if (e.ValueKind == JsonValueKind.String && Enum.TryParse(enumType, e.GetString(), true, out var enumObj))
            //                             {
            //                                 return enumObj;
            //                             }
            //                             return null;
            //                         })
            //                         .Where(v => v != null)
            //                         .ToArray();

            //                     if (enumValues.Any())
            //                     {
            //                         query = query.Where($"@0.Contains({filter.Key})", enumValues);
            //                     }
            //                 }
            //                 else if (jsonElement.ValueKind == JsonValueKind.String)
            //                 {
            //                     var stringValue = jsonElement.GetString();
            //                     if (Enum.TryParse(enumType, stringValue, true, out var enumValue))
            //                     {
            //                         query = query.Where($"{filter.Key} == @0", enumValue);
            //                     }
            //                     else
            //                     {
            //                         throw new ArgumentException($"Invalid enum value for column '{filter.Key}': {stringValue}");
            //                     }
            //                 }
            //                 else if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out var intEnumVal))
            //                 {
            //                     var enumValue = Enum.ToObject(enumType, intEnumVal);
            //                     query = query.Where($"{filter.Key} == @0", enumValue);
            //                 }
            //                 else
            //                 {
            //                     throw new ArgumentException($"Unsupported JsonElement type for enum column '{filter.Key}': {jsonElement.ValueKind}");
            //                 }
            //             }
            //             else if (jsonElement.ValueKind == JsonValueKind.String)
            //             {
            //                 query = query.Where($"{filter.Key} != null && {filter.Key}.ToString().ToLower().Contains(@0)", jsonElement.GetString().ToLower());
            //             }
            //             else if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out var intValue))
            //             {
            //                 query = query.Where($"{filter.Key} == @0", intValue);
            //             }
            //             else if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetSingle(out var floatValue))
            //             {
            //                 query = query.Where($"{filter.Key} == @0", floatValue);
            //             }
            //             else if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
            //             {
            //                 query = query.Where($"{filter.Key} == @0", jsonElement.GetBoolean());
            //             }
            //             else
            //             {
            //                 throw new ArgumentException($"Unsupported JsonElement type for column '{filter.Key}': {jsonElement.ValueKind}");
            //             }
            //         }
            //         else if (value is IEnumerable<object> enumCollection && _enumColumns.ContainsKey(filter.Key))
            //         {
            //             var enumType = _enumColumns[filter.Key];
            //             var enumValues = enumCollection
            //                 .Select(e => Enum.TryParse(enumType, e?.ToString(), true, out var enumValue) ? enumValue : null)
            //                 .Where(e => e != null)
            //                 .ToArray();
            //             if (enumValues.Any())
            //             {
            //                 query = query.Where($"@0.Contains({filter.Key})", enumValues);
            //             }
            //         }
            //         else if (value is IEnumerable<Guid> guidCollection)
            //         {
            //             var guidValues = guidCollection.ToArray();
            //             if (guidValues.Any())
            //             {
            //                 query = query.Where($"@0.Contains({filter.Key})", guidValues);
            //             }
            //         }
            //         else if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            //         {
            //             if (_enumColumns.ContainsKey(filter.Key))
            //             {
            //                 var enumType = _enumColumns[filter.Key];
            //                 if (Enum.TryParse(enumType, stringValue, true, out var enumValue))
            //                 {
            //                     query = query.Where($"{filter.Key} == @0", enumValue);
            //                 }
            //                 else
            //                 {
            //                     throw new ArgumentException($"Invalid enum value for column '{filter.Key}': {stringValue}");
            //                 }
            //             }
            //             else
            //             {
            //                 query = query.Where($"{filter.Key} != null && {filter.Key}.ToString().ToLower().Contains(@0)", stringValue.ToLower());
            //             }
            //         }
            //         else if (value is Guid guidValue)
            //         {
            //             query = query.Where($"{filter.Key} == @0", guidValue);
            //         }
            //         else if (value is int intValue)
            //         {
            //             query = query.Where($"{filter.Key} == @0", intValue);
            //         }
            //         else if (value is float floatValue)
            //         {
            //             query = query.Where($"{filter.Key} == @0", floatValue);
            //         }
            //         else if (value is bool boolValue)
            //         {
            //             query = query.Where($"{filter.Key} == @0", boolValue);
            //         }
            //         else
            //         {
            //             throw new ArgumentException($"Unsupported filter type for column '{filter.Key}': {value.GetType().Name}");
            //         }
            //     }
            // }

