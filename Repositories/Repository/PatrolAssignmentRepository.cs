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

        public async Task<IEnumerable<PatrolAssignment>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
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
        public async Task<PatrolAssignment?> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
            .AsNoTracking()  // ✅ 
            .Where(a => a.Id == id && a.Status != 0)
            .FirstOrDefaultAsync();
        }

        public async Task<PatrolAssignment?> GetByIdWithTrackingAsync(Guid id)
        {
            return await GetAllQueryable()
                .Where(a => a.Id == id && a.Status != 0)
                .FirstOrDefaultAsync();
        }

        public IQueryable<PatrolAssignment> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.PatrolAssignments
            .Include(d => d.PatrolRoute)
            .Include(d => d.PatrolAssignmentSecurities)
                .ThenInclude(pas => pas.Security)
            .Where(d => d.Status != 0);
            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

            public IQueryable<PatrolAssignment> GetAllQueryableWithoutTracking()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.PatrolAssignments
            .Include(d => d.PatrolRoute)
            .Include(d => d.PatrolAssignmentSecurities)
                .ThenInclude(pas => pas.Security)
            .AsNoTracking()
            .Where(d => d.Status != 0);
            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

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



        public async Task<List<PatrolAssignmentLookUpRM>> GetAllLookUpAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.PatrolAssignments
            .AsNoTracking()
            .Where(d => d.Status != 0);

            var projected = query.Select(t => new PatrolAssignmentLookUpRM
            {
                Id = t.Id,
                Name = t.Name,
            });
            return await projected.ToListAsync();
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

    
    }
}
