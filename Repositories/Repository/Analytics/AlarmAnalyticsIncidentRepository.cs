using Entities.Models;
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

        // ===================================================================
        // 1️⃣ Total Alarm per Area (Incident-level)
        // ===================================================================
        public async Task<List<AlarmAreaSummaryRM>> GetAreaSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.FloorplanMaskedArea)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            var data = await query
                .Select(a => new
                {
                    a.AlarmTriggersId,
                    a.FloorplanMaskedAreaId,
                    AreaName = a.FloorplanMaskedArea.Name
                })
                .Distinct()
                .GroupBy(x => new { x.FloorplanMaskedAreaId, x.AreaName })
                .Select(g => new AlarmAreaSummaryRM
                {
                    AreaId = g.Key.FloorplanMaskedAreaId,
                    AreaName = g.Key.AreaName,
                    Total = g.Count()
                })
                .ToListAsync();

            return data;
        }


        // ===================================================================
        // 2️⃣ Total Alarm per Hari (Incident-level)
        // ===================================================================
        public async Task<List<AlarmDailySummaryRM>> GetDailySummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (
                request.From ?? DateTime.UtcNow.AddDays(-7),
                request.To ?? DateTime.UtcNow
            );

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            // Ambil data minimal yang diperlukan dan Distinct berdasarkan AlarmTriggersId + Date
            var incidents = await query
                .Select(a => new
                {
                    Date = a.Timestamp.Value.Date,
                    a.AlarmTriggersId
                })
                .Distinct()
                .ToListAsync();

            // GroupBy di memory — lebih aman dari segi EF Translation
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



        // ===================================================================
        // 3️⃣ Alarm per Status (Incident-level)
        // ===================================================================
            public async Task<List<AlarmStatusSummaryRM>> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (
                request.From ?? DateTime.UtcNow.AddDays(-7),
                request.To ?? DateTime.UtcNow
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
                .ToList();

            return grouped;
        }



        // ===================================================================
        // 4️⃣ Total Alarm per Visitor (Incident-level)
        // ===================================================================
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



        // ===================================================================
        // 5️⃣ Total Alarm per Building (Incident-level)
        // ===================================================================
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
    }
}
