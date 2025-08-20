using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class FloorplanDeviceRepository : BaseRepository
    {
        public FloorplanDeviceRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<FloorplanDevice> GetByIdAsync(Guid id)
        {

            return await GetAllQueryable()
            .Where(fd => fd.Id == id && fd.Status != 0)
            .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("FloorplanDevice not found");
        }

        public async Task<IEnumerable<FloorplanDevice>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
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
            var device = await GetByIdAsync(id);
            if (device == null)
                throw new KeyNotFoundException("FloorplanDevice not found");

            device.Status = 0;
            device.UpdatedAt = DateTime.UtcNow;
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

        public async Task<IEnumerable<FloorplanDevice>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
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
