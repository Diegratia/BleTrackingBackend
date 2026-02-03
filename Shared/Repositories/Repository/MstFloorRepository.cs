using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;
using Repositories.Repository;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class MstFloorRepository : BaseRepository
    {
        public MstFloorRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<MstFloorRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(f => f.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<MstFloor?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .Where(f => f.Id == id && f.Status != 0)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MstFloorRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
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

            // _context.MstFloors.Update(floor);   
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstFloors
                .Include(f => f.Building)
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

        public IQueryable<MstFloor> BaseEntityQuery()
        {
            return GetAllQueryable();
        }

        public async Task<MstBuilding?> GetBuildingByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstBuildings
                .Where(b => b.Id == id && b.Status != 0);
                query = query.WithActiveRelations();
            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public async Task<bool> BuildingExistsAsync(Guid buildingId)
        {
            return await _context.MstBuildings
                .AnyAsync(b => b.Id == buildingId && b.Status != 0);
        }

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidBuildingOwnershipAsync(
            Guid buildingId,
            Guid applicationId
        )
        {
            return await CheckInvalidOwnershipIdsAsync<MstBuilding>(
                new[] { buildingId },
                applicationId
            );
        }

             public async Task<List<MstFloor>> GetByBuildingIdAsync(Guid buildingId)
        {
              return await _context.MstFloors
            .Where(a => a.BuildingId == buildingId && a.Status != 0)
            .ToListAsync();
        }


        public async Task<IEnumerable<MstFloorRead>> GetAllExportAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<(List<MstFloorRead> Data, int Total, int Filtered)> FilterAsync(
            MstFloorFilter filter
        )
        {
            var query = BaseEntityQuery();

            var total = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(search) ||
                    x.Building.Name.ToLower().Contains(search)
                );
            }

            var buildingIds = new List<Guid>();
            if (filter.BuildingId.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var raw = filter.BuildingId.GetString();
                if (!string.IsNullOrWhiteSpace(raw) && Guid.TryParse(raw, out var singleId))
                    buildingIds.Add(singleId);
            }
            else if (filter.BuildingId.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var el in filter.BuildingId.EnumerateArray())
                {
                    if (el.ValueKind != System.Text.Json.JsonValueKind.String)
                        continue;
                    var raw = el.GetString();
                    if (string.IsNullOrWhiteSpace(raw))
                        continue;
                    if (Guid.TryParse(raw, out var parsed))
                        buildingIds.Add(parsed);
                }
            }
            if (buildingIds.Count > 0)
                query = query.Where(x => buildingIds.Contains(x.BuildingId));

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.UpdatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.UpdatedAt <= filter.DateTo.Value);

            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status.Value);

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();
            return (data, total, filtered);
        }

        public IQueryable<MstFloorRead> ProjectToRead(IQueryable<MstFloor> query)
        {
            return query.AsNoTracking().Select(x => new MstFloorRead
            {
                Id = x.Id,
                BuildingId = x.BuildingId,
                Name = x.Name,
                Status = x.Status ?? 0,
                ApplicationId = x.ApplicationId,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedBy = x.UpdatedBy,
                UpdatedAt = x.UpdatedAt,
                Building = x.Building == null ? null : new MstBuildingRead
                {
                    Id = x.Building.Id,
                    Name = x.Building.Name,
                    Image = x.Building.Image,
                    Status = x.Building.Status,
                    ApplicationId = x.Building.ApplicationId
                }
            });
        }
    }
}
