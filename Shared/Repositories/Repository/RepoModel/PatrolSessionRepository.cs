using System;
using System;
using System.Collections.Generic;
using System.Linq;
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
            .FirstOrDefaultAsync(x => x.Id == id);
            return query;
        }
        public async Task<PatrolSession?> GetByIdEntityAsync(Guid id)
        {
            var query = BaseEntityQuery()
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
                        ArrivedAt = c.ArrivedAt,
                        LeftAt = c.LeftAt,
                        DistanceFromPrevMeters = c.DistanceFromPrevMeters
                    }).ToList()
            });
            return projected;
        }

        public async Task<(List<PatrolSessionRead> Data, int Total, int Filtered)> FilterAsync(
            PatrolSessionFilter filter
        )
        {
            var query = BaseEntityQuery();

            var total = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                // query = query.Where(x =>
                //     x.Title.ToLower().Contains(search) ||
                //     x.Description.ToLower().Contains(search)
                // );
            }

            if (filter.SecurityId.HasValue)
                query = query.Where(x => x.SecurityId == filter.SecurityId.Value);

            if (filter.PatrolAssignmentId.HasValue)
                query = query.Where(x => x.PatrolAssignmentId == filter.PatrolAssignmentId.Value);

            if (filter.PatrolRouteId.HasValue)
                query = query.Where(x => x.PatrolRouteId == filter.PatrolRouteId.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.UpdatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.UpdatedAt <= filter.DateTo.Value);

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

    }
}
