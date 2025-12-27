using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Repository.RepoModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class AlarmRecordTrackingRepository : BaseRepository
    {
        public AlarmRecordTrackingRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<List<AlarmRecordTracking>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<AlarmRecordTracking?> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
            .Where(v => v.Id == id)
            .FirstOrDefaultAsync();
        }

        // public IQueryable<AlarmRecordTracking> GetAllQueryable()
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

        //     var query = _context.AlarmRecordTrackings
        //         .Include(v => v.FloorplanMaskedArea)
        //         .Include(v => v.Reader)
        //         .Include(v => v.Visitor)
        //         .Where(v => v.Id != null);

        //         query = query.WithActiveRelations();

        //     return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        // }

         public IQueryable<AlarmRecordTracking> GetAllQueryable()
            {
                var userEmail = GetUserEmail();
                var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
                var isSuperAdmin = IsSuperAdmin();
                var isPrimaryAdmin = IsPrimaryAdmin();
                var isPrimary = IsPrimary();

                var query = _context.AlarmRecordTrackings
                      .IgnoreQueryFilters()
                    .Include(v => v.Application)
                    .Include(v => v.Visitor)
                    .Include(v => v.Member)
                    .Include(v => v.Reader)
                    .Include(v => v.FloorplanMaskedArea)
                    .Include(v => v.AlarmTriggers)
                    // .Where(v => v.Id != null)
                   
                    .AsQueryable();

                if (!isSystemAdmin && !isSuperAdmin && !isPrimaryAdmin && !isPrimary)
                {
                    var userRole = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
                    if (String.Equals(userRole, LevelPriority.Secondary.ToString(), StringComparison.OrdinalIgnoreCase))
                    // {
                    //     query = query.Where(t =>
                    //         _context.MstMembers.Any(m => m.Email == userEmail &&
                    //             (t.PurposePerson == m.Id || (t.MemberIdentity == m.IdentityId && t.IsMember == 1))));
                    // }
                    {
                        query = query.Where(t => true); // No access for other roles
                    }
                    else if (String.Equals(userRole, LevelPriority.UserCreated.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            query = query.Where(t =>
                                _context.Visitors.Any(v => v.Email == userEmail && t.VisitorId == v.Id));
                        }
                        else
                        {
                            query = query.Where(t => false); // No access for other roles
                        }
                }
                //  query = query.WithActiveRelations();

                return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            }

            


        public async Task<IEnumerable<AlarmRecordTracking>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<List<AlarmRecordLog>> GetAlarmLogsAsync(
            TrackingAnalyticsRequestRM request)
        {
            var range = GetTimeRange(request.TimeRange);
            var from = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-1);
            var to = range?.to ?? request.To ?? DateTime.UtcNow;

            var baseQuery = _context.AlarmRecordTrackings
                .AsNoTracking()
                .Include(x => x.Visitor)
                .Include(x => x.Member)
                .Include(x => x.FloorplanMaskedArea)
                .Include(x => x.AlarmTriggers)
                .Where(x => x.Timestamp >= from && x.Timestamp <= to);

            baseQuery = ApplyFilters(baseQuery, request);

            // 🔥 STEP 1: Group per Visitor + Status
            var grouped = await baseQuery
                .GroupBy(x => new
                {
                    x.VisitorId,
                    Status = x.Action != null
                        ? x.Action.ToString()
                        : x.Alarm != null
                            ? x.Alarm.ToString()
                            : "Unknown"
                })
                .Select(g => g
                    .OrderByDescending(x => x.Timestamp)
                    .First())
                .ToListAsync();

            // 🔥 STEP 2: Mapping ke Read Model
            return grouped
                .OrderByDescending(x => x.Timestamp)
                .Select(x => new AlarmRecordLog
                {
                    Visitor = x.Visitor?.Name ?? "-",
                    Area = x.FloorplanMaskedArea?.Name ?? "-",

                    TriggeredAt = x.Timestamp,
                    DoneAt = x.DoneTimestamp,

                    Status = x.Action != null
                        ? x.Action.ToString() 
                        : x.Alarm?.ToString() ?? "-",

                    Host = x.Member?.Name ?? "-",

                    Category = x.AlarmTriggers?.Alarm.ToString() ?? "-"
                })
                .ToList();
        }
        
        public async Task<List<AlarmTriggerLogFlatRM>> GetAlarmTriggerLogsAsync(
            AlarmAnalyticsRequestRM request)
        {
            var range = GetTimeRange(request.TimeRange ?? "weekly");
            var from = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-7);
            var to = range?.to ?? request.To ?? DateTime.UtcNow;

            var query = _context.AlarmTriggers
                .AsNoTracking()
                .Include(a => a.Floorplan)
                    .ThenInclude(fp => fp.Floor)
                        .ThenInclude(f => f.Building)
                .Include(a => a.Visitor)
                .Include(a => a.Member)
                .Where(a => a.TriggerTime >= from && a.TriggerTime <= to);

            // 🔹 PAKAI FILTER YANG SAMA POLANYA
            query = ApplyFilters(query, request);

            var result = await query
                .OrderByDescending(a => a.TriggerTime)
                .Select(a => new AlarmTriggerLogFlatRM
                {
                    VisitorId = a.VisitorId,
                    VisitorName = a.Visitor != null ? a.Visitor.Name : null,

                    BuildingId = a.Floorplan.Floor.Building.Id,
                    BuildingName = a.Floorplan.Floor.Building.Name,

                    FloorId = a.Floorplan.Floor.Id,
                    FloorName = a.Floorplan.Floor.Name,

                    FloorplanId = a.Floorplan.Id,
                    FloorplanName = a.Floorplan.Name,

                    AlarmStatus = a.Alarm.ToString(),
                    ActionStatus = a.Action.ToString(),

                    TriggeredAt = a.TriggerTime,
                    DoneAt = a.DoneTimestamp,
                    InvestigatedResult = a.InvestigatedResult,
                    LastNotifiedAt = a.LastNotifiedAt,

                    AssignedSecurityName = a.Security.Name,

                    // IdleDurationMinutes =
                    //     (a.LastSeenAt.Value - a.IdleTimestamp.Value).TotalMinutes,
                    HandleDurationMinutes = a.DoneTimestamp != null && a.TriggerTime != null ?
                                (a.DoneTimestamp.Value - a.TriggerTime.Value).TotalMinutes : null,

                    IsActive = a.IsActive
                })
                .ToListAsync();

            return result;
        }


         private (DateTime from, DateTime to)? GetTimeRange(string? timeReport)
        {
            if (string.IsNullOrWhiteSpace(timeReport)) return null;

            var wibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var wibNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, wibZone);

            return timeReport.Trim().ToLower() switch
            {
                "daily" =>
                (
                    TimeZoneInfo.ConvertTimeToUtc(wibNow.Date, wibZone),
                    TimeZoneInfo.ConvertTimeToUtc(
                        wibNow.Date.AddDays(1).AddTicks(-1),
                        wibZone
                    )
                ),

                "weekly" =>
                (
                    TimeZoneInfo.ConvertTimeToUtc(
                        wibNow.Date.AddDays(-(int)wibNow.DayOfWeek + 1),
                        wibZone
                    ),
                    TimeZoneInfo.ConvertTimeToUtc(
                        wibNow.Date.AddDays(7 - (int)wibNow.DayOfWeek)
                            .AddDays(1).AddTicks(-1),
                        wibZone
                    )
                ),

                "monthly" =>
                (
                    TimeZoneInfo.ConvertTimeToUtc(
                        new DateTime(wibNow.Year, wibNow.Month, 1),
                        wibZone
                    ),
                    TimeZoneInfo.ConvertTimeToUtc(
                        new DateTime(wibNow.Year, wibNow.Month,
                            DateTime.DaysInMonth(wibNow.Year, wibNow.Month))
                        .AddDays(1).AddTicks(-1),
                        wibZone
                    )
                ),

                _ => null
            };
        }

        private IQueryable<AlarmRecordTracking> ApplyFilters(
            IQueryable<AlarmRecordTracking> query,
            TrackingAnalyticsRequestRM request)
        {
            if (request.BuildingId.HasValue)
                query = query.Where(x =>
                    x.FloorplanMaskedArea.Floorplan.Floor.Building.Id == request.BuildingId);

            if (request.FloorId.HasValue)
                query = query.Where(x =>
                    x.FloorplanMaskedArea.Floorplan.Floor.Id == request.FloorId);

            if (request.AreaId.HasValue)
                query = query.Where(x =>
                    x.FloorplanMaskedAreaId == request.AreaId);

            if (request.VisitorId.HasValue)
                query = query.Where(x =>
                    x.VisitorId == request.VisitorId);

            return query;
        }

                private IQueryable<AlarmTriggers> ApplyFilters(
            IQueryable<AlarmTriggers> query,
            AlarmAnalyticsRequestRM request)
        {
            if (request.BuildingId.HasValue)
                query = query.Where(a =>
                    a.Floorplan.Floor.Building.Id == request.BuildingId);

            if (request.FloorId.HasValue)
                query = query.Where(a =>
                    a.Floorplan.Floor.Id == request.FloorId);

            if (request.FloorplanId.HasValue)
                query = query.Where(a =>
                    a.FloorplanId == request.FloorplanId);

            if (request.VisitorId.HasValue)
                query = query.Where(a =>
                    a.VisitorId == request.VisitorId);

            if (request.IsActive.HasValue)
                query = query.Where(a =>
                    a.IsActive == request.IsActive);

            return query;
        }


        // public async Task DeleteAsync(AlarmRecordTracking entity)
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

        //     if (!isSystemAdmin && entity.ApplicationId != applicationId)
        //         throw new UnauthorizedAccessException("You don’t have permission to delete this entity.");

        //     // _context.AlarmRecordTrackings.Remove(entity);
        //     await _context.SaveChangesAsync();
        // }

        private async Task ValidateRelatedEntitiesAsync(AlarmRecordTracking entity, Guid? applicationId, bool isSystemAdmin)
        {
            if (isSystemAdmin) return;

            if (!applicationId.HasValue)
                throw new UnauthorizedAccessException("Missing ApplicationId for non-admin.");

            var visitor = await _context.Visitors
                .FirstOrDefaultAsync(v => v.Id == entity.VisitorId && v.ApplicationId == applicationId);

            if (visitor == null)
                throw new UnauthorizedAccessException("Visitor not found or not accessible in your application.");

            var floorplanArea = await _context.FloorplanMaskedAreas
                .FirstOrDefaultAsync(f => f.Id == entity.FloorplanMaskedAreaId && f.ApplicationId == applicationId);

            if (floorplanArea == null)
                throw new UnauthorizedAccessException("FloorplanMaskedArea not found or not accessible in your application.");

            var reader = await _context.MstBleReaders
                .FirstOrDefaultAsync(r => r.Id == entity.ReaderId && r.ApplicationId == applicationId);

            if (reader == null)
                throw new UnauthorizedAccessException("BLE Reader not found or not accessible in your application.");
        }
    }
}
