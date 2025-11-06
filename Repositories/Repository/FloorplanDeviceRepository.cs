using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;
using Helpers.Consumer;
using System.Linq;
using Repositories.Repository.RepoModel;
// using Microsoft.Extensions.Caching.Memory;

namespace Repositories.Repository
{
     public class FloorplanDeviceRepository : BaseProjectionRepository<FloorplanDevice, FloorplanDeviceRM>
    {
        // private readonly IMemoryCache _cache;
        // private const string CacheKey = "FloorplanDevice:All";
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

        public async Task<FloorplanDevice> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
            .Where(fd => fd.Id == id && fd.Status != 0)
            .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("FloorplanDevice not found");
        }
        public async Task<FloorplanDeviceRM> GetByIdProjectedAsync(Guid id)
        {
            return await GetAllProjectedQueryable()
            .Where(fd => fd.Id == id)
            .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("FloorplanDevice not found");
        }

        public async Task<IEnumerable<FloorplanDeviceRM>> GetAllAsync()
        {
            return await GetAllProjectedQueryable().ToListAsync();
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
            // _context.FloorplanDevices.Update(device); // Optional
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
        protected override IQueryable<FloorplanDeviceRM> Project(IQueryable<FloorplanDevice> query)
        {
            return query
                .AsNoTracking()
                .Select(t => new FloorplanDeviceRM
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
                    FloorplanId = t.FloorplanId,
                    UpdatedAt = t.UpdatedAt,
                    CreatedAt = t.CreatedAt,
                    ApplicationId = t.ApplicationId,
                    Floorplan = t.Floorplan == null ? null : new MinimalFloorplanRM
                    {
                        Id = t.Floorplan.Id,
                        Name = t.Floorplan.Name,
                    },
                    Reader = t.Reader == null ? null : new MinimalBleReaderRM
                    {
                        Id = t.Reader.Id,
                        Name = t.Reader.Name,
                        Ip = t.Reader.Ip,
                        Gmac = t.Reader.Gmac,
                        BrandId = t.Reader.BrandId,
                    },
                    FloorplanMaskedArea = t.FloorplanMaskedArea == null ? null : new MinimalMaskedAreaRM
                    {
                        Id = t.FloorplanMaskedArea.Id,
                        Name = t.FloorplanMaskedArea.Name,
                        AreaShape = t.FloorplanMaskedArea.AreaShape,
                        FloorplanId = t.FloorplanMaskedArea.FloorplanId,
                        ColorArea = t.FloorplanMaskedArea.ColorArea,
                        RestrictedStatus = t.FloorplanMaskedArea.RestrictedStatus.ToString(),
                    },
                });
        }

        // public async Task<List<FloorplanDeviceRM>> GetAllProjectedAsync()
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

        //     // 🔹 Query dasar FloorplanDevice
        //     var query = _context.FloorplanDevices
        //         .AsNoTracking()
        //         // .Include(fd => fd.FloorplanMaskedArea)
        //         // .Include(fd => fd.Floorplan)
        //         // .Include(fd => fd.AccessControl)
        //         // .Include(fd => fd.AccessCctv)
        //         // .Include(fd => fd.Reader)
        //         .Where(fd => fd.Status != 0);

        //     // 🔹 Terapkan filter ApplicationId kalau ada multi-tenant
        //     query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

        //     // 🔹 Projection efisien ke RM
        //     var projected = await query.Select(t => new FloorplanDeviceRM
        //     {
        //         Id = t.Id,
        //         Name = t.Name,
        //         Type = t.Type.ToString(),
        //         DeviceStatus = t.DeviceStatus.ToString(),
        //         PosX = t.PosX,
        //         PosY = t.PosY,
        //         PosPxX = t.PosPxX,
        //         PosPxY = t.PosPxY,
        //         ReaderId = t.ReaderId,
        //         AccessCctvId = t.AccessCctvId,
        //         AccessControlId = t.AccessControlId,
        //         FloorplanId = t.FloorplanId,
        //         FloorplanMaskedAreaId = t.FloorplanMaskedAreaId,
        //         UpdatedAt = t.UpdatedAt,
        //         CreatedAt = t.CreatedAt,
        //         ApplicationId = t.ApplicationId,

        //         // 🔸 Floorplan
        //         Floorplan = t.Floorplan == null ? null : new MinimalFloorplanRM
        //         {
        //             Id = t.Floorplan.Id,
        //             Name = t.Floorplan.Name,
        //         },

        //         // 🔸 Reader
        //         Reader = t.Reader == null ? null : new MinimalBleReaderRM
        //         {
        //             Id = t.Reader.Id,
        //             Name = t.Reader.Name,
        //             Ip = t.Reader.Ip,
        //             Gmac = t.Reader.Gmac,
        //             BrandId = t.Reader.BrandId,
        //         },

        //         // 🔸 Access CCTV
        //         AccessCctv = t.AccessCctv == null ? null : new MinimalAccessCctvRM
        //         {
        //             Id = t.AccessCctv.Id,
        //             Name = t.AccessCctv.Name,
        //             Rtsp = t.AccessCctv.Rtsp,
        //             IntegrationId = t.AccessCctv.IntegrationId
        //         },

        //         // 🔸 Access Control
        //         AccessControl = t.AccessControl == null ? null : new MinimalAccessControlRM
        //         {
        //             Id = t.AccessControl.Id,
        //             Name = t.AccessControl.Name,
        //         },

        //         // 🔸 Masked Area
        //         FloorplanMaskedArea = t.FloorplanMaskedArea == null ? null : new MinimalMaskedAreaRM
        //         {
        //             Id = t.FloorplanMaskedArea.Id,
        //             Name = t.FloorplanMaskedArea.Name,
        //             ColorArea = t.FloorplanMaskedArea.ColorArea,
        //             AreaShape = t.FloorplanMaskedArea.AreaShape,
        //             FloorplanId = t.FloorplanMaskedArea.FloorplanId,
        //             RestrictedStatus = t.FloorplanMaskedArea.RestrictedStatus.ToString(),
        //         },
        //     }).ToListAsync();

        //     return projected;
        // }


public IQueryable<FloorplanDeviceRM> GetAllProjectedQueryable()
{
    var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

    // 🔹 Query dasar FloorplanDevice
    var query = _context.FloorplanDevices
        .AsNoTracking()
        .Where(fd => fd.Status != 0);

    // 🔹 Terapkan filter ApplicationId kalau ada multi-tenant
    query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

    // 🔹 Projection efisien ke RM (tanpa eksekusi dulu)
    var projected = query.Select(t => new FloorplanDeviceRM
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
        AccessCctvId = t.AccessCctvId,
        AccessControlId = t.AccessControlId,
        FloorplanId = t.FloorplanId,
        FloorplanMaskedAreaId = t.FloorplanMaskedAreaId,
        UpdatedAt = t.UpdatedAt,
        CreatedAt = t.CreatedAt,
        ApplicationId = t.ApplicationId,

        Floorplan = t.Floorplan == null ? null : new MinimalFloorplanRM
        {
            Id = t.Floorplan.Id,
            Name = t.Floorplan.Name,
        },

        Reader = t.Reader == null ? null : new MinimalBleReaderRM
        {
            Id = t.Reader.Id,
            Name = t.Reader.Name,
            Ip = t.Reader.Ip,
            Gmac = t.Reader.Gmac,
            BrandId = t.Reader.BrandId,
        },

        AccessCctv = t.AccessCctv == null ? null : new MinimalAccessCctvRM
        {
            Id = t.AccessCctv.Id,
            Name = t.AccessCctv.Name,
            Rtsp = t.AccessCctv.Rtsp,
            IntegrationId = t.AccessCctv.IntegrationId
        },

        AccessControl = t.AccessControl == null ? null : new MinimalAccessControlRM
        {
            Id = t.AccessControl.Id,
            Name = t.AccessControl.Name,
            BrandId = t.AccessControl.BrandId.Value,
            Channel = t.AccessControl.Channel,

        },

        FloorplanMaskedArea = t.FloorplanMaskedArea == null ? null : new MinimalMaskedAreaRM
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

    return projected;
}





        // Custom filtering (enum-safe, guid-safe)
        // protected override IQueryable<FloorplanDevice> ApplyEntityFilters(
        //     IQueryable<FloorplanDevice> query,
        //     Dictionary<string, object> filters)
        // {
        //     query = base.ApplyEntityFilters(query, filters); // default filter (Status, AppId, dll.)

        //     foreach (var f in filters)
        //     {
        //         var key = f.Key.ToLower();
        //         var val = f.Value?.ToString()?.Trim();

        //         if (string.IsNullOrEmpty(val)) continue;

        //         switch (key.ToLowerInvariant())
        //         {
        //             case "devicestatus":
        //                 if (Enum.TryParse<DeviceStatus>(val, true, out var status))
        //                     query = query.Where(x => x.DeviceStatus == status);
        //                 break;
        //             case "floorplanid":
        //                 if (Guid.TryParse(val, out var fid))
        //                     query = query.Where(x => x.FloorplanId == fid);
        //                 break;
        //             case "readerid":
        //                 if (Guid.TryParse(val, out var rid))
        //                     query = query.Where(x => x.ReaderId == rid);
        //                 break;
        //             case "type":
        //                 if (Enum.TryParse<DeviceType>(val, true, out var type))
        //                     query = query.Where(x => x.Type == type);
        //                 break;
        //             case "accesscctvid":
        //                 if (Guid.TryParse(val, out var aid))
        //                     query = query.Where(x => x.ReaderId == aid);
        //                 break;
        //             case "accesscontrolid":
        //                 if (Guid.TryParse(val, out var cid))
        //                     query = query.Where(x => x.ReaderId == cid);
        //                 break;
        //             case "floorplanmaskedarea":
        //                 if (Guid.TryParse(val, out var areaId))
        //                     query = query.Where(x => x.FloorplanMaskedAreaId == areaId);
        //                 break;
        //         }
        //     }
        //     return query;
        // }

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


        
        // Optional: helper fetchers if needed for validation or other purposes
        public Task<MstFloorplan> GetFloorplanByIdAsync(Guid id) =>
            _context.MstFloorplans.WithActiveRelations().FirstOrDefaultAsync(f => f.Id == id && f.Status != 0);

        public Task<MstAccessCctv> GetAccessCctvByIdAsync(Guid id) =>
            _context.MstAccessCctvs.WithActiveRelations().FirstOrDefaultAsync(c => c.Id == id && c.Status != 0);


        public Task<MstBleReader> GetReaderByIdAsync(Guid id) =>
            _context.MstBleReaders.WithActiveRelations().FirstOrDefaultAsync(r => r.Id == id && r.Status != 0);

        public Task<MstAccessControl> GetAccessControlByIdAsync(Guid id) =>
            _context.MstAccessControls.WithActiveRelations().FirstOrDefaultAsync(ac => ac.Id == id && ac.Status != 0);

        public Task<FloorplanMaskedArea> GetFloorplanMaskedAreaByIdAsync(Guid id) =>
            _context.FloorplanMaskedAreas.WithActiveRelations().FirstOrDefaultAsync(fma => fma.Id == id && fma.Status != 0);

        public Task<MstApplication> GetApplicationByIdAsync(Guid id) =>
            _context.MstApplications.FirstOrDefaultAsync(a => a.Id == id && a.ApplicationStatus != 0);
    }
}
