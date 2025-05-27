using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class AlarmRecordTrackingRepository
    {
        private readonly BleTrackingDbContext _context;

        public AlarmRecordTrackingRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstApplication> GetApplicationByIdAsync(Guid applicationId)
        {
            return await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.ApplicationStatus != 0);
        }

        public async Task<Visitor> GetVisitorByIdAsync(Guid visitorId)
        {
            return await _context.Visitors
                .FirstOrDefaultAsync(v => v.Id == visitorId && v.Status != 0);
        }

        public async Task<MstBleReader> GetReaderByIdAsync(Guid readerId)
        {
            return await _context.MstBleReaders
                .FirstOrDefaultAsync(r => r.Id == readerId && r.Status != 0);
        }

        public async Task<FloorplanMaskedArea> GetFloorplanMaskedAreaByIdAsync(Guid floorplanMaskedAreaId)
        {
            return await _context.FloorplanMaskedAreas
                .FirstOrDefaultAsync(fma => fma.Id == floorplanMaskedAreaId && fma.Status != 0);
        }

        public async Task<AlarmRecordTracking> GetByIdAsync(Guid id)
        {
            return await _context.AlarmRecordTrackings
                .Include(a => a.Application)
                .Include(a => a.Visitor)
                .Include(a => a.Reader)
                .Include(a => a.FloorplanMaskedArea)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<AlarmRecordTracking>> GetAllAsync()
        {
            return await _context.AlarmRecordTrackings
                .Include(a => a.Application)
                .Include(a => a.Visitor)
                .Include(a => a.Reader)
                .Include(a => a.FloorplanMaskedArea)
                .ToListAsync();
        }

        public async Task<AlarmRecordTracking> AddAsync(AlarmRecordTracking alarm)
        {
            _context.AlarmRecordTrackings.Add(alarm);
            await _context.SaveChangesAsync();
            return alarm;
        }

        public async Task UpdateAsync(AlarmRecordTracking alarm)
        {
            // _context.AlarmRecordTrackings.Update(alarm);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var alarm = await GetByIdAsync(id);
            if (alarm == null)
                throw new KeyNotFoundException("Alarm record not found");

            await _context.SaveChangesAsync();
        }
    }
}