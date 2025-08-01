using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class FloorplanMaskedAreaRepository : BaseRepository
    {
        public FloorplanMaskedAreaRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<FloorplanMaskedArea?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.FloorplanMaskedAreas
                .Include(a => a.Floor)
                .Include(a => a.Floorplan)
                .Where(a => a.Id == id && a.Status != 0);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<FloorplanMaskedArea>> GetAllAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.FloorplanMaskedAreas
                .Include(a => a.Floor)
                .Include(a => a.Floorplan)
                .Where(a => a.Status != 0);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).ToListAsync();
        }

        public async Task<FloorplanMaskedArea> AddAsync(FloorplanMaskedArea area)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin users.");
                area.ApplicationId = applicationId.Value;
            }
            else if (area.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(area.ApplicationId);
            ValidateApplicationIdForEntity(area, applicationId, isSystemAdmin);
            _context.FloorplanMaskedAreas.Add(area);
            await _context.SaveChangesAsync();

            return area;
        }

        public async Task UpdateAsync(FloorplanMaskedArea area)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(area.ApplicationId);
            ValidateApplicationIdForEntity(area, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.FloorplanMaskedAreas
                .Where(a => a.Id == id && a.Status != 0);

            var area = await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();

            if (area == null)
                throw new KeyNotFoundException("Masked area not found or unauthorized access.");

            await _context.SaveChangesAsync();
        }

        public IQueryable<FloorplanMaskedArea> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.FloorplanMaskedAreas
                .Include(a => a.Floor)
                .Include(a => a.Floorplan)
                .Where(a => a.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<FloorplanMaskedArea>> GetAllExportAsync()
        {
             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.FloorplanMaskedAreas
                .Where(d => d.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.ToListAsync();
        }

        public async Task<MstFloor?> GetFloorByIdAsync(Guid floorId)
        {
            return await _context.MstFloors
                .FirstOrDefaultAsync(f => f.Id == floorId && f.Status != 0);
        }

        public async Task<MstFloorplan?> GetFloorplanByIdAsync(Guid floorplanId)
        {
            return await _context.MstFloorplans
                .FirstOrDefaultAsync(f => f.Id == floorplanId && f.Status != 0);
        }
    }
}
