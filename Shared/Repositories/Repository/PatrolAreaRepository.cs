using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Extensions;
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class PatrolAreaRepository : BaseRepository
    {
        public PatrolAreaRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<PatrolAreaRead?> GetByIdAsync(Guid id)
        {
            var query = GetAllQueryable().Where(a => a.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<PatrolArea?> GetByIdEntityAsync(Guid id)
        {
            return await GetAllQueryable().FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<PatrolAreaRead>> GetAllAsync()
        {
            return await ProjectToRead(GetAllQueryable()).ToListAsync();
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

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            // Apply building access filter for non-system/super admin users
            query = ApplyBuildingFilterIfNonSystemAdmin(query, d => d.Floor?.BuildingId);

            return query;
        }

        public async Task<List<PatrolAreaLookUpRM>> GetAllLookUpAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.PatrolAreas
            .AsNoTracking()
            .Where(d => d.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

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

        public async Task<List<PatrolArea>> GetByFloorplanIdAsync(Guid floorplanId)
        {
            return await _context.PatrolAreas
                .Where(ma => ma.FloorplanId == floorplanId && ma.Status != 0)
                .ToListAsync();
        }

        public async Task<bool> FloorExistsAsync(Guid floorId)
        {
            return await _context.MstFloors
                .AnyAsync(f => f.Id == floorId && f.Status != 0);
        }

        public async Task<bool> FloorplanExistsAsync(Guid floorplanId)
        {
            return await _context.MstFloorplans
                .AnyAsync(f => f.Id == floorplanId && f.Status != 0);
        }


        public async Task<IReadOnlyCollection<Guid>> CheckInvalidFloorplanOwnershipAsync(
            Guid floorplanId,
            Guid applicationId
        )
        {
            return await CheckInvalidOwnershipIdsAsync<MstFloorplan>(
                new[] { floorplanId },
                applicationId
            );
        }

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidFloorOwnershipAsync(
            Guid floorId,
            Guid applicationId
        )
        {
            return await CheckInvalidOwnershipIdsAsync<MstFloor>(
                new[] { floorId },
                applicationId
            );
        }

        public async Task<(List<PatrolAreaRead> Data, int Total, int Filtered)> FilterAsync(PatrolAreaFilter filter)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            // Base query with tenant filter
            var query = _context.PatrolAreas.AsQueryable();
            query = query.Where(x => x.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var total = await query.CountAsync();

            // 1. Specific Filtering
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(s));
            }

            var floorIds = ExtractIds(filter.FloorId);
            if (floorIds.Count > 0)
                query = query.Where(x => x.FloorId.HasValue && floorIds.Contains(x.FloorId.Value));

            var floorplanIds = ExtractIds(filter.FloorplanId);
            if (floorplanIds.Count > 0)
                query = query.Where(x => x.FloorplanId.HasValue && floorplanIds.Contains(x.FloorplanId.Value));

            if (filter.IsActive.HasValue)
                query = query.Where(x => x.IsActive == filter.IsActive);

            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status);

            var filtered = await query.CountAsync();

            // 2. Sorting & Paging
            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            // 3. Manual Projection
            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        private IQueryable<PatrolAreaRead> ProjectToRead(IQueryable<PatrolArea> query)
        {
            return query.AsNoTracking().Select(t => new PatrolAreaRead
            {
                Id = t.Id,
                Name = t.Name,
                Remarks = t.Remarks,
                AreaShape = t.AreaShape,
                Color = t.Color,
                FloorplanId = t.FloorplanId,
                FloorplanName = t.Floorplan.Name,
                FloorId = t.FloorId,
                FloorName = t.Floor.Name,
                IsActive = t.IsActive,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                CreatedBy = t.CreatedBy,
                UpdatedAt = t.UpdatedAt,
                UpdatedBy = t.UpdatedBy,
                ApplicationId = t.ApplicationId
            });
        }
    }
}
