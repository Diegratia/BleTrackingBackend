using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;
using Repositories.Repository;

namespace Repositories.Repository
{
    public class MstFloorRepository : BaseRepository
    {
        public MstFloorRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<MstFloor?> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable().Where(f => f.Id == id && f.Status != 0).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MstFloor>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<MstFloor> AddAsync(MstFloor floor)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId missing");

                floor.ApplicationId = applicationId.Value;
            }
            else if (floor.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("SystemAdmin Must provide ApplicationId");
            }

            await ValidateApplicationIdAsync(floor.ApplicationId);
            ValidateApplicationIdForEntity(floor, applicationId, isSystemAdmin);

            _context.MstFloors.Add(floor);
            await _context.SaveChangesAsync();

            return floor;
        }

        public async Task UpdateAsync(MstFloor floor)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(floor.ApplicationId);
            ValidateApplicationIdForEntity(floor, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstFloors
                .Where(f => f.Id == id && f.Status != 0);

            var floor = await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();

            if (floor == null)
                throw new KeyNotFoundException("Floor not found or unauthorized");

            await _context.SaveChangesAsync();
        }

        public IQueryable<MstFloor> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstFloors
                .Include(f => f.Building)
                .Where(f => f.Status != 0);

            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<MstBuilding?> GetBuildingByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstBuildings
                .Where(b => b.Id == id && b.Status != 0);
                query = query.WithActiveRelations();
            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MstFloor>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }
    }
}
