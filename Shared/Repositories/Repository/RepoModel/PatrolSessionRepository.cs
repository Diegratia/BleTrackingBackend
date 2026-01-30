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
                PatrolRouteName = ca.PatrolRoute != null ? ca.PatrolRoute.Name : null,
                SecurityId = ca.SecurityId,
                SecurityName = ca.Security != null ? ca.Security.Name : null,
                PatrolAssignmentId = ca.PatrolAssignmentId,
                PatrolAssignmentName = ca.PatrolAssignment != null ? ca.PatrolAssignment.Name : null,
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


        public IQueryable<PatrolSessionRead> ProjectToRead(
            IQueryable<PatrolSession> query 
        )
        {
            var projected = query.AsNoTracking().Select(t => new PatrolSessionRead
            {
                Id = t.Id,
                SecurityId = t.SecurityId,
                PatrolAssignmentId = t.PatrolAssignmentId,
                PatrolRouteId = t.PatrolRouteId,
                StartedAt = t.StartedAt,
                EndedAt = t.EndedAt,
                ApplicationId = t.ApplicationId,
                Security = t.Security == null ? null : new MstSecurityLookUpRead
                {
                    Id = t.Security.Id,
                    Name = t.Security.Name,
                    PersonId = t.Security.PersonId,
                    CardNumber = t.Security.CardNumber,
                    OrganizationId = t.Security.OrganizationId,
                    DepartmentId = t.Security.DepartmentId,
                    DistrictId = t.Security.DistrictId,
                    OrganizationName = t.Security.Organization.Name,
                    DepartmentName = t.Security.Department.Name,
                    DistrictName = t.Security.District.Name,
                },
                PatrolAssignment = t.PatrolAssignment == null ? null : new PatrolAssignmentLookUpRead
                {
                    Id = t.PatrolAssignment.Id,
                    Name = t.PatrolAssignment.Name,
                    Description = t.PatrolAssignment.Description,
                },
                PatrolRoute = t.PatrolRoute == null ? null : new PatrolRouteLookUpRead
                {
                    Id = t.PatrolRoute.Id,
                    Name = t.PatrolRoute.Name,
                    Description = t.PatrolRoute.Description,
                    StartAreaName = t.PatrolRoute.PatrolRouteAreas
                        .OrderBy(x => x.OrderIndex)
                        .Select(x => x.PatrolArea.Name)
                        .FirstOrDefault(),
                    EndAreaName = t.PatrolRoute.PatrolRouteAreas
                        .OrderByDescending(x => x.OrderIndex)
                        .Select(x => x.PatrolArea.Name)
                        .FirstOrDefault()
                },
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
