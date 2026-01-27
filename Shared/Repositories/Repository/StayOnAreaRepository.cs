using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class StayOnAreaRepository : BaseRepository
    {
        public StayOnAreaRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<StayOnArea> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
           .Where(a => a.Id == id && a.Status != 0)
           .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<StayOnArea>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<StayOnArea> AddAsync(StayOnArea onarea)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                onarea.ApplicationId = applicationId.Value;
            }
            else if (onarea.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(onarea.ApplicationId);
            ValidateApplicationIdForEntity(onarea, applicationId, isSystemAdmin);

            _context.StayOnAreas.Add(onarea);
            await _context.SaveChangesAsync();
            return onarea;
        }

        public async Task UpdateAsync(StayOnArea onarea)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(onarea.ApplicationId);
            ValidateApplicationIdForEntity(onarea, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.StayOnAreas
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var onarea = await query.FirstOrDefaultAsync();

            if (onarea == null)
                return;

            await _context.SaveChangesAsync();
        }

        public IQueryable<StayOnArea> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.StayOnAreas
            .Include(d => d.Floor)
            .Include(d => d.Floorplan)
            .Where(d => d.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }
        

            public async Task<List<StayOnArea>> GetByFloorplanIdAsync(Guid floorplanId)
        {
             return await _context.StayOnAreas
                        .Where(ma => ma.FloorplanId == floorplanId && ma.Status != 0)
                        .ToListAsync();
                }
    }
}
