using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
// using Repositories.RepoModels.Analytics;
using Repositories.Repository.RepoModel;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Repository.Analytics
{
    public class AlarmAnalyticsRepository : BaseRepository
    {
        private readonly BleTrackingDbContext _context;

        public AlarmAnalyticsRepository(BleTrackingDbContext context, IHttpContextAccessor accessor)
            : base(context, accessor)
        {
            _context = context;
        }

        // ===================================================================
        // 1️⃣ Daily Summary
        // ===================================================================
        public async Task<List<(DateTime Date, int Total, int Done, int Cancelled, double? AvgResponseSeconds)>>
            GetDailySummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.Date.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.FloorplanMaskedArea.Floorplan.Floor.Building)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            return await query
                .GroupBy(a => a.Timestamp.Value.Date)
                .Select(g => new ValueTuple<DateTime, int, int, int, double?>(
                    g.Key,
                    g.Count(),
                    g.Count(x => x.DoneTimestamp != null),
                    g.Count(x => x.CancelTimestamp != null),
                    g.Where(x => x.DoneTimestamp != null)
                        .Average(x => EF.Functions.DateDiffSecond(x.Timestamp, x.DoneTimestamp))
                ))
                .ToListAsync();
        }

        // ===================================================================
        // 2️⃣ Area Summary
        // ===================================================================
        public async Task<List<(Guid? AreaId, string AreaName, int Total, int Done, double? AvgResponseSeconds)>>
            GetAreaSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.Date.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.FloorplanMaskedArea.Floorplan.Floor.Building)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            return await query
                .GroupBy(a => new { a.FloorplanMaskedAreaId, a.FloorplanMaskedArea.Name })
                .Select(g => new ValueTuple<Guid?, string, int, int, double?>(
                    g.Key.FloorplanMaskedAreaId,
                    g.Key.Name,
                    g.Count(),
                    g.Count(x => x.DoneTimestamp != null),
                    g.Where(x => x.DoneTimestamp != null)
                        .Average(x => EF.Functions.DateDiffSecond(x.Timestamp, x.DoneTimestamp))
                ))
                .ToListAsync();
        }

        // ===================================================================
        // 3️⃣ Operator Summary
        // ===================================================================
        public async Task<List<(string OperatorName, int TotalHandled, double? AvgResponseSeconds)>>
            GetOperatorSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.Date.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Where(a => a.Timestamp >= from && a.Timestamp <= to && a.DoneBy != null);

            query = ApplyFilters(query, request);

            return await query
                .GroupBy(a => a.DoneBy)
                .Select(g => new ValueTuple<string, int, double?>(
                    g.Key,
                    g.Count(),
                    g.Average(x => EF.Functions.DateDiffSecond(x.Timestamp, x.DoneTimestamp))
                ))
                .ToListAsync();
        }

        // ===================================================================
        // 4️⃣ Status Summary
        // ===================================================================

        public async Task<List<(string Status, int Total)>> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (
                request.From ?? DateTime.UtcNow.Date.AddDays(-7),
                request.To ?? DateTime.UtcNow
            );

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Where(a => a.Timestamp >= from && a.Timestamp <= to && a.Alarm != null);

            query = ApplyFilters(query, request);

            // ✅ Step 1: Gunakan enum langsung (tanpa ToString)
            var rawData = await query
                .GroupBy(a => a.Alarm)
                .Select(g => new
                {
                    Alarm = g.Key,
                    Total = g.Count()
                })
                .ToListAsync();

            // ✅ Step 2: Convert enum ke string di memory
            return rawData
                .Select(x => (
                    Status: x.Alarm.HasValue ? x.Alarm.Value.ToString() : "Unknown",
                    x.Total
                ))
                .ToList();
        }


        // public async Task<List<(string Status, int Total)>> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request)
        // {
        //     var (from, to) = (request.From ?? DateTime.UtcNow.Date.AddDays(-7), request.To ?? DateTime.UtcNow);

        //     var query = _context.AlarmRecordTrackings
        //         .AsNoTracking()
        //         .Where(a => a.Timestamp >= from && a.Timestamp <= to && a.Alarm != null);

        //     query = ApplyFilters(query, request);

        //     return await query
        //         .GroupBy(a => a.Alarm.ToString())
        //         .Select(g => new ValueTuple<string, int>(g.Key, g.Count()))
        //         .ToListAsync();
        // }

        // ===================================================================
        // 5️⃣ Building Summary
        // ===================================================================
        public async Task<List<(Guid? BuildingId, string BuildingName, int Total, int Done, double? AvgResponseSeconds)>>
            GetBuildingSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.Date.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.FloorplanMaskedArea.Floorplan.Floor.Building)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            return await query
                .GroupBy(a => new
                {
                    a.FloorplanMaskedArea.Floorplan.Floor.Building.Id,
                    a.FloorplanMaskedArea.Floorplan.Floor.Building.Name
                })
                .Select(g => new ValueTuple<Guid?, string, int, int, double?>(
                    g.Key.Id,
                    g.Key.Name,
                    g.Count(),
                    g.Count(x => x.DoneTimestamp != null),
                    g.Where(x => x.DoneTimestamp != null)
                        .Average(x => EF.Functions.DateDiffSecond(x.Timestamp, x.DoneTimestamp))
                ))
                .ToListAsync();
        }

        // visitor sum
        public async Task<List<(Guid? VisitorId, string VisitorName, int TotalTriggered, int Done)>>
            GetVisitorSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.Date.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.Visitor)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            return await query
                .GroupBy(a => new { a.VisitorId, a.Visitor.Name })
                .Select(g => new ValueTuple<Guid?, string, int, int>(
                    g.Key.VisitorId,
                    g.Key.Name,
                    g.Count(),
                    g.Count(x => x.DoneTimestamp != null)
                ))
                .ToListAsync();
        }

        // time of day
        public async Task<List<(int Hour, int Total)>> GetTimeOfDaySummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.Date.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            return await query
                .GroupBy(a => a.Timestamp.Value.Hour)
                .Select(g => new ValueTuple<int, int>(g.Key, g.Count()))
                .OrderBy(x => x.Item1)
                .ToListAsync();
        }

        // weekly trend
            public async Task<List<(DateTime Date, int Total)>> GetWeeklyTrendAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (
                request.From ?? DateTime.UtcNow.Date.AddDays(-7),
                request.To ?? DateTime.UtcNow
            );

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            // ✅ gunakan anonymous type, bukan ValueTuple
            var rawData = await query
                .GroupBy(a => a.Timestamp.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // ✅ baru ubah ke ValueTuple setelah keluar dari EF
            return rawData
                .Select(x => (x.Date, x.Total))
                .ToList();
        }



            // floor sum
        public async Task<List<(Guid? FloorId, string FloorName, int Total, int Done, double? AvgResponseSeconds)>>
            GetFloorSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.Date.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.FloorplanMaskedArea.Floorplan.Floor)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            return await query
                .GroupBy(a => new
                {
                    a.FloorplanMaskedArea.Floorplan.Floor.Id,
                    a.FloorplanMaskedArea.Floorplan.Floor.Name
                })
                .Select(g => new ValueTuple<Guid?, string, int, int, double?>(
                    g.Key.Id,
                    g.Key.Name,
                    g.Count(),
                    g.Count(x => x.DoneTimestamp != null),
                    g.Where(x => x.DoneTimestamp != null)
                        .Average(x => EF.Functions.DateDiffSecond(x.Timestamp, x.DoneTimestamp))
                ))
                .ToListAsync();
        }

        // duration sum
        public async Task<List<(string DurationRange, int Count)>> GetDurationSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.Date.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Where(x => x.DoneTimestamp != null && x.Timestamp >= from && x.Timestamp <= to);

            query = ApplyFilters(query, request);

            return await query
                .Select(x => EF.Functions.DateDiffSecond(x.Timestamp, x.DoneTimestamp))
                .GroupBy(sec =>
                    sec <= 30 ? "0–30s" :
                    sec <= 60 ? "30–60s" :
                    sec <= 300 ? "1–5m" :
                    sec <= 900 ? "5–15m" : ">15m"
                )
                .Select(g => new ValueTuple<string, int>(g.Key, g.Count()))
                .ToListAsync();
        }

        // trend by action
        public async Task<List<(DateTime Date, string ActionStatus, int Total)>> GetTrendByActionAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (
                request.From ?? DateTime.UtcNow.Date.AddDays(-7),
                request.To ?? DateTime.UtcNow
            );

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            // ✅ Group by Action (integer) — aman untuk SQL translation
            var rawData = await query
                .GroupBy(a => new { Date = a.Timestamp.Value.Date, Action = a.Action })
                .Select(g => new
                {
                    Date = g.Key.Date,
                    Action = g.Key.Action,
                    Total = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // ✅ Convert ke ValueTuple dan ubah Action jadi string di memory
            return rawData
                .Select(x => (
                    x.Date,
                    ActionStatus: x.Action.HasValue ? x.Action.Value.ToString() : "Unknown",
                    x.Total
                ))
                .ToList();
        }


        // public async Task<List<(DateTime Date, string ActionStatus, int Total)>> GetTrendByActionAsync(AlarmAnalyticsRequestRM request)
        // {
        //     var (from, to) = (request.From ?? DateTime.UtcNow.Date.AddDays(-7), request.To ?? DateTime.UtcNow);

        //     var query = _context.AlarmRecordTrackings
        //         .AsNoTracking()
        //         .Where(a => a.Timestamp >= from && a.Timestamp <= to);

        //     query = ApplyFilters(query, request);

        //     return await query
        //         .GroupBy(a => new { Date = a.Timestamp.Value.Date, Action = a.Action.ToString() })
        //         .Select(g => new ValueTuple<DateTime, string, int>(
        //             g.Key.Date,
        //             g.Key.Action,
        //             g.Count()
        //         ))
        //         .ToListAsync();
        // }

        // ===================================================================
        // 🔧 Shared Filter Logic
        // ===================================================================
        private IQueryable<AlarmRecordTracking> ApplyFilters(
            IQueryable<AlarmRecordTracking> query, AlarmAnalyticsRequestRM request)
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
    }
}
