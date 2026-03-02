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
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using Shared.Contracts.Read;

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
                .Include(ma => ma.Floorplan)
                    .ThenInclude(fp => fp.Floor)
                .Where(c => c.Status != 0);

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            // Apply building filter untuk PrimaryAdmin
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                q = q.Where(ma => ma.Floorplan != null
                    && ma.Floorplan.Floor != null
                    && accessibleBuildingIds.Contains(ma.Floorplan.Floor.BuildingId));
            }

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
                .Include(ma => ma.Floorplan)
                    .ThenInclude(fp => fp.Floor)
                .Where(c => c.Status != 0);

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            // Apply building filter untuk PrimaryAdmin
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                q = q.Where(ma => ma.Floorplan != null
                    && ma.Floorplan.Floor != null
                    && accessibleBuildingIds.Contains(ma.Floorplan.Floor.BuildingId));
            }

            return await q.CountAsync();
        }

        public async Task<FloorplanMaskedAreaRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(a => a.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<FloorplanMaskedArea?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery().Where(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<FloorplanMaskedAreaRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
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

        private IQueryable<FloorplanMaskedArea> BaseEntityQuery()
        {
            return GetAllQueryable();
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
    List<Guid>? floorplanIds = null)
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

        public IQueryable<FloorplanMaskedAreaRead> ProjectToRead(IQueryable<FloorplanMaskedArea> query)
        {
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                query = query.Where(x => x.Floorplan != null && x.Floorplan.Floor != null && accessibleBuildingIds.Contains(x.Floorplan.Floor.BuildingId));
            }
            return query.AsNoTracking().Select(x => new FloorplanMaskedAreaRead
            {
                Id = x.Id,
                FloorplanId = x.FloorplanId,
                FloorId = x.FloorId,
                Name = x.Name,
                AreaShape = x.AreaShape,
                ColorArea = x.ColorArea,
                RestrictedStatus = x.RestrictedStatus.ToString(),
                AllowFloorChange = x.AllowFloorChange,
                Status = x.Status,
                ApplicationId = x.ApplicationId,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedBy = x.UpdatedBy,
                UpdatedAt = x.UpdatedAt,
                Floor = x.Floor == null ? null : new MstFloorRead
                {
                    Id = x.Floor.Id,
                    BuildingId = x.Floor.BuildingId,
                    Name = x.Floor.Name,
                    Status = x.Floor.Status ?? 0,
                    ApplicationId = x.Floor.ApplicationId,
                    CreatedBy = x.Floor.CreatedBy,
                    CreatedAt = x.Floor.CreatedAt,
                    UpdatedBy = x.Floor.UpdatedBy,
                    UpdatedAt = x.Floor.UpdatedAt
                },
                Floorplan = x.Floorplan == null ? null : new MstFloorplanRead
                {
                    Id = x.Floorplan.Id,
                    Name = x.Floorplan.Name,
                    FloorId = x.Floorplan.FloorId,
                    FloorplanImage = x.Floorplan.FloorplanImage,
                    PixelX = x.Floorplan.PixelX,
                    PixelY = x.Floorplan.PixelY,
                    FloorX = x.Floorplan.FloorX,
                    FloorY = x.Floorplan.FloorY,
                    MeterPerPx = x.Floorplan.MeterPerPx,
                    EngineId = x.Floorplan.EngineId,
                    Status = x.Floorplan.Status ?? 0,
                    ApplicationId = x.Floorplan.ApplicationId,
                    CreatedBy = x.Floorplan.CreatedBy,
                    CreatedAt = x.Floorplan.CreatedAt,
                    UpdatedBy = x.Floorplan.UpdatedBy,
                    UpdatedAt = x.Floorplan.UpdatedAt
                }
            });
        }

        public async Task<(List<FloorplanMaskedAreaRead> Data, int Total, int Filtered)> FilterAsync(
            FloorplanMaskedAreaFilter filter
        )
        {
            var query = BaseEntityQuery();

            var total = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(search) ||
                    x.Floor.Name.ToLower().Contains(search) ||
                    x.Floorplan.Name.ToLower().Contains(search)
                );
            }

            var floorIds = ExtractIds(filter.FloorId);
            if (floorIds.Count > 0)
                query = query.Where(x => floorIds.Contains(x.FloorId));

            var floorplanIds = ExtractIds(filter.FloorplanId);
            if (floorplanIds.Count > 0)
                query = query.Where(x => floorplanIds.Contains(x.FloorplanId));

            if (!string.IsNullOrWhiteSpace(filter.RestrictedStatus))
            {
                if (Enum.TryParse<RestrictedStatus>(filter.RestrictedStatus, true, out var restricted))
                    query = query.Where(x => x.RestrictedStatus == restricted);
                else if (int.TryParse(filter.RestrictedStatus, out var restrictedInt))
                    query = query.Where(x => (int)x.RestrictedStatus == restrictedInt);
            }

            if (filter.AllowFloorChange.HasValue)
                query = query.Where(x => x.AllowFloorChange == filter.AllowFloorChange.Value);

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

        private static List<Guid> ExtractIds(JsonElement element)
        {
            var ids = new List<Guid>();

            if (element.ValueKind == JsonValueKind.String)
            {
                var raw = element.GetString();
                if (!string.IsNullOrWhiteSpace(raw) && Guid.TryParse(raw, out var singleId))
                    ids.Add(singleId);
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in element.EnumerateArray())
                {
                    if (el.ValueKind != JsonValueKind.String)
                        continue;
                    var raw = el.GetString();
                    if (string.IsNullOrWhiteSpace(raw))
                        continue;
                    if (Guid.TryParse(raw, out var parsed))
                        ids.Add(parsed);
                }
            }

            return ids;
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
