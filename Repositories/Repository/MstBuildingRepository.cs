    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Entities.Models;
    using Microsoft.EntityFrameworkCore;
    using Repositories.DbContexts;
    using Microsoft.AspNetCore.Http;
    using System.Security.Claims;

namespace Repositories.Repository
{
    public class MstBuildingRepository : BaseRepository
        {

            public MstBuildingRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
                : base(context, httpContextAccessor)
            {
            }

            public async Task<MstBuilding?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstBuildings
                .Where(d => d.Id == id && d.Status != 0);

            query = query.WithActiveRelations();

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync();
        }

          public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstBuildings
                .Where(d => d.Id == id && d.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var district = await query.FirstOrDefaultAsync();
            if (district == null)
                throw new KeyNotFoundException("Building not found");

            await _context.SaveChangesAsync();
        }
            

        public async Task<IEnumerable<MstBuilding>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<MstBuilding> AddAsync(MstBuilding building)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            // non system ambil dari claim
            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");
                building.ApplicationId = applicationId.Value;
            }
            // admin set applciation di body
            else if (building.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }
            await ValidateApplicationIdAsync(building.ApplicationId);
            ValidateApplicationIdForEntity(building, applicationId, isSystemAdmin);

            _context.MstBuildings.Add(building);
            await _context.SaveChangesAsync();
            return building;
        }

         public async Task UpdateAsync(MstBuilding building)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            await ValidateApplicationIdAsync(building.ApplicationId);
            ValidateApplicationIdForEntity(building, applicationId, isSystemAdmin);

            // _context.MstDistricts.Update(district);
            await _context.SaveChangesAsync();
        }

        public IQueryable<MstBuilding> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstBuildings
                .Where(d => d.Status != 0);

            query = query.WithActiveRelations();
            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<MstBuilding>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }
    }
}