using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class RecordTrackingLogRepository
    {
        private readonly BleTrackingDbContext _context;

        public RecordTrackingLogRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RecordTrackingLog>> GetRecordTrackingLogsAsync()
        {
            return await _context.RecordTrackingLogs
                .Include(r => r.Floorplan)
                .ToListAsync();
        }

        public async Task<RecordTrackingLog> GetRecordTrackingLogByIdAsync(Guid id)
        {
            return await _context.RecordTrackingLogs
                .Include(r => r.Floorplan)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task DeleteRecordTrackingLogAsync(Guid id)
        {
            var log = await _context.RecordTrackingLogs.FindAsync(id);
            if (log != null)
            {
                _context.RecordTrackingLogs.Remove(log);
                await _context.SaveChangesAsync();
            }
        }
    }
}