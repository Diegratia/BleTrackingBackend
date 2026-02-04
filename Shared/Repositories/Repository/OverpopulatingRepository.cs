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
    public class OverpopulatingRepository : BaseRepository
    {
        public OverpopulatingRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        private IQueryable<Overpopulating> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Overpopulatings
                .Where(x => x.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<OverpopulatingRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(x => x.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<Overpopulating?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .Include(x => x.Floor)
                .Include(x => x.Floorplan)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<OverpopulatingRead>> GetAllAsync()
        {
            var query = BaseEntityQuery();
            return await ProjectToRead(query).ToListAsync();
        }

        private IQueryable<OverpopulatingRead> ProjectToRead(IQueryable<Overpopulating> query)
        {
            return query.AsNoTracking().Select(x => new OverpopulatingRead
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
                MaxCapacity = x.MaxCapacity,
                IsActive = x.IsActive,
                ApplicationId = x.ApplicationId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedBy = x.UpdatedBy,
                Status = x.Status
            });
        }

        public async Task<(List<OverpopulatingRead> Data, int Total, int Filtered)> FilterAsync(
            OverpopulatingFilter filter)
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

            if (filter.MaxCapacityFrom.HasValue)
                query = query.Where(x => x.MaxCapacity >= filter.MaxCapacityFrom.Value);

            if (filter.MaxCapacityTo.HasValue)
                query = query.Where(x => x.MaxCapacity <= filter.MaxCapacityTo.Value);

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

            // Validate Floor ownership if provided
            if (overpopulating.FloorId.HasValue)
            {
                var invalidFloorIds = await CheckInvalidFloorOwnershipAsync(
                    overpopulating.FloorId.Value,
                    overpopulating.ApplicationId);
                if (invalidFloorIds.Any())
                    throw new UnauthorizedAccessException("Floor does not belong to the same Application.");
            }

            // Validate Floorplan ownership if provided
            if (overpopulating.FloorplanId.HasValue)
            {
                var invalidFloorplanIds = await CheckInvalidFloorplanOwnershipAsync(
                    overpopulating.FloorplanId.Value,
                    overpopulating.ApplicationId);
                if (invalidFloorplanIds.Any())
                    throw new UnauthorizedAccessException("Floorplan does not belong to the same Application.");
            }

            _context.Overpopulatings.Add(overpopulating);
            await _context.SaveChangesAsync();
            return overpopulating;
        }

        public async Task UpdateAsync(Overpopulating overpopulating)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(overpopulating.ApplicationId);
            ValidateApplicationIdForEntity(overpopulating, applicationId, isSystemAdmin);

            // Validate Floor ownership if provided
            if (overpopulating.FloorId.HasValue)
            {
                var invalidFloorIds = await CheckInvalidFloorOwnershipAsync(
                    overpopulating.FloorId.Value,
                    overpopulating.ApplicationId);
                if (invalidFloorIds.Any())
                    throw new UnauthorizedAccessException("Floor does not belong to the same Application.");
            }

            // Validate Floorplan ownership if provided
            if (overpopulating.FloorplanId.HasValue)
            {
                var invalidFloorplanIds = await CheckInvalidFloorplanOwnershipAsync(
                    overpopulating.FloorplanId.Value,
                    overpopulating.ApplicationId);
                if (invalidFloorplanIds.Any())
                    throw new UnauthorizedAccessException("Floorplan does not belong to the same Application.");
            }

            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Overpopulatings
                .Where(a => a.Id == id && a.Status != 0);

            var overpopulating = await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();

            if (overpopulating != null)
            {
                overpopulating.Status = 0;
                overpopulating.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Overpopulating>> GetByFloorplanIdAsync(Guid floorplanId)
        {
            return await _context.Overpopulatings
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
