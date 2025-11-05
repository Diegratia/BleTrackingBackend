using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Threading.Tasks;
using Helpers.Consumer;
using Repositories.DbContexts;
using Microsoft.EntityFrameworkCore.Query;
using System.Text.Json;
using Repositories.Models;

namespace Repositories.Repository
{
    public abstract class BaseProjectionRepository<TModel, TRm> : BaseRepository
        where TModel : class
        where TRm : class
    {
        protected readonly DbSet<TModel> _dbSet;

        protected BaseProjectionRepository(BleTrackingDbContext context, IHttpContextAccessor accessor)
            : base(context, accessor)
        {
            _dbSet = _context.Set<TModel>();
        }

        // =====================================================
        // ✅ 1️⃣ Abstract projection implementation
        // =====================================================
        protected abstract IQueryable<TRm> Project(IQueryable<TModel> query);

        // =====================================================
        // ✅ 2️⃣ Base Query dengan filter ApplicationId & Status
        // =====================================================
        protected virtual IQueryable<TModel> GetBaseQuery(bool includeDeleted = false)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _dbSet.AsNoTracking().AsQueryable();

            // Jika entitas punya kolom Status, filter soft delete
            if (!includeDeleted && typeof(TModel).GetProperty("Status") != null)
            {
                query = query.Where("Status != 0");
            }

            // Filter ApplicationId (hanya jika entity implement IApplicationEntity)
            if (typeof(IApplicationEntity).IsAssignableFrom(typeof(TModel)))
            {
                query = ApplyApplicationIdFilter((IQueryable<IApplicationEntity>)query, applicationId, isSystemAdmin)
                    .Cast<TModel>();
            }

            return query;
        }

        // =====================================================
        // ✅ 3️⃣ Apply Entity Filter
        // =====================================================
        protected virtual IQueryable<TModel> ApplyEntityFilters(
            IQueryable<TModel> query,
            Dictionary<string, object> filters)
        {

            // Console.WriteLine("[DEBUG] === ApplyEntityFilters called ===");
            // Console.WriteLine("SQL: " + query.ToQueryString());

            // Console.WriteLine("[DEBUG] Filters: " + JsonSerializer.Serialize(filters));

            if (filters == null || filters.Count == 0)
                return query;

            var props = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var f in filters)
            {
                // Console.WriteLine("SQL: " + query.ToQueryString());

                // Console.WriteLine($"[DEBUG] Key: {f.Key}, Type: {f.Value?.GetType().Name}, Value: {f.Value}");
            }


            foreach (var f in filters)
            {
                var key = f.Key?.Trim();
                var val = f.Value?.ToString()?.Trim();

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(val))
                    continue;

                var prop = props.FirstOrDefault(p => string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase));
                if (prop == null)
                    continue;

                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                try
                {
                    if (type.IsEnum)
                    {
                        var enumList = new List<object>();

                        if (f.Value is JsonElement json)
                        {
                            if (json.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var el in json.EnumerateArray())
                                {
                                    switch (el.ValueKind)
                                    {
                                        case JsonValueKind.String:
                                            var strVal = el.GetString();
                                            if (Enum.TryParse(type, strVal, true, out var parsedStr))
                                                enumList.Add(parsedStr);
                                            break;

                                        case JsonValueKind.Number:
                                            if (el.TryGetInt32(out var numVal))
                                            {
                                                var parsedEnum = Enum.ToObject(type, numVal);
                                                enumList.Add(parsedEnum);
                                            }
                                            break;
                                    }
                                }
                            }
                            else if (json.ValueKind == JsonValueKind.String)
                            {
                                var strVal = json.GetString();
                                if (Enum.TryParse(type, strVal, true, out var parsedStr))
                                    enumList.Add(parsedStr);
                            }
                            else if (json.ValueKind == JsonValueKind.Number)
                            {
                                if (json.TryGetInt32(out var numVal))
                                {
                                    var parsedEnum = Enum.ToObject(type, numVal);
                                    enumList.Add(parsedEnum);
                                }
                            }
                        }
                        else if (f.Value is string sVal)
                        {
                            if (Enum.TryParse(type, sVal, true, out var parsed))
                                enumList.Add(parsed);
                        }
                        else if (f.Value is int intVal)
                        {
                            var parsedEnum = Enum.ToObject(type, intVal);
                            enumList.Add(parsedEnum);
                        }

                        // ✅ Apply filter
                        if (enumList.Count == 1)
                        {
                            query = query.Where($"{prop.Name} == @0", enumList.First());
                            // Console.WriteLine($"[DEBUG] Enum single filter applied: {prop.Name} == {enumList.First()}");
                        }
                        else if (enumList.Count > 1)
                        {
                            // Multiple → build OR expression
                            var conditions = new List<string>();
                            for (int i = 0; i < enumList.Count; i++)
                                conditions.Add($"{prop.Name} == @{i}");

                            var whereExp = string.Join(" OR ", conditions);
                            query = query.Where(whereExp, enumList.ToArray());
                            // Console.WriteLine($"[DEBUG] Enum multi filter applied: {whereExp}");
                        }

                        continue;
                    }



                    if (type == typeof(Guid))
                    {
                        // ✅ Case 1: array of GUID (multiple filter)
                        if (f.Value is JsonElement json && json.ValueKind == JsonValueKind.Array)
                        {
                            var guidList = new List<Guid>();
                            foreach (var el in json.EnumerateArray())
                            {
                                if (el.ValueKind == JsonValueKind.String && Guid.TryParse(el.GetString(), out var gid))
                                    guidList.Add(gid);
                            }

                            if (guidList.Count > 0)
                            {
                                // build: ReaderId IN (...)
                                query = query.Where($"@0.Contains({prop.Name})", guidList);
                            }

                            continue;
                        }

                        // ✅ Case 2: single GUID (string)
                        if (Guid.TryParse(val, out var singleGuid))
                        {
                            query = query.Where($"{prop.Name} == @0", singleGuid);
                            continue;
                        }
                    }


                    if (type == typeof(string))
                    {
                        query = query.Where($"{prop.Name} != null && {prop.Name}.ToLower().Contains(@0)", val.ToLower());
                        continue;
                    }

                    if (type == typeof(bool) && bool.TryParse(val, out var boolVal))
                    {
                        query = query.Where($"{prop.Name} == @0", boolVal);
                        continue;
                    }

                    if (decimal.TryParse(val, out var num))
                    {
                        query = query.Where($"{prop.Name} == @0", Convert.ChangeType(num, type));
                        continue;
                    }

                    if (DateTime.TryParse(val, out var date))
                    {
                        query = query.Where($"{prop.Name} >= @0 && {prop.Name} < @1", date.Date, date.Date.AddDays(1));
                        continue;
                    }

                }
                catch
                {
                    // skip error parsing
                    continue;
                }
            }

            return query;
        }

        // =====================================================
        // ✅ 4️⃣ Apply Post-projection filter (in-memory)
        // =====================================================
        // protected virtual IQueryable<TRm> ApplyPostProjectionFilters(
        //     IQueryable<TRm> query,
        //     Dictionary<string, object> filters)
        // {
        //     if (filters == null || filters.Count == 0)
        //         return query;

        //     var props = typeof(TRm).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //     var data = query.AsEnumerable();

        //     foreach (var f in filters)
        //     {
        //         var prop = props.FirstOrDefault(p => string.Equals(p.Name, f.Key, StringComparison.OrdinalIgnoreCase));
        //         if (prop == null) continue;

        //         var val = f.Value?.ToString()?.ToLower();
        //         if (string.IsNullOrEmpty(val)) continue;

        //         data = data.Where(x =>
        //         {
        //             var value = prop.GetValue(x)?.ToString()?.ToLower();
        //             return value != null && value.Contains(val);
        //         });
        //     }
        //     return data.AsQueryable();
        // }

        // =====================================================
        // ✅ 5️⃣ Entry utama untuk filtering projection
        // =====================================================
        public virtual IQueryable<TRm> GetFilteredProjection(Dictionary<string, object> filters)
        {
            var query = GetBaseQuery();

            query = ApplyEntityFilters(query, filters);
            return Project(query);
        }

        // =====================================================
        // ✅ 6️⃣ Paging, Sorting, dan Count
        // =====================================================
        // public virtual async Task<(List<TRm> Data, long TotalCount)> GetPagedResultAsync(
        //     Dictionary<string, object> filters,
        //     string sortColumn = "UpdatedAt",
        //     string sortDir = "desc",
        //     int start = 0,
        //     int length = 10)
        // {
        //     var query = GetFilteredProjection(filters);

        //     // Sorting
        //     if (!string.IsNullOrEmpty(sortColumn))
        //         query = query.OrderBy($"{sortColumn} {sortDir}");

        //     var total = await query.LongCountAsync();

        //     var data = await query.Skip(start).Take(length).ToListAsync();

        //     return (data, total);
        // }



        public virtual async Task<(List<TRm> Data, long TotalRecords, long FilteredRecords)> GetPagedResultAsync(
        Dictionary<string, object> filters,
        Dictionary<string, DateRangeFilter>? dateFilters = null,
        string? timeReport = null,
        string sortColumn = "UpdatedAt",
        string sortDir = "desc",
        int start = 0,
        int length = 10)
        {
            // Step 1️⃣ - Base query tanpa filter
            var baseQuery = GetBaseQuery();

            long totalRecords = await baseQuery.LongCountAsync();

            // Step 2️⃣ - Apply filter di level entity (SQL-safe)
            var filteredQuery = ApplyEntityFilters(baseQuery, filters);

            filteredQuery = ApplyDateFilters(filteredQuery, dateFilters);


            // Step 3️⃣ - Projection ke RM
            var projectedQuery = Project(filteredQuery);

            // Step 4️⃣ - Apply post-projection filters (non-SQL-safe)
            // projectedQuery = ApplyPostProjectionFilters(projectedQuery, filters);

            // Step 5️⃣ - Hitung jumlah record setelah semua filter
            long filteredRecords;
            if (projectedQuery.Provider is IAsyncQueryProvider)
            {
                filteredRecords = await projectedQuery.LongCountAsync();
            }
            else
            {
                filteredRecords = projectedQuery.LongCount();
            }

            // Step 6️⃣ - Sorting (default ke UpdatedAt desc)
            if (string.IsNullOrEmpty(sortColumn))
                sortColumn = "UpdatedAt";

            if (string.IsNullOrEmpty(sortDir) || !new[] { "asc", "desc" }.Contains(sortDir.ToLower()))
                sortDir = "desc";

            projectedQuery = projectedQuery.OrderBy($"{sortColumn} {sortDir}");

            // Step 7️⃣ - Paging
            if (length <= 0)
                length = 10;

            List<TRm> data;
            if (projectedQuery.Provider is IAsyncQueryProvider)
            {
                data = await projectedQuery.Skip(start).Take(length).ToListAsync();
            }
            else
            {
                data = projectedQuery.Skip(start).Take(length).ToList();
            }
            // Step 8️⃣ - Return result lengkap
            return (data, totalRecords, filteredRecords);
        }

        /// <summary>
/// Menerapkan filter rentang tanggal ke query berdasarkan dictionary dateFilters.
/// Mendukung kombinasi DateFrom / DateTo pada banyak kolom tanggal.
/// </summary>
protected virtual IQueryable<TModel> ApplyDateFilters(
    IQueryable<TModel> query,
    Dictionary<string, DateRangeFilter>? dateFilters,
    string? timeReport = null,
    string defaultTimeColumn = "UpdatedAt")
{
    if ((dateFilters == null || dateFilters.Count == 0) && string.IsNullOrEmpty(timeReport))
        return query;

    // ✅ override ke CustomDate
    if (dateFilters != null && dateFilters.Any())
        timeReport = "CustomDate";

    // ✅ 1. Handle custom date range
    if (timeReport == "CustomDate")
    {
        foreach (var df in dateFilters!)
        {
            var key = df.Key;
            var val = df.Value;
            if (val == null || (val.DateFrom == null && val.DateTo == null))
                continue;

            var prop = typeof(TModel).GetProperty(key, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                continue;

            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            if (propType != typeof(DateTime))
                continue;

            // 🧩 hitung nilai di luar query supaya EF tidak error
            var from = val.DateFrom?.ToUniversalTime();
            var to = val.DateTo?.ToUniversalTime();

            if (from.HasValue && to.HasValue)
            {
                query = query.Where($"{key} >= @0 && {key} <= @1", from.Value, to.Value);
                Console.WriteLine($"[DEBUG] Filter tanggal {key}: {from} - {to}");
            }
            else if (from.HasValue)
            {
                query = query.Where($"{key} >= @0", from.Value);
            }
            else if (to.HasValue)
            {
                query = query.Where($"{key} <= @0", to.Value);
            }
        }
        return query;
    }

    // ✅ 2. Handle preset (daily, weekly, monthly, yearly)
    if (!string.IsNullOrEmpty(timeReport))
    {
        var range = GetTimeRange(timeReport);
                if (range.HasValue)
                {
                    var from = range.Value.from;
                    var to = range.Value.to;

                    var prop = typeof(TModel).GetProperty(defaultTimeColumn, BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                    {
                        query = query.Where($"{defaultTimeColumn} >= @0 && {defaultTimeColumn} <= @1", from, to);
                        Console.WriteLine($"[DEBUG] TimeReport '{timeReport}' applied: {from} - {to}");
                    }
            if (prop == null)
{
    Console.WriteLine($"[WARN] Property '{defaultTimeColumn}' not found on {typeof(TModel).Name}");
    return query;
}
        }
    }

    return query;
}



protected virtual (DateTime from, DateTime to)? GetTimeRange(string? timeReport)
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
