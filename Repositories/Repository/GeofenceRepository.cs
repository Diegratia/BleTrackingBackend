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
    public class GeofenceRepository : BaseRepository
    {
        public GeofenceRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<Geofence> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
           .Where(a => a.Id == id && a.Status != 0)
           .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Geofence>> GetAllAsync()
        {
             return await GetAllQueryable().ToListAsync();
        }

        public async Task<Geofence> AddAsync(Geofence geofence)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                geofence.ApplicationId = applicationId.Value;
            }
            else if (geofence.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(geofence.ApplicationId);
            ValidateApplicationIdForEntity(geofence, applicationId, isSystemAdmin);

            _context.Geofences.Add(geofence);
            await _context.SaveChangesAsync();
            return geofence;
        }

        public async Task UpdateAsync(Geofence geofence)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(geofence.ApplicationId);
            ValidateApplicationIdForEntity(geofence, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Geofences
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var geofence = await query.FirstOrDefaultAsync();

            if (geofence == null)
                return;

            await _context.SaveChangesAsync();
        }

        public IQueryable<Geofence> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Geofences
            .Include(d => d.Floor)
            .Include(d => d.Floorplan)
            .Where(d => d.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }
    }
}
