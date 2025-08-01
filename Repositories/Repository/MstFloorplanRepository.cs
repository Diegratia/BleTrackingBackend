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
    public class MstFloorplanRepository : BaseRepository
    {
        public MstFloorplanRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<MstFloorplan> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable().Where(fp => fp.Id == id && fp.Status != 0).FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Floorplan not found");
        }

        public async Task<IEnumerable<MstFloorplan>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<MstFloorplan> AddAsync(MstFloorplan floorplan)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            // Role-based ApplicationId handling
            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                floorplan.ApplicationId = applicationId.Value;
            }
            else if (floorplan.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(floorplan.ApplicationId);
            ValidateApplicationIdForEntity(floorplan, applicationId, isSystemAdmin);

            // Validasi Floor
            await ValidateFloorOwnershipAsync(floorplan.FloorId, floorplan.ApplicationId);

    
            _context.MstFloorplans.Add(floorplan);
            await _context.SaveChangesAsync();
            return floorplan;
        }

        public async Task UpdateAsync(MstFloorplan floorplan)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(floorplan.ApplicationId);
            ValidateApplicationIdForEntity(floorplan, applicationId, isSystemAdmin);
            await ValidateFloorOwnershipAsync(floorplan.FloorId, floorplan.ApplicationId);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstFloorplans
                .Where(f => f.Id == id && f.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var floorplan = await query.FirstOrDefaultAsync();
            if (floorplan == null)
                throw new KeyNotFoundException("Floorplan not found or access denied");

            await _context.SaveChangesAsync();
        }

        public async Task<MstFloor> GetFloorByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstFloors
                .Where(f => f.Id == id && f.Status != 0);
            query = query.WithActiveRelations();
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MstFloorplan>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public IQueryable<MstFloorplan> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstFloorplans
                .Include(f => f.Floor)
                .Where(f => f.Status != 0);
            
            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public IQueryable<object> GetAllQueryableWithMaskedAreaCount()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstFloorplans
                .Include(f => f.Floor)
                .Include(f => f.FloorplanMaskedAreas)
                .Where(f => f.Status != 0);
            
            query = query.WithActiveRelations();
            
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return query.Select(f => new
            {
                Entity = f,
                MaskedAreaCount = f.FloorplanMaskedAreas.Count()
            }).AsNoTracking();
        }
        

        private async Task ValidateFloorOwnershipAsync(Guid floorId, Guid appId)
        {
            var floor = await _context.MstFloors
                .FirstOrDefaultAsync(f => f.Id == floorId && f.Status != 0 && f.ApplicationId == appId);

            if (floor == null)
                throw new ArgumentException($"Floor with ID {floorId} not found or not part of the application");
        }
    }
}
