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

namespace Repositories.Repository
{
    public class PatrolCaseRepository : BaseRepository
    {
        public PatrolCaseRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<PatrolCaseRM?> GetByIdAsync(Guid id)
        {
            return await GetAllProjectedQueryable()
            .Where(a => a.Id == id )
            .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PatrolCaseRM>> GetAllAsync()
        {
            return await GetAllProjectedQueryable().ToListAsync();
        }

        public async Task<PatrolCase> AddAsync(PatrolCase patrolCase)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                patrolCase.ApplicationId = applicationId.Value;
            }
            else if (patrolCase.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(patrolCase.ApplicationId);
            ValidateApplicationIdForEntity(patrolCase, applicationId, isSystemAdmin);

            _context.PatrolCases.Add(patrolCase);
            await _context.SaveChangesAsync();
            return patrolCase;
        }

        public async Task UpdateAsync(PatrolCase patrolCase)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(patrolCase.ApplicationId);
            ValidateApplicationIdForEntity(patrolCase, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolCases
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var patrolCase = await query.FirstOrDefaultAsync();

            if (patrolCase == null)
                return;

            await _context.SaveChangesAsync();
        }


        // public IQueryable<PatrolCase> GetAllQueryable()
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

        //     var query = _context.PatrolCases
        //     .Include(d => d.Security)
        //     .Include(d => d.ApprovedByHead)
        //     .Include(d => d.PatrolSession)
        //     .Include(d => d.PatrolRoute)
        //     .Include(d => d.PatrolCaseAttachments)
        //     .Where(d => d.Status != 0);

        //     return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        // }

        // public async Task<List<PatrolAreaLookUpRM>> GetAllLookUpAsync()
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
        //     var query = _context.PatrolCases
        //     .AsNoTracking()
        //     .Where(d => d.Status != 0);
        //     query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

        //     var projected = query.Select(t => new PatrolAreaLookUpRM
        //     {
        //         Id = t.Id,
        //         Name = t.Name,
        //         Color = t.Color,
        //         FloorName = t.Floor.Name,
        //         FloorplanName = t.Floorplan.Name,
        //         IsActive = t.IsActive
        //     }); 
        //     return await projected.ToListAsync();
        // }

        public IQueryable<PatrolCaseRM> GetAllProjectedQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolCases
                .AsNoTracking()
                .Where(fd => fd.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var projected = query.Select(t => new PatrolCaseRM
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                CaseType = t.CaseType,
                CaseStatus = t.CaseStatus,
                PatrolSessionId = t.PatrolSessionId,
                SecurityId = t.SecurityId,
                ApprovedByHeadId = t.ApprovedByHeadId,
                PatrolAssignmentId = t.PatrolAssignmentId,
                PatrolRouteId = t.PatrolRouteId,
                ApplicationId = t.ApplicationId,
                Security = t.Security == null ? null : new MstSecurityLookUpRM
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
                PatrolAssignment = t.PatrolAssignment == null ? null : new PatrolAssignmentLookUpRM
                {   
                    Id = t.PatrolAssignment.Id,
                    Name = t.PatrolAssignment.Name,
                    Description = t.PatrolAssignment.Description,
                },
                PatrolRoute = t.PatrolRoute == null ? null : new PatrolRouteMinimalRM
                {   
                    Id = t.PatrolRoute.Id,
                    Name = t.PatrolRoute.Name,
                    Description = t.PatrolRoute.Description,
                },
            });

            return projected;
        }

        public async Task<(List<PatrolCaseRM> Data, int Total, int Filtered)> FilterAsync(
            PatrolCaseFilter filter
        )
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            // 1. Base Query (Filtered by ApplicationId & Active Status)
            var query = _context.PatrolCases
                .AsNoTracking()
                .Where(x => x.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            // 2. Count Total (Before Filter)
            var total = await query.CountAsync();

            // 3. Apply Filters
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x =>
                    x.Title.ToLower().Contains(search) ||
                    x.Description.ToLower().Contains(search) 
                );
            }

            if (filter.CaseType.HasValue)
                query = query.Where(x => x.CaseType == filter.CaseType.Value);

            if (filter.CaseStatus.HasValue)
                query = query.Where(x => x.CaseStatus == filter.CaseStatus.Value);

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

            // 4. Count Filtered
            var filtered = await query.CountAsync();

            // 5. Projection (Manual Select) - Include relationships here
            var projectedQuery = query.Select(t => new PatrolCaseRM
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                CaseType = t.CaseType,
                CaseStatus = t.CaseStatus,
                PatrolSessionId = t.PatrolSessionId,
                SecurityId = t.SecurityId,
                ApprovedByHeadId = t.ApprovedByHeadId,
                PatrolAssignmentId = t.PatrolAssignmentId,
                PatrolRouteId = t.PatrolRouteId,
                ApplicationId = t.ApplicationId,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Security = t.Security == null ? null : new MstSecurityLookUpRM
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
                PatrolAssignment = t.PatrolAssignment == null ? null : new PatrolAssignmentLookUpRM
                {
                    Id = t.PatrolAssignment.Id,
                    Name = t.PatrolAssignment.Name,
                    Description = t.PatrolAssignment.Description,
                },
                PatrolRoute = t.PatrolRoute == null ? null : new PatrolRouteMinimalRM
                {
                    Id = t.PatrolRoute.Id,
                    Name = t.PatrolRoute.Name,
                    Description = t.PatrolRoute.Description,
                }
            });

            // 6. Sorting & Paging
            // Note: Make sure to include Repositories.Extensions namespace
            projectedQuery = projectedQuery.ApplySorting(filter.SortColumn, filter.SortDir);
            projectedQuery = projectedQuery.ApplyPaging(filter.Page, filter.PageSize);

            var data = await projectedQuery.ToListAsync();

            return (data, total, filtered);
        }



        
        
            public async Task<List<PatrolCase>> GetBySessionIdAsync(Guid sessionId)
        {
            return await _context.PatrolCases
                .Where(ma => ma.PatrolSessionId == sessionId && ma.Status != 0)
                .ToListAsync();
        }

        public async Task<bool> SessionExistsAsync(Guid sessionId)
        {
            return await _context.PatrolCases
                .AnyAsync(f => f.Id == sessionId && f.Status != 0);
        }

    }
}
