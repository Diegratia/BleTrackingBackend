using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class MstAccessCctvRepository : BaseRepository
    {
        public MstAccessCctvRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        private IQueryable<MstAccessCctv> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstAccessCctvs
                .Where(x => x.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<MstAccessCctvRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(x => x.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<MstAccessCctv?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .Include(x => x.Integration)
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MstAccessCctvRead>> GetAllAsync()
        {
            var query = BaseEntityQuery();
            return await ProjectToRead(query).ToListAsync();
        }

        public async Task<IEnumerable<MstAccessCctvRead>> GetAllUnassignedAsync()
        {
            var query = BaseEntityQuery()
                .Where(x => x.IsAssigned == false);
            return await ProjectToRead(query).ToListAsync();
        }

        private IQueryable<MstAccessCctvRead> ProjectToRead(IQueryable<MstAccessCctv> query)
        {
            return query.AsNoTracking().Select(x => new MstAccessCctvRead
            {
                Id = x.Id,
                Name = x.Name,
                Rtsp = x.Rtsp,
                IsAssigned = x.IsAssigned ?? false,
                IntegrationId = x.IntegrationId,
                IntegrationType = x.Integration != null ? x.Integration.IntegrationType : null,
                ApplicationId = x.ApplicationId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedBy = x.UpdatedBy,
                Status = x.Status ?? 0
            });
        }

        public async Task<(List<MstAccessCctvRead> Data, int Total, int Filtered)> FilterAsync(
            MstAccessCctvFilter filter)
        {
            var query = BaseEntityQuery();

            var total = await query.CountAsync();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(s));
            }

            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status.Value);

            if (filter.IsAssigned.HasValue)
                query = query.Where(x => x.IsAssigned == filter.IsAssigned.Value);

            // Use ExtractIds to support both single Guid and Guid array
            var integrationIds = ExtractIds(filter.IntegrationId);
            if (integrationIds.Count > 0)
                query = query.Where(x => x.IntegrationId.HasValue && integrationIds.Contains(x.IntegrationId.Value));

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.UpdatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.UpdatedAt <= filter.DateTo.Value);

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();
            return (data, total, filtered);
        }

        public async Task<IEnumerable<MstAccessCctvRead>> GetAllExportAsync()
        {
            var query = BaseEntityQuery();
            return await ProjectToRead(query).ToListAsync();
        }

        public async Task<MstAccessCctv> AddAsync(MstAccessCctv accessCctv)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin users.");
                accessCctv.ApplicationId = applicationId.Value;
            }
            else if (accessCctv.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("SystemAdmin Must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(accessCctv.ApplicationId);
            ValidateApplicationIdForEntity(accessCctv, applicationId, isSystemAdmin);

            // Validate Integration ownership if provided
            if (accessCctv.IntegrationId.HasValue)
            {
                var invalidIntegrationIds = await CheckInvalidIntegrationOwnershipAsync(
                    accessCctv.IntegrationId.Value,
                    accessCctv.ApplicationId);
                if (invalidIntegrationIds.Any())
                    throw new UnauthorizedAccessException("Integration does not belong to the same Application.");
            }

            _context.MstAccessCctvs.Add(accessCctv);
            await _context.SaveChangesAsync();

            return accessCctv;
        }

        public async Task UpdateAsync(MstAccessCctv accessCctv)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(accessCctv.ApplicationId);
            ValidateApplicationIdForEntity(accessCctv, applicationId, isSystemAdmin);

            // Validate Integration ownership if provided
            if (accessCctv.IntegrationId.HasValue)
            {
                var invalidIntegrationIds = await CheckInvalidIntegrationOwnershipAsync(
                    accessCctv.IntegrationId.Value,
                    accessCctv.ApplicationId);
                if (invalidIntegrationIds.Any())
                    throw new UnauthorizedAccessException("Integration does not belong to the same Application.");
            }

            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstAccessCctvs
                .Where(a => a.Id == id && a.Status != 0);

            var accessCctv = await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();

            if (accessCctv != null)
            {
                accessCctv.Status = 0;
                accessCctv.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidIntegrationOwnershipAsync(
            Guid integrationId,
            Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstIntegration>(
                new[] { integrationId },
                applicationId
            );
        }
    }
}
