using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Helpers.Consumer;
using Data.ViewModels;

namespace Repositories.Repository
{
    public class AlarmTriggersRepository : BaseRepository
    {

        public AlarmTriggersRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
           : base(context, httpContextAccessor)
        {
        }
        public async Task<IEnumerable<AlarmTriggers>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

           public async Task<AlarmTriggers?> GetByIdAsync(Guid id)
        {

            return await GetAllQueryable()
            .Where(b => b.Id == id && b.IsActive != false)
            .FirstOrDefaultAsync();
        }
        
            public async Task<int> GetCountAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.AlarmTriggers
                .AsNoTracking()
                .Where(c => c.IsActive == true && c.DoneTimestamp == null && c.Action != ActionStatus.Done && c.Action != ActionStatus.NoAction);

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            return await q.CountAsync();
        }

        //     public async Task<AlarmTriggers?> GetByDmacAsync(string beaconId)
        // {

        //     return await GetAllQueryable()
        //     .Where(b => b.BeaconId == beaconId && b.IsActive != false)
        //     .FirstOrDefaultAsync();
        // }

        public async Task<IEnumerable<AlarmTriggers>> GetByDmacAsync(string beaconId)
        {

            return await GetAllQueryable()
            .Where(b => b.BeaconId == beaconId && b.IsActive != false)
            .ToListAsync();
        }

            public async Task UpdateAsync(AlarmTriggers alarmTriggers)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(alarmTriggers.ApplicationId);
            ValidateApplicationIdForEntity(alarmTriggers, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }
        
            public async Task UpdateBatchAsync(IEnumerable<AlarmTriggers> alarmTriggers)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            foreach (var alarmTrigger in alarmTriggers)
            {
                await ValidateApplicationIdAsync(alarmTrigger.ApplicationId);
                ValidateApplicationIdForEntity(alarmTrigger, applicationId, isSystemAdmin);
            }

            await _context.SaveChangesAsync();
        }
        
            public IQueryable<AlarmTriggers> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.AlarmTriggers
            .Include(b => b.Floorplan);

            // query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }
    }
}