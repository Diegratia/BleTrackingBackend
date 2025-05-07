using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class FloorplanDeviceRepository
    {
        private readonly BleTrackingDbContext _context;

        public FloorplanDeviceRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstFloorplan> GetFloorplanByIdAsync(Guid floorplanId)
        {
            return await _context.MstFloorplans
                .FirstOrDefaultAsync(f => f.Id == floorplanId && f.Status != 0);
        }

        public async Task<MstAccessCctv> GetAccessCctvByIdAsync(Guid accessCctvId)
        {
            return await _context.MstAccessCctvs
                .FirstOrDefaultAsync(c => c.Id == accessCctvId && c.Status != 0);
        }

        public async Task<MstBleReader> GetReaderByIdAsync(Guid readerId)
        {
            return await _context.MstBleReaders
                .FirstOrDefaultAsync(r => r.Id == readerId && r.Status != 0);
        }

        public async Task<MstAccessControl> GetAccessControlByIdAsync(Guid accessControlId)
        {
            return await _context.MstAccessControls
                .FirstOrDefaultAsync(ac => ac.Id == accessControlId && ac.Status != 0);
        }

        public async Task<FloorplanMaskedArea> GetFloorplanMaskedAreaByIdAsync(Guid floorplanMaskedAreaId)
        {
            return await _context.FloorplanMaskedAreas
                .FirstOrDefaultAsync(fma => fma.Id == floorplanMaskedAreaId && fma.Status != 0);
        }

        public async Task<MstApplication> GetApplicationByIdAsync(Guid applicationId)
        {
            return await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.ApplicationStatus != 0);
        }

        public async Task<FloorplanDevice> GetByIdAsync(Guid id)
        {
            return await _context.FloorplanDevices
                .Include(fd => fd.Floorplan)
                .Include(fd => fd.AccessCctv)
                .Include(fd => fd.Reader)
                .Include(fd => fd.AccessControl)
                .Include(fd => fd.FloorplanMaskedArea)
                .Include(fd => fd.Application)
                .FirstOrDefaultAsync(fd => fd.Id == id && fd.Status != 0);
        }

        public async Task<IEnumerable<FloorplanDevice>> GetAllAsync()
        {
            return await _context.FloorplanDevices
                .Include(fd => fd.Floorplan)
                .Include(fd => fd.AccessCctv)
                .Include(fd => fd.Reader)
                .Include(fd => fd.AccessControl)
                .Include(fd => fd.FloorplanMaskedArea)
                .Include(fd => fd.Application)
                .Where(fd => fd.Status != 0)
                .ToListAsync();
        }

        public async Task<FloorplanDevice> AddAsync(FloorplanDevice device)
        {
            _context.FloorplanDevices.Add(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task UpdateAsync(FloorplanDevice device)
        {
            _context.FloorplanDevices.Update(device);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var device = await GetByIdAsync(id);
            if (device == null)
                throw new KeyNotFoundException("FloorplanDevice not found");

            device.Status = 0;
            await _context.SaveChangesAsync();
        }
    }
}