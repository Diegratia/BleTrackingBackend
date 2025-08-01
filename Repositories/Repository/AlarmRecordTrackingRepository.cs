using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class AlarmRecordTrackingRepository : BaseRepository
    {
        public AlarmRecordTrackingRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<List<AlarmRecordTracking>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<AlarmRecordTracking?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.AlarmRecordTrackings
                .Include(v => v.FloorplanMaskedArea)
                .Include(v => v.Reader)
                .Include(v => v.Visitor)
                .Include(v => v.ApplicationId)
                .Where(v => v.Id == id);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public IQueryable<AlarmRecordTracking> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.AlarmRecordTrackings
                .Include(v => v.FloorplanMaskedArea)
                .Include(v => v.Reader)
                .Include(v => v.Visitor)
                .Include(v => v.ApplicationId);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task AddAsync(AlarmRecordTracking entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin user.");

                entity.ApplicationId = applicationId.Value;
            }
            else if (entity.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("SystemAdmin Must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(entity, applicationId, isSystemAdmin);

            await _context.AlarmRecordTrackings.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AlarmRecordTracking entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(entity, applicationId, isSystemAdmin);

            _context.AlarmRecordTrackings.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AlarmRecordTracking>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        // public async Task DeleteAsync(AlarmRecordTracking entity)
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

        //     if (!isSystemAdmin && entity.ApplicationId != applicationId)
        //         throw new UnauthorizedAccessException("You don’t have permission to delete this entity.");

        //     // _context.AlarmRecordTrackings.Remove(entity);
        //     await _context.SaveChangesAsync();
        // }

        private async Task ValidateRelatedEntitiesAsync(AlarmRecordTracking entity, Guid? applicationId, bool isSystemAdmin)
        {
            if (isSystemAdmin) return;

            if (!applicationId.HasValue)
                throw new UnauthorizedAccessException("Missing ApplicationId for non-admin.");

            var visitor = await _context.Visitors
                .FirstOrDefaultAsync(v => v.Id == entity.VisitorId && v.ApplicationId == applicationId);

            if (visitor == null)
                throw new UnauthorizedAccessException("Visitor not found or not accessible in your application.");

            var floorplanArea = await _context.FloorplanMaskedAreas
                .FirstOrDefaultAsync(f => f.Id == entity.FloorplanMaskedAreaId && f.ApplicationId == applicationId);

            if (floorplanArea == null)
                throw new UnauthorizedAccessException("FloorplanMaskedArea not found or not accessible in your application.");

            var reader = await _context.MstBleReaders
                .FirstOrDefaultAsync(r => r.Id == entity.ReaderId && r.ApplicationId == applicationId);

            if (reader == null)
                throw new UnauthorizedAccessException("BLE Reader not found or not accessible in your application.");
        }
    }
}
