using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
            return await GetAllQueryable()
            .Where(v => v.Id == id)
            .FirstOrDefaultAsync();
        }

        // public IQueryable<AlarmRecordTracking> GetAllQueryable()
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

        //     var query = _context.AlarmRecordTrackings
        //         .Include(v => v.FloorplanMaskedArea)
        //         .Include(v => v.Reader)
        //         .Include(v => v.Visitor)
        //         .Where(v => v.Id != null);

        //         query = query.WithActiveRelations();

        //     return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        // }

         public IQueryable<AlarmRecordTracking> GetAllQueryable()
            {
                var userEmail = GetUserEmail();
                var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
                var isSuperAdmin = IsSuperAdmin();
                var isPrimaryAdmin = IsPrimaryAdmin();
                var isPrimary = IsPrimary();

                var query = _context.AlarmRecordTrackings
                      .IgnoreQueryFilters()
                    .Include(v => v.Application)
                    .Include(v => v.Visitor)
                    .Include(v => v.Member)
                    .Include(v => v.Reader)
                    .Include(v => v.FloorplanMaskedArea)
                    .Include(v => v.AlarmTriggers)
                    // .Where(v => v.Id != null)
                   
                    .AsQueryable();

                if (!isSystemAdmin && !isSuperAdmin && !isPrimaryAdmin && !isPrimary)
                {
                    var userRole = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
                    if (String.Equals(userRole, LevelPriority.Secondary.ToString(), StringComparison.OrdinalIgnoreCase))
                    // {
                    //     query = query.Where(t =>
                    //         _context.MstMembers.Any(m => m.Email == userEmail &&
                    //             (t.PurposePerson == m.Id || (t.MemberIdentity == m.IdentityId && t.IsMember == 1))));
                    // }
                    {
                        query = query.Where(t => true); // No access for other roles
                    }
                    else if (String.Equals(userRole, LevelPriority.UserCreated.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            query = query.Where(t =>
                                _context.Visitors.Any(v => v.Email == userEmail && t.VisitorId == v.Id));
                        }
                        else
                        {
                            query = query.Where(t => false); // No access for other roles
                        }
                }
                //  query = query.WithActiveRelations();

                return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
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
