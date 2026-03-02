using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class BoundaryRepository : BaseRepository
    {
        public BoundaryRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        private IQueryable<Boundary> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Boundarys
                .Where(x => x.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<BoundaryRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(x => x.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<Boundary?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .Include(x => x.Floor)
                .Include(x => x.Floorplan)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<BoundaryRead>> GetAllAsync()
        {
            var query = BaseEntityQuery();
            return await ProjectToRead(query).ToListAsync();
        }

        private IQueryable<BoundaryRead> ProjectToRead(IQueryable<Boundary> query)
        {
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                query = query.Where(x => x.Floor != null && accessibleBuildingIds.Contains(x.Floor.BuildingId));
            }
            return query.AsNoTracking().Select(x => new BoundaryRead
            {
                Id = x.Id,
                Name = x.Name,
                AreaShape = x.AreaShape,
                Color = x.Color,
                Remarks = x.Remarks,
                FloorplanId = x.FloorplanId,
                FloorplanName = x.Floorplan != null ? x.Floorplan.Name : null,
                FloorId = x.FloorId,
                FloorName = x.Floor != null ? x.Floor.Name : null,
                BoundaryType = x.BoundaryType,
                IsActive = x.IsActive,
                ApplicationId = x.ApplicationId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedBy = x.UpdatedBy,
                Status = x.Status
            });
        }

        public async Task<(List<BoundaryRead> Data, int Total, int Filtered)> FilterAsync(
            BoundaryFilter filter)
        {
            var query = BaseEntityQuery();

            var total = await query.CountAsync();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(s));
            }

            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(x => x.IsActive == filter.IsActive.Value);

            if (filter.BoundaryType.HasValue)
                query = query.Where(x => x.BoundaryType == filter.BoundaryType.Value);

            // Use ExtractIds to support both single Guid and Guid array
            var floorplanIds = ExtractIds(filter.FloorplanId);
            if (floorplanIds.Count > 0)
                query = query.Where(x => x.FloorplanId.HasValue && floorplanIds.Contains(x.FloorplanId.Value));

            var floorIds = ExtractIds(filter.FloorId);
            if (floorIds.Count > 0)
                query = query.Where(x => x.FloorId.HasValue && floorIds.Contains(x.FloorId.Value));

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.UpdatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.UpdatedAt <= filter.DateTo.Value);

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();
            return (data, total, filtered);
        }

        public async Task<Boundary> AddAsync(Boundary boundary)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                boundary.ApplicationId = applicationId.Value;
            }
            else if (boundary.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(boundary.ApplicationId);
            ValidateApplicationIdForEntity(boundary, applicationId, isSystemAdmin);

            // Validate Floor ownership if provided
            if (boundary.FloorId.HasValue)
            {
                var invalidFloorIds = await CheckInvalidFloorOwnershipAsync(
                    boundary.FloorId.Value,
                    boundary.ApplicationId);
                if (invalidFloorIds.Any())
                    throw new UnauthorizedAccessException("Floor does not belong to the same Application.");
            }

            // Validate Floorplan ownership if provided
            if (boundary.FloorplanId.HasValue)
            {
                var invalidFloorplanIds = await CheckInvalidFloorplanOwnershipAsync(
                    boundary.FloorplanId.Value,
                    boundary.ApplicationId);
                if (invalidFloorplanIds.Any())
                    throw new UnauthorizedAccessException("Floorplan does not belong to the same Application.");
            }

            _context.Boundarys.Add(boundary);
            await _context.SaveChangesAsync();
            return boundary;
        }

        public async Task UpdateAsync(Boundary boundary)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(boundary.ApplicationId);
            ValidateApplicationIdForEntity(boundary, applicationId, isSystemAdmin);

            // Validate Floor ownership if provided
            if (boundary.FloorId.HasValue)
            {
                var invalidFloorIds = await CheckInvalidFloorOwnershipAsync(
                    boundary.FloorId.Value,
                    boundary.ApplicationId);
                if (invalidFloorIds.Any())
                    throw new UnauthorizedAccessException("Floor does not belong to the same Application.");
            }

            // Validate Floorplan ownership if provided
            if (boundary.FloorplanId.HasValue)
            {
                var invalidFloorplanIds = await CheckInvalidFloorplanOwnershipAsync(
                    boundary.FloorplanId.Value,
                    boundary.ApplicationId);
                if (invalidFloorplanIds.Any())
                    throw new UnauthorizedAccessException("Floorplan does not belong to the same Application.");
            }

            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Boundarys
                .Where(a => a.Id == id && a.Status != 0);

            var boundary = await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();

            if (boundary != null)
            {
                boundary.Status = 0;
                boundary.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Boundary>> GetByFloorplanIdAsync(Guid floorplanId)
        {
            return await _context.Boundarys
                .Where(ma => ma.FloorplanId == floorplanId && ma.Status != 0)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidFloorplanOwnershipAsync(
            Guid floorplanId,
            Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstFloorplan>(
                new[] { floorplanId },
                applicationId
            );
        }

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidFloorOwnershipAsync(
            Guid floorId,
            Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstFloor>(
                new[] { floorId },
                applicationId
            );
        }
    }
}
