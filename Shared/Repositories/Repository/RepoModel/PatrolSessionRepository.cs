using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;
using Repositories.Repository.RepoModel;
using System.Linq.Dynamic.Core;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class PatrolSessionRepository : BaseRepository
    {
        public PatrolSessionRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        private IQueryable<PatrolSession> BaseEntityQuery()
        {
            var userEmail = GetUserEmail();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var isSuperAdmin = IsSuperAdmin();
            var isPrimaryAdmin = IsPrimaryAdmin();

            var query = _context.PatrolSessions
                .Where(x => x.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            if (!isSystemAdmin && !isSuperAdmin && !isPrimaryAdmin)
            {
                    query = query.Where(pas =>
                        pas.Security != null &&
                        pas.Security.Email == userEmail
                    );
            }

            return query;
        }

        public async Task<PatrolSessionRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(x => x.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }
        public async Task<MstSecurity?> GetSecurityByEmail()
        {
            var query = await _context.MstSecurities
                .FirstOrDefaultAsync(x => x.Email == GetUserEmail());
            return query;
        }
        public async Task<PatrolAssignment?> GetAssignmentByIdAsync(Guid id)
        {
            var query = await _context.PatrolAssignments
                .Include(x => x.PatrolRoute)
                .Include(x => x.PatrolAssignmentSecurities)
                .Include(x => x.TimeGroup)
                    .ThenInclude(tg => tg.TimeBlocks)
                .Include(x => x.PatrolShiftReplacements)
            .FirstOrDefaultAsync(x => x.Id == id);
            return query;
        }
        public async Task<PatrolSession?> GetByIdEntityAsync(Guid id)
        {
            var query = BaseEntityQuery()
                .Include(s => s.PatrolCheckpointLogs)
                .Include(s => s.PatrolCases)
                .Where(x => x.Id == id);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PatrolSessionRead>> GetAllAsync()
        {
            var query = BaseEntityQuery();
            return await ProjectToRead(query).ToListAsync();
        }

            public async Task<List<PatrolSessionLookUpRead>> GetAllLookUpAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = BaseEntityQuery().AsNoTracking();

            var projected = query.Select(ca => new PatrolSessionLookUpRead
            {
                Id = ca.Id,
                PatrolRouteId = ca.PatrolRouteId,
                PatrolRouteName = ca.PatrolRouteNameSnap,
                SecurityId = ca.SecurityId,
                SecurityName = ca.SecurityNameSnap,
                PatrolAssignmentId = ca.PatrolAssignmentId,
                PatrolAssignmentName = ca.PatrolAssignmentNameSnap,
                StartedAt = ca.StartedAt,
                EndedAt = ca.EndedAt
            });
            return await projected.ToListAsync();
        }


        public async Task<PatrolSession> AddAsync(PatrolSession patrolSession)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                patrolSession.ApplicationId = applicationId.Value;
            }
            else if (patrolSession.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(patrolSession.ApplicationId);
            ValidateApplicationIdForEntity(patrolSession, applicationId, isSystemAdmin);

            _context.PatrolSessions.Add(patrolSession);

            await _context.SaveChangesAsync();
            return patrolSession;
        }
        
                public async Task UpdateAsync(PatrolSession patrolSession)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(patrolSession.ApplicationId);
            ValidateApplicationIdForEntity(patrolSession, applicationId, isSystemAdmin);
            await _context.SaveChangesAsync();
        }



        public IQueryable<PatrolSessionRead> ProjectToRead(
            IQueryable<PatrolSession> query
        )
        {
            // ini snapshot sejak sesi dimulai
            var projected = query.AsNoTracking().Select(t => new PatrolSessionRead
            {
                Id = t.Id,
                SecurityId = t.SecurityId,
                SecurityNameSnap = t.SecurityNameSnap,
                SecurityIdentityIdSnap = t.SecurityIdentityIdSnap,
                SecurityCardNumberSnap = t.SecurityCardNumberSnap,
                PatrolAssignmentId = t.PatrolAssignmentId,
                PatrolAssignmentNameSnap = t.PatrolAssignmentNameSnap,
                PatrolRouteId = t.PatrolRouteId,
                PatrolRouteNameSnap = t.PatrolRouteNameSnap,
                TimeGroupId = t.TimeGroupId,
                TimeGroupNameSnap = t.TimeGroupNameSnap,
                StartedAt = t.StartedAt,
                EndedAt = t.EndedAt,
                StartAreaNameSnap = t.PatrolCheckpointLogs
                                    .OrderBy(x => x.OrderIndex)
                                    .Select(x => x.AreaNameSnap)
                                    .FirstOrDefault(),
                EndAreaNameSnap = t.PatrolCheckpointLogs
                            .OrderByDescending(x => x.OrderIndex)
                            .Select(x => x.AreaNameSnap)
                            .FirstOrDefault(),
                ApplicationId = t.ApplicationId,

                // Checkpoint Details
                CheckpointCount = t.PatrolCheckpointLogs.Count(),
                Checkpoints = t.PatrolCheckpointLogs
                    .OrderBy(c => c.OrderIndex)
                    .Select(c => new PatrolCheckpointLogRead
                    {
                        Id = c.Id,
                        PatrolAreaId = c.PatrolAreaId,
                        AreaNameSnap = c.AreaNameSnap,
                        OrderIndex = c.OrderIndex,
                        CycleIndex = c.CycleIndex,
                        CheckpointStatus = c.CheckpointStatus,
                        ArrivedAt = c.ArrivedAt,
                        LeftAt = c.LeftAt,
                        ClearedAt = c.ClearedAt,
                        MinDwellTime = c.MinDwellTime,
                        MaxDwellTime = c.MaxDwellTime,
                        DistanceFromPrevMeters = c.DistanceFromPrevMeters,
                        Notes = c.Notes
                    }).ToList()
            });
            return projected;
        }

        public async Task<(List<PatrolSessionRead> Data, int Total, int Filtered)> FilterAsync(
            PatrolSessionFilter filter
        )
        {
            var query = BaseEntityQuery();

            // Apply TimeRange preset (overrides DateFrom/DateTo)
            var timeRange = GetTimeRange(filter.TimeRange);
            if (timeRange.HasValue)
            {
                query = query.Where(x => x.StartedAt >= timeRange.Value.from && x.StartedAt <= timeRange.Value.to);
            }
            else
            {
                // Use manual DateFrom/DateTo if TimeRange not specified
                if (filter.DateFrom.HasValue)
                    query = query.Where(x => x.StartedAt >= filter.DateFrom.Value);

                if (filter.DateTo.HasValue)
                    query = query.Where(x => x.StartedAt <= filter.DateTo.Value);
            }

            // Hitung total AWAL (sebelum filter diterapkan)
            var total = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x =>
                    (x.SecurityNameSnap != null && x.SecurityNameSnap.ToLower().Contains(search)) ||
                    (x.PatrolRouteNameSnap != null && x.PatrolRouteNameSnap.ToLower().Contains(search)) ||
                    (x.PatrolAssignmentNameSnap != null && x.PatrolAssignmentNameSnap.ToLower().Contains(search))
                );
            }

            if (filter.SecurityId.HasValue)
                query = query.Where(x => x.SecurityId == filter.SecurityId.Value);

            if (filter.PatrolAssignmentId.HasValue)
                query = query.Where(x => x.PatrolAssignmentId == filter.PatrolAssignmentId.Value);

            if (filter.PatrolRouteId.HasValue)
                query = query.Where(x => x.PatrolRouteId == filter.PatrolRouteId.Value);

            if (filter.EndedAt.HasValue)
                query = query.Where(x => x.EndedAt <= filter.EndedAt.Value);

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        /// <summary>
        /// Gets patrol route areas with their patrol area data for checkpoint log creation.
        /// </summary>
        public async Task<List<PatrolRouteAreas>> GetPatrolRouteAreasAsync(Guid routeId)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolRouteAreas
                .Include(x => x.PatrolArea)
                .Where(x => x.PatrolRouteId == routeId && x.status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.OrderBy(x => x.OrderIndex).ToListAsync();
        }

        /// <summary>
        /// Adds checkpoint logs in batch for a patrol session.
        /// </summary>
        public async Task AddCheckpointLogsAsync(IEnumerable<PatrolCheckpointLog> checkpointLogs)
        {
            await _context.PatrolCheckpointLogs.AddRangeAsync(checkpointLogs);
            await _context.SaveChangesAsync();
        }

        public async Task<PatrolCheckpointLog?> GetActiveCheckpointLogAsync(Guid logId, Guid areaId)
        {
            return await _context.PatrolCheckpointLogs
                .Include(l => l.PatrolSession)
                    .ThenInclude(s => s.PatrolAssignment)
                .FirstOrDefaultAsync(l => l.Id == logId 
                                       && l.PatrolAreaId == areaId 
                                       && l.ArrivedAt != null 
                                       && l.ClearedAt == null);
        }

        public async Task<bool> HasUnclearedPreviousCheckpointsAsync(Guid sessionId, int currentOrderIndex)
        {
            return await _context.PatrolCheckpointLogs
                .AnyAsync(l => l.PatrolSessionId == sessionId 
                            && l.OrderIndex != null
                            && l.OrderIndex < currentOrderIndex 
                            && l.ClearedAt == null);
        }

        public async Task UpdateCheckpointLogAsync(PatrolCheckpointLog log)
        {
            _context.PatrolCheckpointLogs.Update(log);
            await _context.SaveChangesAsync();
        }

        // =====================================================
        // ANALYTICS METHODS
        // =====================================================

        /// <summary>
        /// Gets raw patrol session data for analytics
        /// Uses DataTables Pattern with BaseFilter
        /// </summary>
        public async Task<(List<PatrolSession> Data, int Total, int Filtered)> GetAnalyticsDataAsync(
            PatrolSessionAnalyticsFilter filter)
        {
            IQueryable<PatrolSession> query = BaseEntityQuery();

            // Apply TimeRange preset (overrides DateFrom/DateTo)
            var timeRange = GetTimeRange(filter.TimeRange);
            if (timeRange.HasValue)
            {
                query = query.Where(s => s.StartedAt >= timeRange.Value.from && s.StartedAt <= timeRange.Value.to);
            }
            else
            {
                // Apply time filters from BaseFilter (DateFrom, DateTo)
                if (filter.DateFrom.HasValue)
                    query = query.Where(s => s.StartedAt >= filter.DateFrom.Value);

                if (filter.DateTo.HasValue)
                    query = query.Where(s => s.StartedAt <= filter.DateTo.Value);
            }

            // Hitung total AWAL (sebelum filter diterapkan)
            var total = await query.CountAsync();

            // Apply search from BaseFilter
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(s =>
                    (s.SecurityNameSnap != null && s.SecurityNameSnap.ToLower().Contains(search)) ||
                    (s.PatrolRouteNameSnap != null && s.PatrolRouteNameSnap.ToLower().Contains(search)) ||
                    (s.PatrolAssignmentNameSnap != null && s.PatrolAssignmentNameSnap.ToLower().Contains(search))
                );
            }

            // Apply entity filters using ExtractIds for JsonElement
            var securityIds = ExtractIds(filter.SecurityId);
            if (securityIds.Count > 0)
                query = query.Where(s => securityIds.Contains(s.SecurityId));

            var routeIds = ExtractIds(filter.RouteId);
            if (routeIds.Count > 0)
                query = query.Where(s => routeIds.Contains(s.PatrolRouteId));

            var assignmentIds = ExtractIds(filter.AssignmentId);
            if (assignmentIds.Count > 0)
                query = query.Where(s => assignmentIds.Contains(s.PatrolAssignmentId));

            var areaIds = ExtractIds(filter.AreaId);
            if (areaIds.Count > 0)
                query = query.Where(s => s.PatrolCheckpointLogs
                    .Any(l => areaIds.Contains(l.PatrolAreaId.Value)));

            // Filter by completion status
            if (filter.IsCompleted.ValueKind != JsonValueKind.Undefined && filter.IsCompleted.ValueKind != JsonValueKind.Null)
            {
                if (filter.IsCompleted.ValueKind == JsonValueKind.True || filter.IsCompleted.ValueKind == JsonValueKind.False)
                {
                    var isCompleted = filter.IsCompleted.GetBoolean();
                    query = isCompleted
                        ? query.Where(s => s.EndedAt != null)
                        : query.Where(s => s.EndedAt == null);
                }
            }

            // Apply sorting and paging from BaseFilter
            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            // Now apply includes for the final query
            query = query
                .Include(s => s.PatrolCheckpointLogs)
                .Include(s => s.PatrolCases)
                .Include(s => s.PatrolRoute)
                .Include(s => s.PatrolAssignment)
                .Include(s => s.Security);

            var filtered = await query.CountAsync();

            var data = await query.ToListAsync();

            return (data, total, filtered);
        }

        /// <summary>
        /// Gets single patrol session for timeline
        /// </summary>
        public async Task<PatrolSession?> GetSessionForTimelineAsync(Guid sessionId)
        {
            return await BaseEntityQuery()
                .Include(s => s.PatrolCheckpointLogs)
                    .ThenInclude(l => l.PatrolArea)
                .Include(s => s.PatrolCases)
                .Include(s => s.PatrolRoute)
                .Include(s => s.PatrolAssignment)
                .Include(s => s.Security)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        /// <summary>
        /// Check if security has an active (uncompleted) patrol session for specific assignment
        /// </summary>
        public async Task<bool> HasActiveSessionAsync(Guid securityId, Guid patrolAssignmentId)
        {
            return await BaseEntityQuery()
                .AnyAsync(s => s.SecurityId == securityId
                    && s.PatrolAssignmentId == patrolAssignmentId
                    && s.EndedAt == null);
        }

        /// <summary>
        /// Check if security has already completed a patrol session within the specific TimeBlock limits for today
        /// </summary>
        public async Task<bool> HasPatrolledTimeBlockAsync(Guid securityId, Guid patrolAssignmentId, DateTime currentDateUtc, TimeSpan blockStart, TimeSpan blockEnd)
        {
            var startOfDay = currentDateUtc.Date; // 00:00:00 of the current day
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1); // 23:59:59

            var sessionsToday = await BaseEntityQuery()
                .Where(s => s.SecurityId == securityId
                    && s.PatrolAssignmentId == patrolAssignmentId
                    && s.StartedAt >= startOfDay
                    && s.StartedAt <= endOfDay
                    && s.EndedAt != null)
                .ToListAsync();

            // We evaluate in memory because evaluating TimeSpan logic across StartedAt inside DB provider varies
            return sessionsToday.Any(s => 
                s.StartedAt.TimeOfDay >= blockStart && 
                s.StartedAt.TimeOfDay <= blockEnd
            );
        }
    }
}
