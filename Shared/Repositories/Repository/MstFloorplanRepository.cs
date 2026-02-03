using System;
using System.Collections.Generic;
using System.Linq;
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
    public class MstFloorplanRepository : BaseRepository
    {
        public MstFloorplanRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<MstFloorplanRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(fp => fp.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<MstFloorplan?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .Where(fp => fp.Id == id && fp.Status != 0)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MstFloorplanRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
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

            var floorplan = await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();

            if (floorplan == null)
                throw new KeyNotFoundException("Floorplan not found or unauthorized");

            await _context.SaveChangesAsync();
        }

        public async Task<MstFloor?> GetFloorByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstFloors
                .Where(f => f.Id == id && f.Status != 0);
            query = query.WithActiveRelations();
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> FloorExistsAsync(Guid floorId)
        {
            return await _context.MstFloors
                .AnyAsync(f => f.Id == floorId && f.Status != 0);
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

        public async Task<IEnumerable<MstFloorplanRead>> GetAllExportAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public IQueryable<MstFloorplan> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstFloorplans
                .Include(f => f.Floor)
                .Include(f => f.Engine)
                .Where(f => f.Status != 0);
            
            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public IQueryable<MstFloorplan> BaseEntityQuery()
        {
            return GetAllQueryable();
        }

            public async Task<List<MstFloorplan>> GetByFloorIdAsync(Guid floorId)
            {
                return await _context.MstFloorplans
                    .Where(ma => ma.FloorId == floorId && ma.Status != 0)
                    .ToListAsync();
            }
        

        private async Task ValidateFloorOwnershipAsync(Guid floorId, Guid appId)
        {
            var floor = await _context.MstFloors
                .FirstOrDefaultAsync(f => f.Id == floorId && f.Status != 0 && f.ApplicationId == appId);

            if (floor == null)
                throw new ArgumentException($"Floor with ID {floorId} not found or not part of the application");
        }

        public async Task<(List<MstFloorplanRead> Data, int Total, int Filtered)> FilterAsync(
            MstFloorplanFilter filter
        )
        {
            var query = BaseEntityQuery();

            var total = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(search) ||
                    x.Floor.Name.ToLower().Contains(search)
                );
            }

            // if (filter.FloorId.HasValue)
            //     query = query.Where(x => x.FloorId == filter.FloorId.Value);
            
            var floorIds = new List<Guid>();
            if (filter.FloorId.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var raw = filter.FloorId.GetString();
                if (!string.IsNullOrWhiteSpace(raw) && Guid.TryParse(raw, out var singleId))
                    floorIds.Add(singleId);
            }
            else if (filter.FloorId.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var el in filter.FloorId.EnumerateArray())
                {
                    if (el.ValueKind != System.Text.Json.JsonValueKind.String)
                        continue;
                    var raw = el.GetString();
                    if (string.IsNullOrWhiteSpace(raw))
                        continue;
                    if (Guid.TryParse(raw, out var parsed))
                        floorIds.Add(parsed);
                }
            }
            if (floorIds.Count > 0)
                query = query.Where(x => floorIds.Contains(x.FloorId));

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

        public IQueryable<MstFloorplanRead> ProjectToRead(IQueryable<MstFloorplan> query)
        {
            return query.AsNoTracking().Select(x => new MstFloorplanRead
            {
                Id = x.Id,
                Name = x.Name,
                FloorId = x.FloorId,
                FloorplanImage = x.FloorplanImage,
                PixelX = x.PixelX,
                PixelY = x.PixelY,
                FloorX = x.FloorX,
                FloorY = x.FloorY,
                MeterPerPx = x.MeterPerPx,
                EngineId = x.EngineId,
                Status = x.Status ?? 0,
                ApplicationId = x.ApplicationId,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedBy = x.UpdatedBy,
                UpdatedAt = x.UpdatedAt,
                MaskedAreaCount = x.FloorplanMaskedAreas.Count(m => m.Status != 0),
                DeviceCount = x.FloorplanDevices.Count(m => m.Status != 0),
                PatrolAreaCount = x.PatrolAreas.Count(m => m.Status != 0),
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
                }
            });
        }
    }
}
