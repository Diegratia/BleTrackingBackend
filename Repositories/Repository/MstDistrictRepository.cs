// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Entities.Models;
// using Microsoft.EntityFrameworkCore;
// using Repositories.DbContexts;
// using Microsoft.AspNetCore.Http;
// using System.Security.Claims;

// namespace Repositories.Repository
// {
//     public class MstDistrictRepository
//     {
//         private readonly BleTrackingDbContext _context;
//         private readonly IHttpContextAccessor _httpContextAccessor;

//         public MstDistrictRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
//         {
//             _context = context;
//             _httpContextAccessor = httpContextAccessor;
//         }

//          private (Guid? ApplicationId, bool IsSystemAdmin) GetApplicationIdAndRole()
//         {
//             var isSystemAdmin = _httpContextAccessor.HttpContext?.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == LevelPriority.System.ToString());
//             if (isSystemAdmin == true)
//             {
//                 return (null, true); // System admin tidak perlu filter ApplicationId
//             }

//             // Prioritas 1: Dari token Bearer (claim di JWT)
//             var applicationIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("ApplicationId")?.Value;
//             if (Guid.TryParse(applicationIdClaim, out var applicationIdFromToken))
//             {
//                 return (applicationIdFromToken, false);
//             }

//             // Prioritas 2: Dari MstIntegration (X-API-KEY-TRACKING-PEOPLE)
//             // var integration = _httpContextAccessor.HttpContext?.Items["MstIntegration"] as MstIntegration;
//             // if (integration?.ApplicationId != null)
//             // {
//             //     return (integration.ApplicationId, false);
//             // }

//             return (null, false);
//         }

//         public async Task<MstDistrict> GetByIdAsync(Guid id)
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
//             // if (!isSystemAdmin && applicationId == null)
//             //     throw new UnauthorizedAccessException("ApplicationId not found in context");
//             var query = _context.MstDistricts
//                 .Where(d => d.Id == id && d.Status != 0);
//             if (!isSystemAdmin)
//             {
//                 query = query.Where(d => d.ApplicationId == applicationId);
//             }
//                 return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("District not found");
//         }

//             public async Task<MstFloorplan> GetFloorplanByIdAsync(Guid floorplanId)
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
//             if (!isSystemAdmin && applicationId == null)
//                 throw new UnauthorizedAccessException("ApplicationId not found in context");

//             var query = _context.MstFloorplans
//                 .Where(f => f.Id == floorplanId && f.Status != 0);
//             if (!isSystemAdmin)
//             {
//                 query = query.Where(f => f.ApplicationId == applicationId);
//             }

//             return await query.FirstOrDefaultAsync();
//         }

//         public async Task<IEnumerable<MstDistrict>> GetAllAsync()
//         {
//             return await _context.MstDistricts
//                 .Where(d => d.Status != 0)
//                 .ToListAsync();
//         }

//         public async Task<MstDistrict> AddAsync(MstDistrict district)
//         {
//             // Validasi ApplicationId
//             var application = await _context.MstApplications
//                 .FirstOrDefaultAsync(a => a.Id == district.ApplicationId && a.ApplicationStatus != 0);
//             if (application == null)
//                 throw new ArgumentException($"Application with ID {district.ApplicationId} not found.");

//             _context.MstDistricts.Add(district);
//             await _context.SaveChangesAsync();
//             return district;
//         }

//         public async Task UpdateAsync(MstDistrict district)
//         {
//             // Validasi ApplicationId
//             var application = await _context.MstApplications
//                 .FirstOrDefaultAsync(a => a.Id == district.ApplicationId && a.ApplicationStatus != 0);
//             if (application == null)
//                 throw new ArgumentException($"Application with ID {district.ApplicationId} not found.");

//             // _context.MstDistricts.Update(district);
//             await _context.SaveChangesAsync();
//         }

//         public async Task DeleteAsync(Guid id)
//         {
//             var district = await _context.MstDistricts
//                 .FirstOrDefaultAsync(d => d.Id == id && d.Status != 0);
//             if (district == null)
//                 throw new KeyNotFoundException("District not found");

//             district.Status = 0;
//             await _context.SaveChangesAsync();
//         }

//         public IQueryable<MstDistrict> GetAllQueryable()
//         {
//             return _context.MstDistricts
//                 .Where(f => f.Status != 0)
//                 .AsQueryable();
//         }

//         public async Task<IEnumerable<MstDistrict>> GetAllExportAsync()
//         {
//             return await _context.MstDistricts
//                 .Where(d => d.Status != 0)
//                 .ToListAsync();
//         }
//     }
// }




using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Repositories.Repository
{
    public class MstDistrictRepository : BaseRepository
    {
        public MstDistrictRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<MstDistrict> GetByIdAsync(Guid id)
        {

            return await GetAllQueryable()
            .Where(d => d.Id == id && d.Status != 0)
            .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("District not found");
        }

        public async Task<MstFloorplan> GetFloorplanByIdAsync(Guid floorplanId)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            if (!isSystemAdmin && applicationId == null)
                throw new UnauthorizedAccessException("ApplicationId not found in context");

            var query = _context.MstFloorplans
                .Where(f => f.Id == floorplanId && f.Status != 0);
            query = query.WithActiveRelations();
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<MstDistrict> AddAsync(MstDistrict district)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

                // non system ambil dari claim
                if (!isSystemAdmin)
                {
                    if (!applicationId.HasValue)
                        throw new UnauthorizedAccessException("ApplicationId not found in context");
                    district.ApplicationId = applicationId.Value;
                }
                // admin set applciation di body
                else if (district.ApplicationId == Guid.Empty)
                {
                    throw new ArgumentException("System admin must provide a valid ApplicationId");
                }
            await ValidateApplicationIdAsync(district.ApplicationId);
            ValidateApplicationIdForEntity(district, applicationId, isSystemAdmin);
            
            _context.MstDistricts.Add(district);
            await _context.SaveChangesAsync();
            return district;
        }

        public async Task UpdateAsync(MstDistrict district)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            await ValidateApplicationIdAsync(district.ApplicationId);
            ValidateApplicationIdForEntity(district, applicationId, isSystemAdmin);

            // _context.MstDistricts.Update(district);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstDistricts
                .Where(d => d.Id == id && d.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var district = await query.FirstOrDefaultAsync();
            if (district == null)
                throw new KeyNotFoundException("District not found");

            district.Status = 0;
            await _context.SaveChangesAsync();
        }

          public async Task<IEnumerable<MstDistrict>> GetAllAsync()
        {
            return await GetAllQueryable()
                .ToListAsync();
        }

        public IQueryable<MstDistrict> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstDistricts
                .Where(d => d.Status != 0);

            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<MstDistrict>> GetAllExportAsync()
        {
            return await GetAllQueryable()
                .ToListAsync();
        }
    }
}











// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Entities.Models;
// using Microsoft.EntityFrameworkCore;
// using Repositories.DbContexts;
// using Microsoft.AspNetCore.Http;
// using System.Security.Claims;

// namespace Repositories.Repository
// {
//     public class FloorplanDeviceRepository
//     {
//         private readonly BleTrackingDbContext _context;
//         private readonly IHttpContextAccessor _httpContextAccessor;

//         public FloorplanDeviceRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
//         {
//             _context = context;
//             _httpContextAccessor = httpContextAccessor;
//         }

//         private (Guid? ApplicationId, bool IsSystemAdmin) GetApplicationIdAndRole()
//         {
//             var isSystemAdmin = _httpContextAccessor.HttpContext?.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == LevelPriority.System.ToString());
//             if (isSystemAdmin == true)
//             {
//                 return (null, true); // System admin tidak perlu filter ApplicationId
//             }

//             // Prioritas 1: Dari token Bearer (claim di JWT)
//             var applicationIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("ApplicationId")?.Value;
//             if (Guid.TryParse(applicationIdClaim, out var applicationIdFromToken))
//             {
//                 return (applicationIdFromToken, false);
//             }

//             // Prioritas 2: Dari MstIntegration (X-API-KEY-TRACKING-PEOPLE)
//             var integration = _httpContextAccessor.HttpContext?.Items["MstIntegration"] as MstIntegration;
//             if (integration?.ApplicationId != null)
//             {
//                 return (integration.ApplicationId, false);
//             }

//             return (null, false);
//         }

//         public async Task<MstFloorplan> GetFloorplanByIdAsync(Guid floorplanId)
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
//             if (!isSystemAdmin && applicationId == null)
//                 throw new UnauthorizedAccessException("ApplicationId not found in context");

//             var query = _context.MstFloorplans
//                 .Where(f => f.Id == floorplanId && f.Status != 0);
//             if (!isSystemAdmin)
//             {
//                 query = query.Where(f => f.ApplicationId == applicationId);
//             }

//             return await query.FirstOrDefaultAsync();
//         }

//         public async Task<MstAccessCctv> GetAccessCctvByIdAsync(Guid accessCctvId)
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
//             if (!isSystemAdmin && applicationId == null)
//                 throw new UnauthorizedAccessException("ApplicationId not found in context");

//             var query = _context.MstAccessCctvs
//                 .Where(c => c.Id == accessCctvId && c.Status != 0);
//             if (!isSystemAdmin)
//             {
//                 query = query.Where(c => c.ApplicationId == applicationId);
//             }

//             return await query.FirstOrDefaultAsync();
//         }

//         public async Task<MstBleReader> GetReaderByIdAsync(Guid readerId)
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
//             if (!isSystemAdmin && applicationId == null)
//                 throw new UnauthorizedAccessException("ApplicationId not found in context");

//             var query = _context.MstBleReaders
//                 .Where(r => r.Id == readerId && r.Status != 0);
//             if (!isSystemAdmin)
//             {
//                 query = query.Where(r => r.ApplicationId == applicationId);
//             }

//             return await query.FirstOrDefaultAsync();
//         }

//         public async Task<MstAccessControl> GetAccessControlByIdAsync(Guid accessControlId)
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
//             if (!isSystemAdmin && applicationId == null)
//                 throw new UnauthorizedAccessException("ApplicationId not found in context");

//             var query = _context.MstAccessControls
//                 .Where(ac => ac.Id == accessControlId && ac.Status != 0);
//             if (!isSystemAdmin)
//             {
//                 query = query.Where(ac => ac.ApplicationId == applicationId);
//             }

//             return await query.FirstOrDefaultAsync();
//         }

//         public async Task<FloorplanMaskedArea> GetFloorplanMaskedAreaByIdAsync(Guid floorplanMaskedAreaId)
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
//             if (!isSystemAdmin && applicationId == null)
//                 throw new UnauthorizedAccessException("ApplicationId not found in context");

//             var query = _context.FloorplanMaskedAreas
//                 .Where(fma => fma.Id == floorplanMaskedAreaId && fma.Status != 0);
//             if (!isSystemAdmin)
//             {
//                 query = query.Where(fma => fma.ApplicationId == applicationId);
//             }

//             return await query.FirstOrDefaultAsync();
//         }

//         public async Task<MstApplication> GetApplicationByIdAsync(Guid applicationId)
//         {
//             return await _context.MstApplications
//                 .Where(a => a.Id == applicationId && a.ApplicationStatus != 0)
//                 .FirstOrDefaultAsync();
//         }

//         public async Task<FloorplanDevice> GetByIdAsync(Guid id)
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
//             if (!isSystemAdmin && applicationId == null)
//                 throw new UnauthorizedAccessException("ApplicationId not found in context");

//             var query = _context.FloorplanDevices
//                 .Include(fd => fd.Floorplan)
//                 .Include(fd => fd.AccessCctv)
//                 .Include(fd => fd.Reader)
//                 .Include(fd => fd.AccessControl)
//                 .Include(fd => fd.FloorplanMaskedArea)
//                 .Where(fd => fd.Id == id && fd.Status != 0);
//             if (!isSystemAdmin)
//             {
//                 query = query.Where(fd => fd.ApplicationId == applicationId);
//             }

//             return await query.FirstOrDefaultAsync();
//         }

//         public async Task<IEnumerable<FloorplanDevice>> GetAllAsync()
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
//             if (!isSystemAdmin && applicationId == null)
//                 throw new UnauthorizedAccessException("ApplicationId not found in context");

//             var query = _context.FloorplanDevices
//                 .Include(fd => fd.Floorplan)
//                 .Include(fd => fd.AccessCctv)
//                 .Include(fd => fd.Reader)
//                 .Include(fd => fd.AccessControl)
//                 .Include(fd => fd.FloorplanMaskedArea)
//                 .Where(fd => fd.Status != 0);
//             if (!isSystemAdmin)
//             {
//                 query = query.Where(fd => fd.ApplicationId == applicationId);
//             }

//             return await query.ToListAsync();
//         }

//         public async Task<FloorplanDevice> AddAsync(FloorplanDevice device)
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
//             if (!isSystemAdmin && applicationId == null)
//                 throw new UnauthorizedAccessException("ApplicationId not found in context");

//             if (!isSystemAdmin && device.ApplicationId != applicationId)
//                 throw new UnauthorizedAccessException("ApplicationId mismatch");

//             _context.FloorplanDevices.Add(device);
//             device.Id = Guid.NewGuid();
//             device.Status = 1;
//             device.CreatedAt = DateTime.UtcNow;
//             device.UpdatedAt = DateTime.UtcNow;
//             await _context.SaveChangesAsync();
//             return device;
//         }

//         public async Task UpdateAsync(FloorplanDevice device)
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
//             if (!isSystemAdmin && applicationId == null)
//                 throw new UnauthorizedAccessException("ApplicationId not found in context");

//             if (!isSystemAdmin && device.ApplicationId != applicationId)
//                 throw new UnauthorizedAccessException("ApplicationId mismatch");

//             device.UpdatedAt = DateTime.UtcNow;
//             await _context.SaveChangesAsync();
//         }

//         public async Task SoftDeleteAsync(Guid id)
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
//             if (!isSystemAdmin && applicationId == null)
//                 throw new UnauthorizedAccessException("ApplicationId not found in context");

//             var query = _context.FloorplanDevices
//                 .Where(fd => fd.Id == id && fd.Status != 0);
//             if (!isSystemAdmin)
//             {
//                 query = query.Where(fd => fd.ApplicationId == applicationId);
//             }

//             var device = await query.FirstOrDefaultAsync();
//             if (device == null)
//                 throw new KeyNotFoundException("FloorplanDevice not found or ApplicationId mismatch");

//             device.UpdatedAt = DateTime.UtcNow;
//             device.Status = 0;
//             await _context.SaveChangesAsync();
//         }

//         public IQueryable<FloorplanDevice> GetAllQueryable()
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
//             if (!isSystemAdmin && applicationId == null)
//                 throw new UnauthorizedAccessException("ApplicationId not found in context");

//             var query = _context.FloorplanDevices
//                 .Include(a => a.FloorplanMaskedArea)
//                 .Include(a => a.Floorplan)
//                 .Include(a => a.AccessControl)
//                 .Include(a => a.AccessCctv)
//                 .Include(a => a.Reader)
//                 .Where(f => f.Status != 0);
//             if (!isSystemAdmin)
//             {
//                 query = query.Where(f => f.ApplicationId == applicationId);
//             }

//             return query.AsQueryable();
//         }

//         public async Task<IEnumerable<FloorplanDevice>> GetAllExportAsync()
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
//             if (!isSystemAdmin && applicationId == null)
//                 throw new UnauthorizedAccessException("ApplicationId not found in context");

//             var query = _context.FloorplanDevices
//                 .Include(fd => fd.Floorplan)
//                 .Include(fd => fd.AccessCctv)
//                 .Include(fd => fd.Reader)
//                 .Include(fd => fd.AccessControl)
//                 .Include(fd => fd.FloorplanMaskedArea)
//                 .Where(fd => fd.Status != 0);
//             if (!isSystemAdmin)
//             {
//                 query = query.Where(fd => fd.ApplicationId == applicationId);
//             }

//             return await query.ToListAsync();
//         }
//     }
// }