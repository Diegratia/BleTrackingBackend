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
    public class FloorplanDeviceRepository : BaseRepository
    {
        public FloorplanDeviceRepository(BleTrackingDbContext context, IHttpContextAccessor accessor)
            : base(context, accessor) { }

        public async Task<int> GetCountAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.FloorplanDevices
                .AsNoTracking()
                .Where(c => c.Status != 0 && c.DeviceStatus == DeviceStatus.Active && c.Type == DeviceType.BleReader);

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            return await q.CountAsync();
        }

        public async Task<List<FloorplanDeviceRM2>> GetTopReadersAsync(int topCount = 5)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.FloorplanDevices
                .AsNoTracking()
                .Where(c => c.Status != 0 && c.DeviceStatus == DeviceStatus.Active && c.Type == DeviceType.BleReader);

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            return await q
                .OrderByDescending(x => x.UpdatedAt)
                .Take(topCount)
                .Select(x => new FloorplanDeviceRM2
                {
                    Id = x.Id,
                    Name = x.Name ?? "Unknown Reader",
                })
                .ToListAsync();
        }

        public async Task<FloorplanDeviceRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(fd => fd.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<FloorplanDevice?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery().Where(fd => fd.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<FloorplanDeviceRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<FloorplanDevice> AddAsync(FloorplanDevice device)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                device.ApplicationId = applicationId.Value;
            }
            else if (device.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(device.ApplicationId);
            ValidateApplicationIdForEntity(device, applicationId, isSystemAdmin);

            device.Id = Guid.NewGuid();
            device.Status = 1;
            device.CreatedAt = DateTime.UtcNow;
            device.UpdatedAt = DateTime.UtcNow;

            _context.FloorplanDevices.Add(device);
            await _context.SaveChangesAsync();

            return device;
        }

        public async Task UpdateAsync(FloorplanDevice device)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(device.ApplicationId);
            ValidateApplicationIdForEntity(device, applicationId, isSystemAdmin);

            device.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.FloorplanDevices
                .Where(a => a.Id == id && a.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var device = await query.FirstOrDefaultAsync();
            if (device == null)
                throw new KeyNotFoundException("FloorplanDevice not found");

            await _context.SaveChangesAsync();
        }

        private IQueryable<FloorplanDevice> BaseEntityQuery()
        {
            return GetAllQueryable();
        }

        public IQueryable<FloorplanDevice> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.FloorplanDevices
                .Include(fd => fd.FloorplanMaskedArea)
                .Include(fd => fd.Floorplan)
                .Include(fd => fd.AccessControl)
                .Include(fd => fd.AccessCctv)
                .Include(fd => fd.Reader)
                .Where(fd => fd.Status != 0);

            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public IQueryable<FloorplanDeviceRead> ProjectToRead(IQueryable<FloorplanDevice> query)
        {
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                query = query.Where(t => t.Floorplan != null && t.Floorplan.Floor != null && accessibleBuildingIds.Contains(t.Floorplan.Floor.BuildingId));
            }
            return query.AsNoTracking().Select(t => new FloorplanDeviceRead
            {
                Id = t.Id,
                Name = t.Name,
                Type = t.Type.ToString(),
                DeviceStatus = t.DeviceStatus.ToString(),
                PosX = t.PosX,
                PosY = t.PosY,
                PosPxX = t.PosPxX,
                PosPxY = t.PosPxY,
                ReaderId = t.ReaderId,
                FloorplanMaskedAreaId = t.FloorplanMaskedAreaId,
                AccessCctvId = t.AccessCctvId,
                AccessControlId = t.AccessControlId,
                Path = t.Path,
                FloorplanId = t.FloorplanId,
                Status = t.Status,
                ApplicationId = t.ApplicationId,
                Floorplan = t.Floorplan == null ? null : new MinimalFloorplanRead
                {
                    Id = t.Floorplan.Id,
                    Name = t.Floorplan.Name,
                    FloorplanImage = t.Floorplan.FloorplanImage
                },
                Reader = t.Reader == null ? null : new MinimalBleReaderRead
                {
                    Id = t.Reader.Id,
                    Name = t.Reader.Name,
                    Ip = t.Reader.Ip,
                    Gmac = t.Reader.Gmac,
                    BrandId = t.Reader.BrandId,
                },
                AccessCctv = t.AccessCctv == null ? null : new MinimalAccessCctvRead
                {
                    Id = t.AccessCctv.Id,
                    Name = t.AccessCctv.Name,
                    Rtsp = t.AccessCctv.Rtsp,
                    IntegrationId = t.AccessCctv.IntegrationId
                },
                AccessControl = t.AccessControl == null ? null : new MinimalAccessControlRead
                {
                    Id = t.AccessControl.Id,
                    Name = t.AccessControl.Name,
                    BrandId = t.AccessControl.BrandId,
                    Channel = t.AccessControl.Channel,
                },
                FloorplanMaskedArea = t.FloorplanMaskedArea == null ? null : new MinimalMaskedAreaRead
                {
                    Id = t.FloorplanMaskedArea.Id,
                    Name = t.FloorplanMaskedArea.Name,
                    ColorArea = t.FloorplanMaskedArea.ColorArea,
                    AreaShape = t.FloorplanMaskedArea.AreaShape,
                    FloorplanId = t.FloorplanMaskedArea.FloorplanId,
                    FloorId = t.FloorplanMaskedArea.FloorId,
                    RestrictedStatus = t.FloorplanMaskedArea.RestrictedStatus.ToString(),
                },
            });
        }

        public async Task<(List<FloorplanDeviceRead> Data, int Total, int Filtered)> FilterAsync(
            FloorplanDeviceFilter filter
        )
        {
            var query = BaseEntityQuery();

            var total = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(search)) ||
                    (x.Floorplan != null && x.Floorplan.Name.ToLower().Contains(search)) ||
                    (x.AccessCctv != null && x.AccessCctv.Name.ToLower().Contains(search)) ||
                    (x.Reader != null && x.Reader.Name.ToLower().Contains(search)) ||
                    (x.AccessControl != null && x.AccessControl.Name.ToLower().Contains(search)) ||
                    (x.FloorplanMaskedArea != null && x.FloorplanMaskedArea.Name.ToLower().Contains(search))
                );
            }

            var floorplanIds = ExtractIds(filter.FloorplanId);
            if (floorplanIds.Count > 0)
                query = query.Where(x => floorplanIds.Contains(x.FloorplanId));

            var maskedAreaIds = ExtractIds(filter.FloorplanMaskedAreaId);
            if (maskedAreaIds.Count > 0)
                query = query.Where(x => maskedAreaIds.Contains(x.FloorplanMaskedAreaId));

            var readerIds = ExtractIds(filter.ReaderId);
            if (readerIds.Count > 0)
                query = query.Where(x => x.ReaderId.HasValue && readerIds.Contains(x.ReaderId.Value));

            var accessCctvIds = ExtractIds(filter.AccessCctvId);
            if (accessCctvIds.Count > 0)
                query = query.Where(x => x.AccessCctvId.HasValue && accessCctvIds.Contains(x.AccessCctvId.Value));

            var accessControlIds = ExtractIds(filter.AccessControlId);
            if (accessControlIds.Count > 0)
                query = query.Where(x => x.AccessControlId.HasValue && accessControlIds.Contains(x.AccessControlId.Value));

            if (filter.DeviceStatus.HasValue)
                query = query.Where(x => x.DeviceStatus == filter.DeviceStatus.Value);

            if (filter.Type.HasValue)
                query = query.Where(x => x.Type == filter.Type.Value);

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

        public async Task<IEnumerable<FloorplanDevice>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<List<FloorplanDevice>> GetByFloorplanIdAsync(Guid floorplanId)
        {
            return await _context.FloorplanDevices
                .Where(ma => ma.FloorplanId == floorplanId && ma.Status != 0)
                .ToListAsync();
        }

        public async Task<List<FloorplanDevice>> GetByAreaIdAsync(Guid areaId)
        {
            return await _context.FloorplanDevices
                .Where(ma => ma.FloorplanMaskedAreaId == areaId && ma.Status != 0)
                .ToListAsync();
        }

        public Task<MstFloorplan?> GetFloorplanByIdAsync(Guid id) =>
            _context.MstFloorplans.WithActiveRelations().FirstOrDefaultAsync(f => f.Id == id && f.Status != 0);

        public Task<MstAccessCctv?> GetAccessCctvByIdAsync(Guid id) =>
            _context.MstAccessCctvs.WithActiveRelations().FirstOrDefaultAsync(c => c.Id == id && c.Status != 0);

        public Task<MstBleReader?> GetReaderByIdAsync(Guid id) =>
            _context.MstBleReaders.WithActiveRelations().FirstOrDefaultAsync(r => r.Id == id && r.Status != 0);

        public Task<MstAccessControl?> GetAccessControlByIdAsync(Guid id) =>
            _context.MstAccessControls.WithActiveRelations().FirstOrDefaultAsync(ac => ac.Id == id && ac.Status != 0);

        public Task<FloorplanMaskedArea?> GetFloorplanMaskedAreaByIdAsync(Guid id) =>
            _context.FloorplanMaskedAreas.WithActiveRelations().FirstOrDefaultAsync(fma => fma.Id == id && fma.Status != 0);

        public Task<MstApplication?> GetApplicationByIdAsync(Guid id) =>
            _context.MstApplications.FirstOrDefaultAsync(a => a.Id == id && a.ApplicationStatus != 0);
    }
}
