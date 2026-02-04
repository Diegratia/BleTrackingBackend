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
    public class GeofenceRepository : BaseRepository
    {
        public GeofenceRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<GeofenceRead?> GetByIdAsync(Guid id)
        {
            var query = GetAllQueryable().Where(a => a.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<Geofence?> GetByIdEntityAsync(Guid id)
        {
            return await GetAllQueryable().FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<GeofenceRead>> GetAllAsync()
        {
            return await ProjectToRead(GetAllQueryable()).ToListAsync();
        }

        public async Task<Geofence> AddAsync(Geofence geofence)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                geofence.ApplicationId = applicationId.Value;
            }
            else if (geofence.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(geofence.ApplicationId);
            ValidateApplicationIdForEntity(geofence, applicationId, isSystemAdmin);

            _context.Geofences.Add(geofence);
            await _context.SaveChangesAsync();
            return geofence;
        }

        public async Task UpdateAsync(Geofence geofence)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(geofence.ApplicationId);
            ValidateApplicationIdForEntity(geofence, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Geofences
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var geofence = await query.FirstOrDefaultAsync();

            if (geofence == null)
                return;

            await _context.SaveChangesAsync();
        }


        public IQueryable<Geofence> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Geofences
            .Include(d => d.Floor)
            .Include(d => d.Floorplan)
            .Where(d => d.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<(List<GeofenceRead> Data, int Total, int Filtered)> FilterAsync(GeofenceFilter filter)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Geofences.AsQueryable();
            query = query.Where(x => x.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var total = await query.CountAsync();

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

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        private IQueryable<GeofenceRead> ProjectToRead(IQueryable<Geofence> query)
        {
            return query.AsNoTracking().Select(t => new GeofenceRead
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
                ApplicationId = t.ApplicationId,
            });
        }

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidFloorplanOwnershipAsync(Guid floorplanId, Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstFloorplan>(
               new[] { floorplanId },
               applicationId
           );
        }

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidFloorOwnershipAsync(Guid floorId, Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstFloor>(
               new[] { floorId },
               applicationId
           );
        }

        public async Task<bool> FloorExistsAsync(Guid floorId)
        {
            return await _context.MstFloors
                .AnyAsync(b => b.Id == floorId && b.Status != 0);
        }

        public async Task<bool> FloorplanExistsAsync(Guid floorplanId)
        {
            return await _context.MstFloorplans
                .AnyAsync(b => b.Id == floorplanId && b.Status != 0);
        }

        public async Task<List<Geofence>> GetByFloorplanIdAsync(Guid floorplanId)
        {
            return await _context.Geofences
                .Where(ma => ma.FloorplanId == floorplanId && ma.Status != 0)
                .ToListAsync();
        }
    }
}
