// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Entities.Models;
// using Microsoft.EntityFrameworkCore;
// using Repositories.DbContexts;

// namespace Repositories.Repository
// {
//     public class MstTrackingLogRepository
//     {
//         private readonly BleTrackingDbDevContext _context;

//         public MstTrackingLogRepository(BleTrackingDbDevContext context)
//         {
//             _context = context;
//         }


//         public async Task<IEnumerable<MstTrackingLog>> GetMstTrackingLogsAsync()
//         {
//             return await _context.MstTrackingLogs
//                 .Include(t => t.Floorplan)
//                 .ToListAsync();
//         }

//         public async Task<MstTrackingLog> GetMstTrackingLogByIdAsync(Guid id)
//         {
//             return await _context.MstTrackingLogs
//                 .Include(t => t.Floorplan)
//                 .FirstOrDefaultAsync(t => t.Id == id);
//         }

//         public async Task DeleteMstTrackingLogAsync(Guid id)
//         {
//             var log = await _context.MstTrackingLogs.FindAsync(id);
//             if (log != null)
//             {
//                 _context.MstTrackingLogs.Remove(log);
//                 await _context.SaveChangesAsync();
//             }
//         }
//     }
// }