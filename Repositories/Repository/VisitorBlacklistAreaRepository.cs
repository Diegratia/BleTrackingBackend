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
    public class VisitorBlacklistAreaRepository : BaseRepository
    {
        public VisitorBlacklistAreaRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<List<VisitorBlacklistArea>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<VisitorBlacklistArea?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.VisitorBlacklistAreas
                .Include(v => v.FloorplanMaskedArea)
                .Include(v => v.Visitor)
                .Where(v => v.Id == id);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public IQueryable<VisitorBlacklistArea> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.VisitorBlacklistAreas
                .Include(v => v.FloorplanMaskedArea)
                .Include(v => v.Visitor);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task AddAsync(VisitorBlacklistArea entity)
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

            await _context.VisitorBlacklistAreas.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(VisitorBlacklistArea entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(entity, applicationId, isSystemAdmin);

            // _context.VisitorBlacklistAreas.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(VisitorBlacklistArea entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin && entity.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("You don’t have permission to delete this entity.");

            _context.VisitorBlacklistAreas.Remove(entity);
            await _context.SaveChangesAsync();
        }

        private async Task ValidateRelatedEntitiesAsync(VisitorBlacklistArea entity, Guid? applicationId, bool isSystemAdmin)
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
        }
    }
}
