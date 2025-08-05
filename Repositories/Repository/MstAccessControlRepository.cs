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
    public class MstAccessControlRepository : BaseRepository
    {
        public MstAccessControlRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<MstAccessControl?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstAccessControls
                .Include(a => a.Brand)
                .Include(a => a.Integration)
                .Include(a => a.Application)
                .Where(a => a.Id == id && a.Status != 0);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MstAccessControl>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<IEnumerable<MstAccessControl>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<MstAccessControl> AddAsync(MstAccessControl accessControl)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin users.");

                accessControl.ApplicationId = applicationId.Value;
            }
            else if (accessControl.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("SystemAdmin Must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(accessControl.ApplicationId);
            ValidateApplicationIdForEntity(accessControl, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(accessControl, applicationId, isSystemAdmin);

            _context.MstAccessControls.Add(accessControl);
            await _context.SaveChangesAsync();
            return accessControl;
        }

        public async Task UpdateAsync(MstAccessControl accessControl)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(accessControl.ApplicationId);
            ValidateApplicationIdForEntity(accessControl, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(accessControl, applicationId, isSystemAdmin);

            // _context.MstAccessControls.Update(accessControl);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var accessControl = await GetByIdAsync(id);
            if (accessControl == null)
                throw new KeyNotFoundException("Access Control not found");

            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin && accessControl.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("You don't have permission to delete this item.");

            // _context.MstAccessControls.Update(accessControl);
            await _context.SaveChangesAsync();
        }

        public IQueryable<MstAccessControl> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstAccessControls
                // .Include(a => a.Brand)
                .Include(a => a.Integration)
                .Include(a => a.Application)
                .Where(a => a.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private async Task ValidateRelatedEntitiesAsync(MstAccessControl accessControl, Guid? applicationId, bool isSystemAdmin)
        {
            if (isSystemAdmin) return;

            if (!applicationId.HasValue)
                throw new UnauthorizedAccessException("Missing ApplicationId for non-admin user.");

            var brand = await _context.MstBrands
                .FirstOrDefaultAsync(b => b.Id == accessControl.BrandId && b.ApplicationId == applicationId);
            if (brand == null)
                throw new UnauthorizedAccessException("Invalid BrandId for this application.");

            var integration = await _context.MstIntegrations
                .FirstOrDefaultAsync(i => i.Id == accessControl.IntegrationId && i.ApplicationId == applicationId);
            if (integration == null)
                throw new UnauthorizedAccessException("Invalid IntegrationId for this application.");
        }
    }
}
