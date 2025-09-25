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
    public class OverpopulatingRepository : BaseRepository
    {
        public OverpopulatingRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<Overpopulating> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
           .Where(a => a.Id == id && a.Status != 0)
           .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Overpopulating>> GetAllAsync()
        {
             return await GetAllQueryable().ToListAsync();
        }

        public async Task<Overpopulating> AddAsync(Overpopulating overpopulating)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                overpopulating.ApplicationId = applicationId.Value;
            }
            else if (overpopulating.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(overpopulating.ApplicationId);
            ValidateApplicationIdForEntity(overpopulating, applicationId, isSystemAdmin);

            _context.Overpopulatings.Add(overpopulating);
            await _context.SaveChangesAsync();
            return overpopulating;
        }

        public async Task UpdateAsync(Overpopulating overpopulating)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(overpopulating.ApplicationId);
            ValidateApplicationIdForEntity(overpopulating, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Boundarys
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var overpopulating = await query.FirstOrDefaultAsync();

            if (overpopulating == null)
                return;

            await _context.SaveChangesAsync();
        }

        public IQueryable<Overpopulating> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Overpopulatings
            .Include(d => d.Floor)
            .Include(d => d.Floorplan)
            .Where(d => d.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }
    }
}
