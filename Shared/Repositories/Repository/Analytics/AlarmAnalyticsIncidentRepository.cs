using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Repository.RepoModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository.Analytics
{

    public class AlarmAnalyticsIncidentRepository : BaseRepository
    {
        private readonly BleTrackingDbContext _context;

        public AlarmAnalyticsIncidentRepository(BleTrackingDbContext context, IHttpContextAccessor accessor)
            : base(context, accessor)
        {
            _context = context;
        }

        // total alarm per area
        // public async Task<List<AlarmAreaSummaryRM>> GetAreaSummaryAsync(AlarmAnalyticsRequestRM request)
        // {
        //     var range = GetTimeRange(request.TimeRange ?? "weekly");
        //     var (from, to) = (range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-7), range?.to ?? request.To ?? DateTime.UtcNow);
        //     var query = _context.AlarmRecordTrackings
        //         .AsNoTracking()
        //         .Include(a => a.FloorplanMaskedArea)
        //         .Where(a => a.Timestamp >= from && a.Timestamp <= to);

        //     query = ApplyFilters(query, request);

        //     var data = await query
        //         .Select(a => new
        //         {
        //             a.AlarmTriggersId,
        //             a.FloorplanMaskedAreaId,
        //             AreaName = a.FloorplanMaskedArea.Name,
        //             AlarmStatus = a.Alarm
        //         })
        //         .Distinct()
        //         .GroupBy(x => new
        //         {
        //             x.FloorplanMaskedAreaId,
        //             x.AreaName,
        //             x.AlarmStatus
        //         })
        //         .Select(g => new AlarmAreaSummaryRM
        //         {
        //             AreaId = g.Key.FloorplanMaskedAreaId,
        //             AreaName = g.Key.AreaName ?? "Unknown",
        //             AlarmStatus = g.Key.AlarmStatus.ToString() ?? "Unknown",
        //             Total = g.Count()
        //         })
        //         .ToListAsync();

        //     return data;
        // }

        public async Task<List<AlarmAreaDailyAggregateRM>> GetAreaDailySummaryAsync(
            AlarmAnalyticsRequestRM request
        )
        {
            var range = GetTimeRange(request.TimeRange);
            var from = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-7);
            var to   = range?.to   ?? request.To   ?? DateTime.UtcNow;

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.FloorplanMaskedArea)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            return await query
                .Where(a => a.Timestamp.HasValue)  
                .GroupBy(a => new
                {
                    Date = a.Timestamp.Value.Date,
                    a.FloorplanMaskedAreaId,
                    AreaName = a.FloorplanMaskedArea.Name,
                    AlarmStatus = a.Alarm
                })
                .Select(g => new AlarmAreaDailyAggregateRM
                {
                    Date = g.Key.Date,
                    AreaId = g.Key.FloorplanMaskedAreaId,
                    AreaName = g.Key.AreaName ?? "Unknown",
                    AlarmStatus = g.Key.AlarmStatus.ToString(),
                    Total = g
                    .Select(x => x.AlarmTriggersId)
                    .Distinct()
                    .Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }


        // alarm per day
        public async Task<List<AlarmDailySummaryRM>> GetDailySummaryAsync(AlarmAnalyticsRequestRM request)
        {
            // 🕒 Gunakan helper untuk override from-to
            var range = GetTimeRange(request.TimeRange ?? "weekly");
            var from = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-7);
            var to = range?.to ?? request.To ?? DateTime.UtcNow;

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            var incidents = await query
                .Select(a => new
                {
                    Date = a.Timestamp.Value.Date,
                    a.AlarmTriggersId
                })
                .Distinct()
                .ToListAsync();

            var grouped = incidents
                .GroupBy(x => x.Date)
                .Select(g => new AlarmDailySummaryRM
                {
                    Date = g.Key,
                    Total = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            return grouped;
        }




        // alarm per status
        public async Task<List<AlarmStatusSummaryRM>> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var range = GetTimeRange(request.TimeRange);
            var (from, to) = (
                range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-7),
                range?.to ?? request.To ?? DateTime.UtcNow
            );

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            // Ambil data unik berdasarkan kombinasi AlarmTrigger + Status
            var incidents = await query
                .Select(a => new
                {
                    a.AlarmTriggersId,
                    a.Alarm
                })
                .Distinct()
                .ToListAsync();

            // GroupBy di memory untuk hasil yang aman dari translation EF
            var grouped = incidents
                .GroupBy(x => x.Alarm)
                .Select(g => new AlarmStatusSummaryRM
                {
                    Status = g.Key.HasValue ? g.Key.Value.ToString() : "Unknown",
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(3)
                .ToList();

            if (grouped.Count == 0)
            {
                var random = new Random();
                var statuses = Enum.GetValues(typeof(AlarmRecordStatus)).Cast<AlarmRecordStatus>().ToList();
                var randomStatuses = statuses.OrderBy(x => random.Next()).Take(3).Select(x => x.ToString()).ToList();

                grouped.AddRange(randomStatuses.Select(s => new AlarmStatusSummaryRM
                {
                    Status = s,
                    Total = 0
                }));
            }

            return grouped;
        }



        // alarm per visitor
        public async Task<List<AlarmVisitorSummaryRM>> GetVisitorSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (
                request.From ?? DateTime.UtcNow.AddDays(-7),
                request.To ?? DateTime.UtcNow
            );

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.Visitor)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            // Ambil kombinasi unik antara AlarmTriggerId dan Visitor
            var incidents = await query
                .Select(a => new
                {
                    a.AlarmTriggersId,
                    a.VisitorId,
                    VisitorName = a.Visitor != null ? a.Visitor.Name : "Unknown"
                })
                .Distinct()
                .ToListAsync();

            // Lakukan grouping di memory
            var grouped = incidents
                .GroupBy(x => new { x.VisitorId, x.VisitorName })
                .Select(g => new AlarmVisitorSummaryRM
                {
                    VisitorId = g.Key.VisitorId,
                    VisitorName = g.Key.VisitorName,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            return grouped;
        }



        // alarm per building
        public async Task<List<AlarmBuildingSummaryRM>> GetBuildingSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.FloorplanMaskedArea.Floorplan.Floor.Building)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            var incidents = await query
                .Select(a => new
                {
                    a.AlarmTriggersId,
                    BuildingId = a.FloorplanMaskedArea.Floorplan.Floor.Building.Id,
                    BuildingName = a.FloorplanMaskedArea.Floorplan.Floor.Building.Name
                })
                .Distinct()
                .ToListAsync();

            var grouped = incidents
                .GroupBy(x => new { x.BuildingId, x.BuildingName })
                .Select(g => new AlarmBuildingSummaryRM
                {
                    BuildingId = g.Key.BuildingId,
                    BuildingName = g.Key.BuildingName,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .ToList();
            return grouped;
        }

       public async Task<List<AlarmHourlyStatusSummaryRM>> GetHourlyStatusSummaryAsync(AlarmAnalyticsRequestRM request)
{
    // Range penuh 1 hari
    var date = request.From?.Date ?? DateTime.UtcNow.Date;

    var from = date;                              // 00:00
    var to = date.AddDays(1).AddTicks(-1);        // 23:59:59.9999999

    var query = _context.AlarmRecordTrackings
        .AsNoTracking()
        .Where(a => a.Timestamp >= from && a.Timestamp <= to);

    query = ApplyFilters(query, request);

    // STEP 1: Ambil *alarm pertama* dari setiap AlarmTriggersId dalam 1 hari
    var incidents = await query
        .GroupBy(a => a.AlarmTriggersId)
        .Select(g => new
        {
            AlarmTriggersId = g.Key,
            FirstTimestamp = g.Min(x => x.Timestamp),   // ambil jam pertama
            Status = g.OrderBy(x => x.Timestamp)
                      .First().Alarm.ToString() ?? "Unknown"
        })
        .ToListAsync();

    // STEP 2: Konversi ke hour
    var flattened = incidents
        .Select(x => new
        {
            Hour = x.FirstTimestamp.Value.Hour,
            x.Status
        })
        .ToList();

    // STEP 3: Grouping by hour + status
    var groupedHours = flattened
        .GroupBy(x => x.Hour)
        .ToDictionary(
            g => g.Key,
            g => new AlarmHourlyStatusSummaryRM
            {
                Hour = g.Key,
                HourLabel = g.Key.ToString("00") + ".00",
                Status = g.GroupBy(x => x.Status)
                            .ToDictionary(
                                s => s.Key,
                                s => s.Count()
                            )
            }
        );

    // STEP 4: Pastikan output 24 hours lengkap
    var fullDay = Enumerable.Range(0, 24)
        .Select(hour =>
            groupedHours.ContainsKey(hour)
                ? groupedHours[hour]
                : new AlarmHourlyStatusSummaryRM
                {
                    Hour = hour,
                    HourLabel = hour.ToString("00") + ".00",
                    Status = new Dictionary<string, int>()
                }
        )
        .OrderBy(x => x.Hour)
        .ToList();

    return fullDay;
}



        //    public async Task<List<(Guid BuildingId, string BuildingName, int Total)>> GetBuildingSummaryAsync(AlarmAnalyticsRequestRM request)
        //         {
        //             var (from, to) = (request.From ?? DateTime.UtcNow.AddDays(-7), request.To ?? DateTime.UtcNow);

        //             var query = _context.AlarmRecordTrackings
        //                 .AsNoTracking()
        //                 .Include(a => a.FloorplanMaskedArea.Floorplan.Floor.Building)
        //                 .Where(a => a.Timestamp >= from && a.Timestamp <= to);

        //             query = ApplyFilters(query, request);

        //             var incidents = await query
        //                 .Select(a => new
        //                 {
        //                     a.AlarmTriggersId,
        //                     BuildingId = a.FloorplanMaskedArea.Floorplan.Floor.Building.Id,
        //                     BuildingName = a.FloorplanMaskedArea.Floorplan.Floor.Building.Name
        //                 })
        //                 .Distinct()
        //                 .ToListAsync();

        //             return incidents
        //                 .GroupBy(x => new { x.BuildingId, x.BuildingName })
        //                 .Select(g => (g.Key.BuildingId, g.Key.BuildingName, g.Count()))
        //                 .ToList();
        // }


        // ===================================================================
        // 6️⃣ Helper Filter
        // ===================================================================
        private IQueryable<AlarmRecordTracking> ApplyFilters(IQueryable<AlarmRecordTracking> query, AlarmAnalyticsRequestRM request)
        {
            if (request.BuildingId.HasValue)
                query = query.Where(a => a.FloorplanMaskedArea.Floorplan.Floor.Building.Id == request.BuildingId);

            if (request.FloorId.HasValue)
                query = query.Where(a => a.FloorplanMaskedArea.Floorplan.Floor.Id == request.FloorId);

            if (request.FloorplanMaskedAreaId.HasValue)
                query = query.Where(a => a.FloorplanMaskedAreaId == request.FloorplanMaskedAreaId);

            if (request.VisitorId.HasValue)
                query = query.Where(a => a.VisitorId == request.VisitorId);

            if (!string.IsNullOrWhiteSpace(request.OperatorName))
                query = query.Where(a => a.DoneBy == request.OperatorName);

            return query;
        }

            // time range
//             private (DateTime from, DateTime to)? GetTimeRange(string? timeReport)
//             {
//                 if (string.IsNullOrWhiteSpace(timeReport))
//                     return null;

//                 // Gunakan UTC agar konsisten untuk server analytics
//                 var now = DateTime.UtcNow;

//                 // Pastikan format switch case aman (lowercase)
//                 return timeReport.Trim().ToLower() switch
//                 {
//                     "daily" => (
//                         now.Date,
//                         now.Date.AddDays(1).AddTicks(-1)
//                     ),

//                     "weekly" => (
//                         now.Date.AddDays(-(int)now.DayOfWeek + 1),                // Senin awal minggu
//                         now.Date.AddDays(7 - (int)now.DayOfWeek).AddDays(1).AddTicks(-1) // Minggu akhir
//                     ),

//                     "monthly" => (
//                         new DateTime(now.Year, now.Month, 1),
//                         new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month))
//                             .AddDays(1).AddTicks(-1)
//                     ),

//                     "yearly" => (
//                         new DateTime(now.Year, 1, 1),
//                         new DateTime(now.Year, 12, 31)
//                             .AddDays(1).AddTicks(-1)
//                     ),

//                     _ => null
//                 };
// }

    }
    
    
}

