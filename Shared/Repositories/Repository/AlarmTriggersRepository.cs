using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Extensions;
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class AlarmTriggersRepository : BaseRepository
    {
        public AlarmTriggersRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
           : base(context, httpContextAccessor)
        {
        }

        public IQueryable<AlarmTriggers> BaseEntityQuery()
        {
            var userEmail = GetUserEmail();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var isSuperAdmin = IsSuperAdmin();
            var isPrimaryAdmin = IsPrimaryAdmin();

            var query = _context.AlarmTriggers
                .Include(b => b.Floorplan)
                    .ThenInclude(f => f.Floor)
                .Include(b => b.Visitor)
                .Include(b => b.Member)
                .Include(b => b.Security)
                .AsSplitQuery();

            if (!isSystemAdmin && !isSuperAdmin && !isPrimaryAdmin)
            {
                query = query.Where(pc =>
                    (pc.Security != null && pc.Security.Email == userEmail)
                    || _context.MstSecurities.Any(pas =>
                        pas.Id == pc.SecurityId
                        && pas.Email == userEmail
                    )
                );
            }

            query = ApplyAccessibleBuildingFilter(query);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private IQueryable<AlarmTriggers> ApplyAccessibleBuildingFilter(IQueryable<AlarmTriggers> query)
        {
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                query = query.Where(b =>
                    b.Floorplan != null &&
                    b.Floorplan.Floor != null &&
                    accessibleBuildingIds.Contains(b.Floorplan.Floor.BuildingId));
            }

            return query;
        }

        private IQueryable<AlarmTriggersRead> ProjectToRead(IQueryable<AlarmTriggers> query)
        {
            return query
                .Select(b => new AlarmTriggersRead
                {
                    Id = b.Id,
                    BeaconId = b.BeaconId,
                    FloorplanId = b.FloorplanId,
                    PosX = b.PosX,
                    PosY = b.PosY,
                    IsInRestrictedArea = b.IsInRestrictedArea,
                    FirstGatewayId = b.FirstGatewayId,
                    SecondGatewayId = b.SecondGatewayId,
                    FirstDistance = b.FirstDistance,
                    SecondDistance = b.SecondDistance,
                    VisitorId = b.VisitorId,
                    MemberId = b.MemberId,
                    SecurityId = b.SecurityId,
                    TriggerTime = b.TriggerTime,
                    AlarmColor = b.AlarmColor,
                    Alarm = b.Alarm,
                    Action = b.Action,
                    IsActive = b.IsActive,
                    IdleTimestamp = b.IdleTimestamp,
                    DoneTimestamp = b.DoneTimestamp,
                    CancelTimestamp = b.CancelTimestamp,
                    WaitingTimestamp = b.WaitingTimestamp,
                    DispatchedAt = b.DispatchedAt,
                    InvestigatedDoneAt = b.InvestigatedDoneAt,
                    ActionUpdatedAt = b.ActionUpdatedAt,
                    LastSeenAt = b.LastSeenAt,
                    LastNotifiedAt = b.LastNotifiedAt,
                    IdleBy = b.IdleBy,
                    DoneBy = b.DoneBy,
                    CancelBy = b.CancelBy,
                    WaitingBy = b.WaitingBy,
                    DispatchedBy = b.DispatchedBy,
                    AcceptedBy = b.AcceptedBy,
                    InvestigatedDoneBy = b.InvestigatedDoneBy,
                    InvestigatedResult = b.InvestigatedResult,
                    ApplicationId = b.ApplicationId,

                    // Navigation properties
                    FloorplanName = b.Floorplan != null ? b.Floorplan.Name : null,
                    FloorName = b.Floorplan != null && b.Floorplan.Floor != null ? b.Floorplan.Floor.Name : null,
                    FloorplanImage = b.Floorplan != null ? b.Floorplan.FloorplanImage : null,
                    BuildingId = b.Floorplan != null && b.Floorplan.Floor != null ? b.Floorplan.Floor.BuildingId : null,
                    BuildingName = b.Floorplan != null && b.Floorplan.Floor != null && b.Floorplan.Floor.Building != null ? b.Floorplan.Floor.Building.Name : null,

                    // Person navigation properties
                    VisitorName = b.Visitor != null ? b.Visitor.Name : null,
                    VisitorIdentityId = b.Visitor != null ? b.Visitor.IdentityId : null,
                    VisitorCardNumber = b.Visitor != null ? b.Visitor.CardNumber : null,
                    VisitorFaceImage = b.Visitor != null ? b.Visitor.FaceImage : null,

                    MemberName = b.Member != null ? b.Member.Name : null,
                    MemberIdentityId = b.Member != null ? b.Member.IdentityId : null,
                    MemberCardNumber = b.Member != null ? b.Member.CardNumber : null,
                    MemberFaceImage = b.Member != null ? b.Member.FaceImage : null,

                    SecurityName = b.Security != null ? b.Security.Name : null,
                    SecurityEmail = b.Security != null ? b.Security.Email : null
                });
        }

        public async Task<AlarmTriggersRead?> GetByIdAsync(Guid id)
        {
            return await ProjectToRead(BaseEntityQuery())
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<AlarmTriggers?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<AlarmTriggersRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<(List<AlarmTriggersRead> Data, int Total, int Filtered)> FilterAsync(AlarmTriggersFilter filter)
        {
            var query = BaseEntityQuery();
            var total = await query.CountAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(b =>
                    (b.BeaconId != null && b.BeaconId.Contains(filter.Search)) ||
                    (b.Visitor != null && b.Visitor.Name != null && b.Visitor.Name.Contains(filter.Search)) ||
                    (b.Member != null && b.Member.Name != null && b.Member.Name.Contains(filter.Search)) ||
                    (b.Floorplan != null && b.Floorplan.Name != null && b.Floorplan.Name.Contains(filter.Search)));
            }

            if (!string.IsNullOrEmpty(filter.BeaconId))
                query = query.Where(b => b.BeaconId != null && b.BeaconId.Contains(filter.BeaconId));

            if (filter.Alarm.HasValue)
                query = query.Where(b => b.Alarm == filter.Alarm.Value);

            if (filter.Action.HasValue)
                query = query.Where(b => b.Action == filter.Action.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(b => b.IsActive == filter.IsActive.Value);

            if (filter.IsInRestrictedArea.HasValue)
                query = query.Where(b => b.IsInRestrictedArea == filter.IsInRestrictedArea.Value);

            if (!string.IsNullOrEmpty(filter.AlarmColor))
                query = query.Where(b => b.AlarmColor != null && b.AlarmColor == filter.AlarmColor);

            if (filter.TriggerTimeFrom.HasValue)
                query = query.Where(b => b.TriggerTime >= filter.TriggerTimeFrom.Value);

            if (filter.TriggerTimeTo.HasValue)
                query = query.Where(b => b.TriggerTime <= filter.TriggerTimeTo.Value);

            if (filter.ActionUpdatedAtFrom.HasValue)
                query = query.Where(b => b.ActionUpdatedAt >= filter.ActionUpdatedAtFrom.Value);

            if (filter.ActionUpdatedAtTo.HasValue)
                query = query.Where(b => b.ActionUpdatedAt <= filter.ActionUpdatedAtTo.Value);

            if (filter.FloorplanId.ValueKind != JsonValueKind.Undefined && filter.FloorplanId.ValueKind != JsonValueKind.Null)
            {
                var floorplanIds = ExtractIds(filter.FloorplanId);
                if (floorplanIds.Any())
                    query = query.Where(b => b.FloorplanId.HasValue && floorplanIds.Contains(b.FloorplanId.Value));
            }

            if (filter.VisitorId.ValueKind != JsonValueKind.Undefined && filter.VisitorId.ValueKind != JsonValueKind.Null)
            {
                var visitorIds = ExtractIds(filter.VisitorId);
                if (visitorIds.Any())
                    query = query.Where(b => b.VisitorId.HasValue && visitorIds.Contains(b.VisitorId.Value));
            }
            if (filter.MemberId.ValueKind != JsonValueKind.Undefined && filter.MemberId.ValueKind != JsonValueKind.Null)
            {
                var memberIds = ExtractIds(filter.MemberId);
                if (memberIds.Any())
                    query = query.Where(b => b.MemberId.HasValue && memberIds.Contains(b.MemberId.Value));
            }

            if (filter.SecurityId.ValueKind != JsonValueKind.Undefined && filter.SecurityId.ValueKind != JsonValueKind.Null)
            {
                var securityIds = ExtractIds(filter.SecurityId);
                if (securityIds.Any())
                    query = query.Where(b => b.SecurityId.HasValue && securityIds.Contains(b.SecurityId.Value));
            }

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);

            if (string.IsNullOrEmpty(filter.SortColumn))
            {
                query = query.OrderByDescending(b => b.TriggerTime);
            }

            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        // ===========================================================
        // Legacy methods kept for backward compatibility
        // ===========================================================

        // public async Task<IEnumerable<AlarmTriggersLookUp>> GetAllLookUpAsync()
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

        //     var query = _context.AlarmTriggers
        //         .AsNoTracking()
        //         .Where(b => b.IsActive == true && 
        //                 b.Alarm.HasValue && 
        //                 (b.VisitorId.HasValue || b.MemberId.HasValue));

        //     if (!isSystemAdmin)
        //     {
        //         query = query.Where(b => b.ApplicationId == applicationId);
        //     }

        //     var result = await query
        //         .Select(b => new AlarmTriggersLookUp
        //         {
        //             Id = b.Id,
        //             BeaconId = b.BeaconId,
        //             VisitorId = b.VisitorId,
        //             MemberId = b.MemberId,
        //             VisitorName = b.VisitorId.HasValue 
        //                 ? b.Visitor.Name 
        //                 : null,
        //             MemberName = b.MemberId.HasValue
        //                 ? b.Member.Name
        //                 : null,
        //             VisitorFaceImage = b.Visitor.FaceImage,
        //             MemberFaceImage = b.Member.FaceImage, 
        //             PersonImage = b.Visitor.FaceImage ?? b.Member.FaceImage,
        //             CardNumber = b.Visitor.CardNumber ?? b.Member.CardNumber,
        //             Dmac = b.Visitor.BleCardNumber ?? b.Member.BleCardNumber,
        //             TriggerTime = b.TriggerTime,
        //             ApplicationId = b.ApplicationId
        //         })
        //         .OrderByDescending(b => b.TriggerTime)
        //         .ToListAsync();

        //     return result;
        // }

        public async Task<IEnumerable<AlarmTriggersLookUp>> GetAllLookUpAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            // Apply ApplicationId filter first
            var query = _context.AlarmTriggers
                .Include(b => b.Floorplan)
                .ThenInclude(f => f.Floor)
                .AsNoTracking()
                .Where(b => b.IsActive == true &&
                       b.Alarm.HasValue &&
                       (b.VisitorId.HasValue || b.MemberId.HasValue));

            query = ApplyAccessibleBuildingFilter(query);

            if (!isSystemAdmin)
            {
                query = query.Where(b => b.ApplicationId == applicationId);
            }

            try
            {
                var allData = await query
                    .Select(b => new
                    {
                        Entity = b,
                        PersonGuid = b.VisitorId ?? b.MemberId,
                        VisitorName = b.VisitorId.HasValue && b.Visitor != null ? b.Visitor.Name : null,
                        MemberName = b.MemberId.HasValue && b.Member != null ? b.Member.Name : null,
                        VisitorFaceImage = b.VisitorId.HasValue && b.Visitor != null ? b.Visitor.FaceImage : null,
                        MemberFaceImage = b.MemberId.HasValue && b.Member != null ? b.Member.FaceImage : null,
                        TriggerTime = b.TriggerTime,
                        ApplicationId = b.ApplicationId
                    })
                    .OrderByDescending(x => x.TriggerTime)
                    .ToListAsync();

                var distinctData = allData
                    .Where(x => x.PersonGuid.HasValue)
                    .GroupBy(x => x.PersonGuid)
                    .Select(g => g.First())
                    .Select(x => new AlarmTriggersLookUp
                    {
                        Id = x.Entity.Id,
                        BeaconId = x.Entity.BeaconId,
                        VisitorId = x.Entity.VisitorId,
                        MemberId = x.Entity.MemberId,
                        VisitorName = x.VisitorName,
                        MemberName = x.MemberName,
                        VisitorFaceImage = x.VisitorFaceImage,
                        MemberFaceImage = x.MemberFaceImage,
                        PersonImage = x.VisitorFaceImage ?? x.MemberFaceImage,
                        TriggerTime = x.TriggerTime,
                        ApplicationId = x.ApplicationId
                    })
                    .OrderByDescending(b => b.TriggerTime)
                    .ToList();

                return distinctData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllLookUpAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<int> GetCountAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.AlarmTriggers
                .AsNoTracking()
                .Where(c => c.IsActive == true && c.DoneTimestamp == null && c.Action != ActionStatus.Done && c.Action != ActionStatus.NoAction);

            q = ApplyAccessibleBuildingFilter(q);
            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            return await q.CountAsync();
        }

        public async Task<List<AlarmTriggersRM>> GetTopTriggersAsync(int topCount = 5)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.AlarmTriggers
                .AsNoTracking()
                .Where(c => c.IsActive == true && c.DoneTimestamp == null && c.Action != ActionStatus.Done && c.Action != ActionStatus.NoAction);

            q = ApplyAccessibleBuildingFilter(q);
            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            return await q
                .OrderByDescending(x => x.TriggerTime)
                .Take(topCount)
                .Select(x => new AlarmTriggersRM
                {
                    Id = x.Id,
                    BeaconId = x.BeaconId ?? "Unknown Card",
                })
                .ToListAsync();
        }

        public async Task<List<AlarmTriggers>> GetByDmacAsync(string beaconId)
        {
            return await GetAllQueryable()
                .Where(b => b.BeaconId == beaconId && b.IsActive != false)
                .ToListAsync();
        }

        public async Task UpdateAsync(AlarmTriggers alarmTriggers)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(alarmTriggers.ApplicationId);
            ValidateApplicationIdForEntity(alarmTriggers, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateBatchAsync(IEnumerable<AlarmTriggers> alarmTriggers)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            foreach (var alarmTrigger in alarmTriggers)
            {
                await ValidateApplicationIdAsync(alarmTrigger.ApplicationId);
                ValidateApplicationIdForEntity(alarmTrigger, applicationId, isSystemAdmin);
            }

            await _context.SaveChangesAsync();
        }

        public IQueryable<AlarmTriggers> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.AlarmTriggers
                .Include(b => b.Visitor)
                .Include(b => b.Member)
                .Include(b => b.Security)
                .Include(b => b.Floorplan)
                    .ThenInclude(f => f.Floor);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<MstSecurity?> GetSecurityByIdAsync(Guid securityId)
        {
            return await _context.MstSecurities
                .Where(x => x.Id == securityId && x.Status != 0)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get complete incident timeline data for analytics
        /// Includes all related entities: Visitor, Member, Security, Floorplan, Floor
        /// </summary>
        public async Task<AlarmTriggers?> GetIncidentTimelineAsync(Guid alarmTriggerId)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.AlarmTriggers
                .Include(b => b.Visitor)
                .Include(b => b.Member)
                .Include(b => b.Security)
                .Include(b => b.Floorplan)
                    .ThenInclude(f => f.Floor)
                .Where(b => b.Id == alarmTriggerId);

            query = ApplyAccessibleBuildingFilter(query);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.FirstOrDefaultAsync();
        }

        // ===========================================================
        // Ownership validation helpers
        // ===========================================================

        /// <summary>
        /// Validate that VisitorId belongs to the same application
        /// </summary>
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidVisitorOwnershipAsync(
            Guid visitorId,
            Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<Visitor>(
                new[] { visitorId },
                applicationId
            );
        }

        /// <summary>
        /// Validate that MemberId belongs to the same application
        /// </summary>
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidMemberOwnershipAsync(
            Guid memberId,
            Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstMember>(
                new[] { memberId },
                applicationId
            );
        }

        /// <summary>
        /// Validate that SecurityId belongs to the same application
        /// </summary>
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidSecurityOwnershipAsync(
            Guid securityId,
            Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstSecurity>(
                new[] { securityId },
                applicationId
            );
        }

        /// <summary>
        /// Validate that FloorplanId belongs to the same application
        /// </summary>
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidFloorplanOwnershipAsync(
            Guid floorplanId,
            Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstFloorplan>(
                new[] { floorplanId },
                applicationId
            );
        }
    }
}
