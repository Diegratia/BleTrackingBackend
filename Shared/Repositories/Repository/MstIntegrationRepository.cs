using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class MstIntegrationRepository : BaseRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstIntegrationRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
            _context = context;
        }

        private IQueryable<MstIntegration> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstIntegrations
                .Include(i => i.Brand)
                .Where(x => x.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private IQueryable<MstIntegrationRead> ProjectToRead(IQueryable<MstIntegration> query)
        {
            return query.AsNoTracking().Select(x => new MstIntegrationRead
            {
                Id = x.Id,
                BrandId = x.BrandId,
                IntegrationType = x.IntegrationType.ToString(),
                ApiTypeAuth = x.ApiTypeAuth.ToString(),
                ApiUrl = x.ApiUrl,
                ApiAuthUsername = x.ApiAuthUsername,
                ApiAuthPasswd = x.ApiAuthPasswd,
                ApiKeyField = x.ApiKeyField,
                ApiKeyValue = x.ApiKeyValue,
                Status = x.Status ?? 1,
                ApplicationId = x.ApplicationId,
                Brand = x.Brand != null ? new BrandNavigationRead
                {
                    Id = x.Brand.Id,
                    Name = x.Brand.Name
                } : null
            });
        }

        public async Task<MstIntegrationRead?> GetByIdAsync(Guid id)
        {
            return await ProjectToRead(BaseEntityQuery()).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<MstIntegration?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<MstIntegrationRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<(List<MstIntegrationRead> Data, int Total, int Filtered)> FilterAsync(MstIntegrationFilter filter)
        {
            var query = BaseEntityQuery();

            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(x =>
                    (x.ApiUrl != null && x.ApiUrl.ToLower().Contains(filter.Search.ToLower())) ||
                    (x.ApiAuthUsername != null && x.ApiAuthUsername.ToLower().Contains(filter.Search.ToLower())) ||
                    (x.Brand != null && x.Brand.Name != null && x.Brand.Name.ToLower().Contains(filter.Search.ToLower())));

            var brandIds = ExtractIds(filter.BrandId);
            if (brandIds.Count > 0)
                query = query.Where(x => brandIds.Contains(x.BrandId));

            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status.Value);

            var total = await query.CountAsync();
            var filtered = total;

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
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

        public async Task SoftDeleteAsync(MstIntegration integration)
        {
            integration.Status = 0;
            await _context.SaveChangesAsync();
        }

        public async Task<MstIntegration?> GetApiKeyAsync(string apiKeyValue)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstIntegrations
                .Include(i => i.Brand)
                .Include(i => i.Application)
                .Where(x => x.ApiKeyValue == apiKeyValue && x.Status != 0 && x.ApiTypeAuth == ApiTypeAuth.ApiKey);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
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

        public async Task<List<MstIntegration>> GetAllExportAsync()
        {
            return await BaseEntityQuery().Include(x => x.Brand).ToListAsync();
        }
    }
}
