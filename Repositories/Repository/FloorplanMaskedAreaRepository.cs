using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Repository.RepoModel;

namespace Repositories.Repository
{
    public class FloorplanMaskedAreaRepository : BaseRepository
    {
        public FloorplanMaskedAreaRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<List<AreaSummaryRM>> GetTopAreasAsync(int topCount = 5)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.FloorplanMaskedAreas
                .AsNoTracking()
                .Where(c => c.Status != 0);

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            return await q
                .OrderByDescending(x => x.UpdatedAt) 
                .Take(topCount)
                .Select(x => new AreaSummaryRM
                {
                    Id = x.Id,
                    Name = x.Name ?? "Unknown Area",
                })
                .ToListAsync();
        }

        public async Task<int> GetCountAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.FloorplanMaskedAreas
                .AsNoTracking()
                .Where(c => c.Status != 0);

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            return await q.CountAsync();
        }

        public async Task<FloorplanMaskedArea?> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable().Where(a => a.Id == id && a.Status != 0).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<FloorplanMaskedArea>> GetAllAsync()
        {
            return await GetAllQueryable().AsNoTracking().ToListAsync();
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

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var area = await query.FirstOrDefaultAsync();
            if (area == null)
                throw new KeyNotFoundException("Area not found");

            await _context.SaveChangesAsync();
        }

        public IQueryable<FloorplanMaskedArea> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.FloorplanMaskedAreas
                .Include(a => a.Floor)
                .Include(a => a.Floorplan)
                .Include(a => a.Application)
                .Where(a => a.Status != 0);

            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<FloorplanMaskedArea>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<MstFloor?> GetFloorByIdAsync(Guid floorId)
        {
            return await _context.MstFloors
                .WithActiveRelations()
                .FirstOrDefaultAsync(f => f.Id == floorId && f.Status != 0);
        }

        public async Task<MstFloorplan?> GetFloorplanByIdAsync(Guid floorplanId)
        {
            return await _context.MstFloorplans
                .WithActiveRelations()
                .FirstOrDefaultAsync(f => f.Id == floorplanId && f.Status != 0);
        }

        public async Task<List<FloorplanMaskedArea>> GetByFloorIdAsync(Guid floorId)
        {
            return await _context.FloorplanMaskedAreas
                .Where(ma => ma.FloorId == floorId && ma.Status != 0)
                .ToListAsync();
        }

        public async Task<List<FloorplanMaskedArea>> GetByFloorplanIdAsync(Guid floorplanId)
        {
            return await _context.FloorplanMaskedAreas
                .Where(ma => ma.FloorplanId == floorplanId && ma.Status != 0)
                .ToListAsync();
        }
        
                public async Task<List<Guid>> GetMaskedAreaIdsByLocationAsync(
            List<Guid>? buildingIds = null,
            List<Guid>? floorIds = null,
            List<Guid>?  floorplanIds = null)
        {
            var query = _context.FloorplanMaskedAreas
                .Include(m => m.Floorplan)
                    .ThenInclude(fp => fp.Floor)
                        .ThenInclude(f => f.Building)
                .AsQueryable();

            // 🔹 Filter by floorplan
            if (floorplanIds?.Any() == true)
            {
                query = query.Where(m => floorplanIds.Contains(m.FloorplanId));
            }

            // 🔹 Filter by floor
            if (floorIds?.Any() == true)
            {
                query = query.Where(m => floorIds.Contains(m.Floorplan.FloorId));
            }

            // 🔹 Filter by building
            if (buildingIds?.Any() == true)
            {
                query = query.Where(m => buildingIds.Contains(m.Floorplan.Floor.BuildingId));
            }

            // 🔹 Jika semua parameter kosong, return kosong agar tidak load semua area
            if ((buildingIds == null || !buildingIds.Any()) &&
                (floorIds == null || !floorIds.Any()) &&
                (floorplanIds == null || !floorplanIds.Any()))
            {
                return new List<Guid>();
            }

            return await query
                .Select(m => m.Id)
                .Distinct()
                .ToListAsync();
        }

    
    // single input
        //     public async Task<List<Guid>> GetMaskedAreaIdsByLocationAsync(
        //     Guid? buildingId,
        //     Guid? floorId,
        //     Guid? floorplanId)
        // {
        //     var query = _context.FloorplanMaskedAreas.AsQueryable();

        //     if (floorplanId.HasValue)
        //     {
        //         query = query.Where(m => m.FloorplanId == floorplanId.Value);
        //     }
        //     else if (floorId.HasValue)
        //     {
        //         query = query.Where(m => m.Floorplan.FloorId == floorId.Value);
        //     }
        //     else if (buildingId.HasValue)
        //     {
        //         query = query.Where(m => m.Floorplan.Floor.BuildingId == buildingId.Value);
        //     }
        //     else
        //     {
        //         return new List<Guid>(); // No filter
        //     }

        //     return await query
        //         .Select(m => m.Id)
        //         .Distinct()
        //         .ToListAsync();
        // }

    }
}
