using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class EvacuationAssemblyPointRepository : BaseRepository
    {
        public EvacuationAssemblyPointRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public IQueryable<EvacuationAssemblyPoint> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.EvacuationAssemblyPoints
                .Include(e => e.Floorplan)
                    .ThenInclude(f => f!.Floor)
                .Where(e => e.Status != 0)
                .AsSplitQuery();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private IQueryable<EvacuationAssemblyPointRead> ProjectToRead(IQueryable<EvacuationAssemblyPoint> query)
        {
            return query
                .Select(e => new EvacuationAssemblyPointRead
                {
                    Id = e.Id,
                    Name = e.Name,
                    AreaShape = e.AreaShape,
                    Color = e.Color,
                    Remarks = e.Remarks,
                    FloorplanId = e.FloorplanId,
                    FloorId = e.FloorId,
                    IsActive = e.IsActive,
                    CreatedBy = e.CreatedBy,
                    CreatedAt = e.CreatedAt,
                    UpdatedBy = e.UpdatedBy,
                    UpdatedAt = e.UpdatedAt,
                    Status = e.Status,
                    ApplicationId = e.ApplicationId,
                    FloorplanName = e.Floorplan != null ? e.Floorplan.Name : null,
                    FloorName = e.Floorplan != null && e.Floorplan.Floor != null ? e.Floorplan.Floor.Name : null,
                    BuildingName = e.Floorplan != null && e.Floorplan.Floor != null ? e.Floorplan.Floor.Building.Name : null
                });
        }

        public async Task<EvacuationAssemblyPointRead?> GetByIdAsync(Guid id)
        {
            return await ProjectToRead(BaseEntityQuery())
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<EvacuationAssemblyPoint?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<EvacuationAssemblyPointRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<List<EvacuationAssemblyPointRead>> GetByFloorplanIdAsync(Guid floorplanId)
        {
            return await ProjectToRead(BaseEntityQuery())
                .Where(e => e.FloorplanId == floorplanId)
                .ToListAsync();
        }

        public async Task<(List<EvacuationAssemblyPointRead> Data, int Total, int Filtered)> FilterAsync(EvacuationAssemblyPointFilter filter)
        {
            var query = BaseEntityQuery();

            // Apply search
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(x => x.Name.ToLower().Contains(filter.Search.ToLower()));
            }

            // Apply filters
            var floorIds = ExtractIds(filter.FloorId);
            if (floorIds.Count > 0)
            {
                query = query.Where(x => x.FloorId.HasValue && floorIds.Contains(x.FloorId.Value));
            }

            var floorplanIds = ExtractIds(filter.FloorplanId);
            if (floorplanIds.Count > 0)
            {
                query = query.Where(x => x.FloorplanId.HasValue && floorplanIds.Contains(x.FloorplanId.Value));
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == filter.IsActive.Value);
            }

            var total = await query.CountAsync();
            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir ?? "desc");
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        public async Task AddAsync(EvacuationAssemblyPoint entity)
        {
            _context.EvacuationAssemblyPoints.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EvacuationAssemblyPoint entity)
        {
            _context.EvacuationAssemblyPoints.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.EvacuationAssemblyPoints.FindAsync(id);
            if (entity != null)
            {
                entity.Status = 0;
                await _context.SaveChangesAsync();
            }
        }

        // Ownership validation methods
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidFloorOwnershipAsync(
            Guid floorId, Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstFloor>(
                new[] { floorId }, applicationId);
        }

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidFloorplanOwnershipAsync(
            Guid floorplanId, Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstFloorplan>(
                new[] { floorplanId }, applicationId);
        }

        public async Task<bool> FloorExistsAsync(Guid floorId)
        {
            return await _context.MstFloors.AnyAsync(f => f.Id == floorId && f.Status != 0);
        }

        public async Task<bool> FloorplanExistsAsync(Guid floorplanId)
        {
            return await _context.MstFloorplans.AnyAsync(f => f.Id == floorplanId && f.Status != 0);
        }
    }
}
