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
    public class PatrolAreaRepository : BaseRepository
    {
        public PatrolAreaRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<PatrolArea> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
           .Where(a => a.Id == id && a.Status != 0)
           .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PatrolArea>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<PatrolArea> AddAsync(PatrolArea patrolarea)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                patrolarea.ApplicationId = applicationId.Value;
            }
            else if (patrolarea.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(patrolarea.ApplicationId);
            ValidateApplicationIdForEntity(patrolarea, applicationId, isSystemAdmin);

            _context.PatrolAreas.Add(patrolarea);
            await _context.SaveChangesAsync();
            return patrolarea;
        }

        public async Task UpdateAsync(PatrolArea patrolarea)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(patrolarea.ApplicationId);
            ValidateApplicationIdForEntity(patrolarea, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolAreas
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var patrolarea = await query.FirstOrDefaultAsync();

            if (patrolarea == null)
                return;

            await _context.SaveChangesAsync();
        }


        public IQueryable<PatrolArea> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolAreas
            .Include(d => d.Floor)
            .Include(d => d.Floorplan)
            .Where(d => d.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<List<PatrolAreaLookUpRM>> GetAllLookUpAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.PatrolAreas
            .AsNoTracking()
            .Where(d => d.Status != 0);

            var projected = query.Select(t => new PatrolAreaLookUpRM
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color,
                FloorName = t.Floor.Name,
                FloorplanName = t.Floorplan.Name,
                IsActive = t.IsActive
            }); 
            return await projected.ToListAsync();
        }

        public async Task<bool> FloorExistsAsync(Guid floorId)
        {
            return await _context.MstFloors
                .AnyAsync(f => f.Id == floorId && f.Status !=0);
        }

        public async Task<bool> FloorplanExistsAsync(Guid floorplanId)
        {
            return await _context.MstFloorplans
                .AnyAsync(f => f.Id == floorplanId && f.Status != 0);
        }

    }
}
