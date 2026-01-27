using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Shared.Contracts;

namespace Repositories.Repository
{
    public class MstIntegrationRepository : BaseRepository
    {
        public MstIntegrationRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<MstBrand?> GetBrandByIdAsync(Guid brandId)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstBrands
                .Where(b => b.Id == brandId && b.Status != 0);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public async Task<MstApplication?> GetApplicationByIdAsync(Guid applicationId)
        {
            var query = _context.MstApplications
                .Where(a => a.Id == applicationId && a.ApplicationStatus != 0);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<MstIntegration?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstIntegrations
                .Include(i => i.Brand)
                .Where(i => i.Id == id && i.Status != 0);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }
        public async Task<MstIntegration?> GetApiKeyAsync(string apiKeyValue)
        {

            var query = _context.MstIntegrations
                .Include(i => i.Brand)
                .Include(i => i.Application)
                .FirstOrDefaultAsync(i => i.ApiKeyValue == apiKeyValue && i.Status != 0 && i.ApiTypeAuth == ApiTypeAuth.ApiKey);

            return await query;
        }

        public async Task<IEnumerable<MstIntegration>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<MstIntegration> AddAsync(MstIntegration integration)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId is required.");
                integration.ApplicationId = applicationId.Value;
            }
            else if (integration.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("SystemAdmin Must explicitly provide ApplicationId.");
            }

            await ValidateApplicationIdAsync(integration.ApplicationId);
            ValidateApplicationIdForEntity(integration, applicationId, isSystemAdmin);

            _context.MstIntegrations.Add(integration);
            await _context.SaveChangesAsync();

            return integration;
        }

        public async Task UpdateAsync(MstIntegration integration)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(integration.ApplicationId);
            ValidateApplicationIdForEntity(integration, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstIntegrations
                .Where(i => i.Id == id && i.Status != 0);

            var integration = await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
            if (integration == null)
                throw new KeyNotFoundException("Integration not found or unauthorized");

            await _context.SaveChangesAsync();
        }

        public IQueryable<MstIntegration> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstIntegrations
                .Include(f => f.Brand)
                .Where(f => f.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<MstIntegration>> GetAllExportAsync()
         {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstIntegrations
                .Where(d => d.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.ToListAsync();
        }
    }
}
