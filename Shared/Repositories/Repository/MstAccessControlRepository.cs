using Entities.Models;
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
    public class MstAccessControlRepository : BaseRepository
    {
        public MstAccessControlRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public IQueryable<MstAccessControl> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstAccessControls
                .Include(a => a.Brand)
                .Include(a => a.Integration)
                .Include(a => a.Application)
                .Where(a => a.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private IQueryable<MstAccessControlRead> ProjectToRead(IQueryable<MstAccessControl> query)
        {
            return query.Select(x => new MstAccessControlRead
            {
                Id = x.Id,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedBy = x.UpdatedBy,
                UpdatedAt = x.UpdatedAt,
                Status = x.Status,
                ApplicationId = x.ApplicationId,

                BrandId = x.BrandId,
                Name = x.Name,
                Type = x.Type,
                IsAssigned = x.IsAssigned,
                Description = x.Description,
                Channel = x.Channel,
                DoorId = x.DoorId,
                Raw = x.Raw,
                IntegrationId = x.IntegrationId,

                Brand = x.Brand != null ? new BrandNavigationRead
                {
                    Id = x.Brand.Id,
                    Name = x.Brand.Name
                } : null,
                Integration = x.Integration != null ? new IntegrationNavigationRead
                {
                    Id = x.Integration.Id,
                    ApiUrl = x.Integration.ApiUrl
                } : null
            });
        }

        public async Task<MstAccessControlRead?> GetByIdAsync(Guid id)
        {
            return await ProjectToRead(BaseEntityQuery())
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<MstAccessControl?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<MstAccessControlRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<IEnumerable<MstAccessControlRead>> GetAllUnassignedAsync()
        {
            return await ProjectToRead(GetAllUnassignedQueryable()).ToListAsync();
        }

        public async Task<(List<MstAccessControlRead> Data, int Total, int Filtered)> FilterAsync(
            MstAccessControlFilter filter)
        {
            var query = BaseEntityQuery();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x => x.Name != null && x.Name.ToLower().Contains(search));
            }

            if (filter.BrandId.HasValue)
                query = query.Where(x => x.BrandId == filter.BrandId.Value);

            if (filter.IntegrationId.HasValue)
                query = query.Where(x => x.IntegrationId == filter.IntegrationId.Value);

            if (filter.IsAssigned.HasValue)
                query = query.Where(x => x.IsAssigned == filter.IsAssigned.Value);

            if (!string.IsNullOrWhiteSpace(filter.Type))
                query = query.Where(x => x.Type != null && x.Type.ToLower() == filter.Type.ToLower());

            var total = await query.CountAsync();
            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            if (string.IsNullOrEmpty(filter.SortColumn))
            {
                query = query.OrderByDescending(x => x.UpdatedAt);
            }

            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        public async Task AddAsync(MstAccessControl accessControl)
        {
            await _context.MstAccessControls.AddAsync(accessControl);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MstAccessControl accessControl)
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(MstAccessControl accessControl)
        {
            _context.MstAccessControls.Remove(accessControl);
            await _context.SaveChangesAsync();
        }

        public IQueryable<MstAccessControl> GetAllQueryable()
        {
            return BaseEntityQuery();
        }

        public IQueryable<MstAccessControl> GetAllUnassignedQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstAccessControls
                .Include(r => r.Brand)
                .Where(r => r.IsAssigned == false && r.Status != 0);

            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<MstAccessControl>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidBrandOwnershipAsync(
            Guid brandId,
            Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstBrand>(
                new[] { brandId },
                applicationId
            );
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
