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
using Repositories.Repository.RepoModel;

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

        public async Task<IEnumerable<AlarmTriggersLookUp>> GetAllLookUpAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.AlarmTriggers
                .AsNoTracking()
                .Where(b => b.IsActive == true && 
                        b.Alarm.HasValue && 
                        (b.VisitorId.HasValue || b.MemberId.HasValue));

            if (!isSystemAdmin)
            {
                query = query.Where(b => b.ApplicationId == applicationId);
            }

            var result = await query
                .Select(b => new AlarmTriggersLookUp
                {
                    Id = b.Id,
                    BeaconId = b.BeaconId,
                    VisitorId = b.VisitorId,
                    MemberId = b.MemberId,
                    VisitorName = b.VisitorId.HasValue 
                        ? b.Visitor.Name 
                        : null,
                    MemberName = b.MemberId.HasValue
                        ? b.Member.Name
                        : null,
                    VisitorFaceImage = b.Visitor.FaceImage,
                    MemberFaceImage = b.Member.FaceImage, 
                    PersonImage = b.Visitor.FaceImage ?? b.Member.FaceImage,
                    // FloorplanId = b.FloorplanId,
                    // PosX = b.PosX,
                    // PosY = b.PosY,
                    TriggerTime = b.TriggerTime,
                    // AlarmRecordStatus = b.Alarm.HasValue ? b.Alarm.ToString() : null,
                    // AlarmColor = b.AlarmColor,
                    // ActionStatus = b.Action.HasValue ? b.Action.ToString() : null,
                    // IsActive = b.IsActive,
                    // IdleTimestamp = b.IdleTimestamp,
                    // DoneTimestamp = b.DoneTimestamp,
                    // CancelTimestamp = b.CancelTimestamp,
                    // WaitingTimestamp = b.WaitingTimestamp,
                    // InvestigatedTimestamp = b.InvestigatedTimestamp,
                    // InvestigatedDoneAt = b.InvestigatedDoneAt,
                    // IdleBy = b.IdleBy,
                    // DoneBy = b.DoneBy,
                    // CancelBy = b.CancelBy,
                    // WaitingBy = b.WaitingBy,
                    // InvestigatedBy = b.InvestigatedBy,
                    // InvestigatedResult = b.InvestigatedResult,
                    ApplicationId = b.ApplicationId
                })
                .OrderByDescending(b => b.TriggerTime)
                .ToListAsync();

            return result;
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

         public async Task<List<AlarmTriggersRM>> GetTopTriggersAsync(int topCount = 5)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.AlarmTriggers
                .AsNoTracking()
                .Where(c => c.IsActive == true && c.DoneTimestamp == null && c.Action != ActionStatus.Done && c.Action != ActionStatus.NoAction);

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            return await q
                .OrderByDescending(x => x.TriggerTime) 
                .Take(topCount)
                .Select(x => new AlarmTriggersRM
                {
                    Id = x.Id,
                    BeaconId = x.BeaconId ?? "Unknown Card",
                })
                .ToListAsync();
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
            .Include(b => b.Visitor)
            .Include(b => b.Member)
            .Include(b => b.Floorplan);

            // query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }
    }
}