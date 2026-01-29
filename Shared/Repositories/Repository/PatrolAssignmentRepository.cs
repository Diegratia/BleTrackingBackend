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
using Shared.Contracts;
using System.Security.Claims;
using Shared.Contracts.Shared.Contracts;
using Repositories.Extensions;

namespace Repositories.Repository
{
    public class PatrolAssignmentRepository : BaseRepository
    {
        public PatrolAssignmentRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        // public async Task<PatrolAssignment?> GetByIdAsync(Guid id)
        // {
        //     return await GetAllQueryable()
        //    .Where(a => a.Id == id && a.Status != 0)
        //    .FirstOrDefaultAsync();
        // }

        public async Task<IEnumerable<PatrolAssignmentRM>> GetAllAsync()
        {
            return await GetAllProjectedQueryable().ToListAsync();
        }

        public async Task<PatrolAssignment> AddAsync(PatrolAssignment patrolassignment)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                patrolassignment.ApplicationId = applicationId.Value;
            }
            else if (patrolassignment.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(patrolassignment.ApplicationId);
            ValidateApplicationIdForEntity(patrolassignment, applicationId, isSystemAdmin);

            _context.PatrolAssignments.Add(patrolassignment);
            await _context.SaveChangesAsync();
            return patrolassignment;
        }

        public async Task UpdateAsync(PatrolAssignment patrolassignment)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(patrolassignment.ApplicationId);
            ValidateApplicationIdForEntity(patrolassignment, applicationId, isSystemAdmin);
            await _context.SaveChangesAsync();
        }




        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolAssignments
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var patrolassignment = await query.FirstOrDefaultAsync();

            if (patrolassignment == null)
                return;

            await _context.SaveChangesAsync();
        }

        // Di PatrolAssignmentRepository
        public async Task<PatrolAssignmentRM?> GetByIdAsync(Guid id)
        {
            return await GetAllProjectedQueryable()
            .AsNoTracking()
            .Where(a => a.Id == id && a.Status != 0)
            .FirstOrDefaultAsync();
        }

        public async Task<PatrolAssignment?> GetByIdWithTrackingAsync(Guid id)
        {
            return await GetAllQueryable()
                .Where(a => a.Id == id && a.Status != 0)
                .FirstOrDefaultAsync();
        }

        // public IQueryable<PatrolAssignment> GetAllQueryable()
        // {

        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
        //     var query = _context.PatrolAssignments
        //     .Include(d => d.PatrolRoute)
        //     .Include(d => d.PatrolAssignmentSecurities)
        //         .ThenInclude(pas => pas.Security)
        //     .Where(d => d.Status != 0);
        //     return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        // }
        public IQueryable<PatrolAssignment> GetAllQueryable()
        {
            var userEmail = GetUserEmail();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var isSuperAdmin = IsSuperAdmin();
            var isPrimaryAdmin = IsPrimaryAdmin();

            var query = _context.PatrolAssignments
                .Include(d => d.PatrolRoute)
                .Include(d => d.TimeGroup)
                .Include(d => d.PatrolAssignmentSecurities)
                    .ThenInclude(pas => pas.Security)
                .Where(d => d.Status != 0);

            if (!isSystemAdmin && !isSuperAdmin && !isPrimaryAdmin)
            {
                query = query.Where(pa =>
                    pa.PatrolAssignmentSecurities.Any(pas =>
                        pas.Security.Email == userEmail
                    )
                );
            }

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public IQueryable<PatrolAssignment> GetAllQueryableWithoutTracking()
        {
            var userEmail = GetUserEmail();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var isSuperAdmin = IsSuperAdmin();
            var isPrimaryAdmin = IsPrimaryAdmin();
            var query = _context.PatrolAssignments
            .Include(d => d.PatrolRoute)
            .Include(d => d.TimeGroup)
            .Include(d => d.PatrolAssignmentSecurities)
                .ThenInclude(pas => pas.Security)
                    .ThenInclude(s => s.Organization)
            .Include(d => d.PatrolAssignmentSecurities)
                .ThenInclude(pas => pas.Security)
                    .ThenInclude(s => s.Department)
            .Include(d => d.PatrolAssignmentSecurities)
                .ThenInclude(pas => pas.Security)
                    .ThenInclude(s => s.District)
            .Where(d => d.Status != 0);
            if (!isSystemAdmin && !isSuperAdmin && !isPrimaryAdmin)
            {
                query = query.Where(pa =>
                    pa.PatrolAssignmentSecurities.Any(pas =>
                        pas.Security.Email == userEmail
                    )
                );
            }
            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<List<PatrolAssignmentLookUpRM>> GetAllLookUpAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolRoutes
            .AsNoTracking()
            .Where(ca => ca.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var projected = query.Select(ca => new PatrolAssignmentLookUpRM
            {
                Id = ca.Id,
                Name = ca.Name,
                Description = ca.Description,
                ApplicationId = ca.ApplicationId
            });
            return await projected.ToListAsync();
        }

        //Projection Query


    public async Task<(List<PatrolAssignmentRM> Data, int Total, int Filtered)>
        FilterAsync(PatrolAssignmentFilter filter)
    {
        var query = GetAllProjectedQueryable();

        var total = await query.CountAsync();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(x =>
                x.Name!.ToLower().Contains(search) ||
                x.Description!.ToLower().Contains(search)
            );
        }

        if (filter.PatrolRouteId.HasValue)
            query = query.Where(x => x.PatrolRouteId == filter.PatrolRouteId);

        if (filter.TimeGroupId.HasValue)
            query = query.Where(x => x.TimeGroupId == filter.TimeGroupId);

        var filtered = await query.CountAsync();

        // 🔥 PROJECTION

        query = query.ApplySorting(filter.SortColumn, filter.SortDir);
        query = query.ApplyPaging(filter.Page, filter.PageSize);

        var data = await query.ToListAsync();
        return (data, total, filtered);
    }

            private IQueryable<PatrolAssignmentRM> GetAllProjectedQueryable()
        {
            var userEmail = GetUserEmail();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var isSuperAdmin = IsSuperAdmin();
            var isPrimaryAdmin = IsPrimaryAdmin();

            var query = _context.PatrolAssignments
                .AsNoTracking()
                .Where(x => x.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            // 🔐 Role-based filter
            if (!isSystemAdmin && !isSuperAdmin && !isPrimaryAdmin)
            {
                query = query.Where(pa =>
                    pa.PatrolAssignmentSecurities.Any(pas =>
                        pas.Security.Email == userEmail
                    )
                );
            }

            // 🔥 FULL PROJECTION
            return query.Select(pa => new PatrolAssignmentRM
            {
                Id = pa.Id,
                Name = pa.Name,
                Description = pa.Description,
                PatrolRouteId = pa.PatrolRouteId,
                TimeGroupId = pa.TimeGroupId,
                StartDate = pa.StartDate,
                EndDate = pa.EndDate,
                Status = pa.Status,
                CreatedAt = pa.CreatedAt,
                UpdatedAt = pa.UpdatedAt,
                CreatedBy = pa.CreatedBy,
                UpdatedBy = pa.UpdatedBy,
                ApplicationId = pa.ApplicationId,

                PatrolRoute = pa.PatrolRoute == null ? null : new PatrolRouteLookUpRM
                {
                    Id = pa.PatrolRoute.Id,
                    Name = pa.PatrolRoute.Name,
                    Description = pa.PatrolRoute.Description,
                    StartAreaName = pa.PatrolRoute.PatrolRouteAreas
                        .Where(x => x.status != 0)
                        .OrderBy(x => x.OrderIndex)
                        .Select(x => x.PatrolArea.Name)
                        .FirstOrDefault(),
                    EndAreaName = pa.PatrolRoute.PatrolRouteAreas
                        .Where(x => x.status != 0)
                        .OrderByDescending(x => x.OrderIndex)
                        .Select(x => x.PatrolArea.Name)
                        .FirstOrDefault()
                },

                TimeGroup = pa.TimeGroup == null ? null : new AssignmentTimeGroupRM
                {
                    Id = pa.TimeGroup.Id,
                    Name = pa.TimeGroup.Name,
                    ScheduleType = pa.TimeGroup.ScheduleType.ToString(),
                    TimeBlocks = pa.TimeGroup.TimeBlocks
                        .Where(x => x.Status != 0)
                        .Select(x => new TimeBlockRM
                        {
                            Id = x.Id,
                            DayOfWeek = x.DayOfWeek,
                            StartTime = x.StartTime,
                            EndTime = x.EndTime
                        })
                        .ToList()
                },

                Securities = pa.PatrolAssignmentSecurities
                    .Where(x => x.Status != 0)
                    .Select(x => new SecurityListRM
                    {
                        Id = x.Security.Id,
                        Name = x.Security.Name,
                        CardNumber = x.Security.CardNumber,
                        IdentityId = x.Security.IdentityId,
                        OrganizationName = x.Security.Organization.Name,
                        DepartmentName = x.Security.Department.Name,
                        DistrictName = x.Security.District.Name
                    })
                    .ToList()
            });
        }



        //Helpers

        public async Task RemoveAllPatrolAssignmentSecurities(Guid assignmentId)
        {
            await _context.PatrolAssignmentSecurities
                .Where(x => x.PatrolAssignmentId == assignmentId)
                .ExecuteDeleteAsync();
        }

        public async Task AddPatrolAssignmentSecurityAsync(PatrolAssignmentSecurity entity)
        {
            await _context.PatrolAssignmentSecurities.AddAsync(entity);
        }

        // public async Task UpdateScalarAsync(PatrolAssignment assignment)
        // {
        //     _context.PatrolAssignments.Update(assignment);
        //     await _context.SaveChangesAsync();
        // }


        public void Attach(PatrolAssignment entity)
        {
            _context.Attach(entity);
        }


        public async Task<IReadOnlyCollection<Guid>> GetMissingSecurityIdsAsync(
    IEnumerable<Guid> ids
    )
        {
            var idList = ids.Distinct().ToList();
            if (!idList.Any())
                return Array.Empty<Guid>();

            var existingIds = await _context.MstSecurities
                .Where(x => idList.Contains(x.Id) && x.Status != 0)
                .Select(x => x.Id)
                .ToListAsync();

            return idList.Except(existingIds).ToList();
        }

        public async Task<bool> PatrolRouteExistsAsync(Guid patrolRouteId)
        {
            return await _context.PatrolRoutes
                .AnyAsync(f => f.Id == patrolRouteId && f.Status != 0);
        }
        public async Task<bool> TimeGroupExistsAsync(Guid timeGroupId)
        {
            return await _context.TimeGroups
                .AnyAsync(f => f.Id == timeGroupId && f.Status != 0);
        }
        public async Task<bool> CheckTimeGroupPatrolType(Guid timeGroupId)
        {
            return await _context.TimeGroups
                .AnyAsync(f => f.Id == timeGroupId && f.Status != 0 && f.ScheduleType == ScheduleType.Patrol);
        }

        public async Task RemoveAssignmentSecurities(Guid patrolAssignmentId)
        {
            var rows = _context.PatrolAssignmentSecurities
                .Where(x => x.PatrolAssignmentId == patrolAssignmentId);

            _context.PatrolAssignmentSecurities.RemoveRange(rows);
            await _context.SaveChangesAsync();
        }


        public async Task<IReadOnlyCollection<Guid>> GetInvalidSecurityIdsByApplicationAsync(
    IEnumerable<Guid> securityIds,
    Guid applicationId
        )
        {
            var ids = securityIds.Distinct().ToList();
            if (!ids.Any())
                return Array.Empty<Guid>();

            var validIds = await _context.MstSecurities
                .Where(s =>
                    ids.Contains(s.Id) &&
                    s.ApplicationId == applicationId &&
                    s.Status != 0
                )
                .Select(s => s.Id)
                .ToListAsync();

            return ids.Except(validIds).ToList();
        }
        
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidRouteOwnershipAsync(
            Guid routeId,
            Guid applicationId
        )
        {
            return await CheckInvalidOwnershipIdsAsync<PatrolRoute>(
                new[] { routeId },
                applicationId
            );
        }
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidTGOwnershipAsync(
            Guid timeGroupId,
            Guid applicationId
        )
        {
            return await CheckInvalidOwnershipIdsAsync<TimeGroup>(
                new[] { timeGroupId },
                applicationId
            );
        }
    }
}
