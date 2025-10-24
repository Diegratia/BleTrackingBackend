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
        public async Task<List<(Guid? AreaId, string AreaName, int Total)>>
            GetAreaSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.FloorplanMaskedArea)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            var incidents = query
                .Select(a => new
                {
                    a.AlarmTriggersId,
                    a.FloorplanMaskedAreaId,
                    AreaName = a.FloorplanMaskedArea.Name
                })
                .Distinct();

            return await incidents
                .GroupBy(a => new { a.FloorplanMaskedAreaId, a.AreaName })
                .Select(g => new ValueTuple<Guid?, string, int>(
                    g.Key.FloorplanMaskedAreaId,
                    g.Key.AreaName,
                    g.Count()
                ))
                .ToListAsync();
        }

        // ===================================================================
        // 2️⃣ Total Alarm per Hari (Incident-level)
        // ===================================================================
        public async Task<List<(DateTime Date, int Total)>> GetDailySummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            var incidents = query
                .Select(a => new
                {
                    Date = a.Timestamp.Value.Date,
                    a.AlarmTriggersId
                })
                .Distinct();

            return await incidents
                .GroupBy(a => a.Date)
                .Select(g => new ValueTuple<DateTime, int>(
                    g.Key,
                    g.Count()
                ))
                .OrderBy(x => x.Item1)
                .ToListAsync();
        }

        // ===================================================================
        // 3️⃣ Alarm per Status (Incident-level)
        // ===================================================================
        public async Task<List<(string Status, int Total)>> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            var incidents = query
                .Select(a => new
                {
                    a.AlarmTriggersId,
                    a.Alarm
                })
                .Distinct();

            return await incidents
                .GroupBy(x => x.Alarm)
                .Select(g => new ValueTuple<string, int>(
                    g.Key.HasValue ? g.Key.Value.ToString() : "Unknown",
                    g.Count()
                ))
                .ToListAsync();
        }

        // ===================================================================
        // 4️⃣ Total Alarm per Visitor (Incident-level)
        // ===================================================================
        public async Task<List<(Guid? VisitorId, string VisitorName, int Total)>> 
            GetVisitorSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.Visitor)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            var incidents = query
                .Select(a => new
                {
                    a.AlarmTriggersId,
                    a.VisitorId,
                    VisitorName = a.Visitor.Name
                })
                .Distinct();

            return await incidents
                .GroupBy(a => new { a.VisitorId, a.VisitorName })
                .Select(g => new ValueTuple<Guid?, string, int>(
                    g.Key.VisitorId,
                    g.Key.VisitorName,
                    g.Count()
                ))
                .OrderByDescending(x => x.Item3)
                .ToListAsync();
        }

        // ===================================================================
        // 5️⃣ Total Alarm per Building (Incident-level)
        // ===================================================================
        public async Task<List<(Guid? BuildingId, string BuildingName, int Total)>> 
            GetBuildingSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.AddDays(-7), request.To ?? DateTime.UtcNow);

            var query = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(a => a.FloorplanMaskedArea.Floorplan.Floor.Building)
                .Where(a => a.Timestamp >= from && a.Timestamp <= to);

            query = ApplyFilters(query, request);

            var incidents = query
                .Select(a => new
                {
                    a.AlarmTriggersId,
                    BuildingId = a.FloorplanMaskedArea.Floorplan.Floor.Building.Id,
                    BuildingName = a.FloorplanMaskedArea.Floorplan.Floor.Building.Name
                })
                .Distinct();

            return await incidents
                .GroupBy(x => new { x.BuildingId, x.BuildingName })
                .Select(g => new ValueTuple<Guid?, string, int>(
                    g.Key.BuildingId,
                    g.Key.BuildingName,
                    g.Count()
                ))
                .ToListAsync();
        }

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
