using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class MonitoringConfigRepository : BaseRepository
    {
        public MonitoringConfigRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        private IQueryable<MonitoringConfig> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MonitoringConfigs.AsQueryable();

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            // Building access filter untuk PrimaryAdmin (bukan System/SuperAdmin)
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                query = query.Where(x => x.BuildingId.HasValue &&
                                        accessibleBuildingIds.Contains(x.BuildingId.Value));
            }

            return query.Include(x => x.Building);
        }

        public async Task<MonitoringConfigRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(x => x.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<MonitoringConfig?> GetByIdEntityAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MonitoringConfigs.AsQueryable();
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<MonitoringConfigRead>> GetAllAsync()
        {
            var query = BaseEntityQuery();
            return await ProjectToRead(query).ToListAsync();
        }

        public IQueryable<MonitoringConfig> GetAllQueryable()
        {
            return BaseEntityQuery();
        }

        private IQueryable<MonitoringConfigRead> ProjectToRead(IQueryable<MonitoringConfig> query)
        {
            return query.AsNoTracking().Select(x => new MonitoringConfigRead
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Config = x.Config,
                BuildingId = x.BuildingId,
                BuildingName = x.Building != null ? x.Building.Name : null,
                ApplicationId = x.ApplicationId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedBy = x.UpdatedBy
            });
        }

        public async Task<(List<MonitoringConfigRead> Data, int Total, int Filtered)> FilterAsync(
            MonitoringConfigFilter filter)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            // Base query with tenant filter
            var query = _context.MonitoringConfigs.AsQueryable();
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            // Building access filter untuk PrimaryAdmin
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                query = query.Include(x => x.Building)
                            .Where(x => x.BuildingId.HasValue &&
                                       accessibleBuildingIds.Contains(x.BuildingId.Value));
            }

            var total = await query.CountAsync();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(x => x.Name.ToLower().Contains(filter.Name.ToLower()));
            }

            var applicationIds = ExtractIds(filter.ApplicationId);
            if (applicationIds.Count > 0)
                query = query.Where(x => applicationIds.Contains(x.ApplicationId));

            var buildingIds = ExtractIds(filter.BuildingId);
            if (buildingIds.Count > 0)
                query = query.Where(x => x.BuildingId.HasValue && buildingIds.Contains(x.BuildingId.Value));

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

        public async Task<MonitoringConfig> AddAsync(MonitoringConfig config)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                config.ApplicationId = applicationId.Value;
            }
            else if (config.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(config.ApplicationId);
            ValidateApplicationIdForEntity(config, applicationId, isSystemAdmin);

            _context.MonitoringConfigs.Add(config);
            await _context.SaveChangesAsync();
            return config;
        }

        public async Task UpdateAsync(MonitoringConfig config)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(config.ApplicationId);
            ValidateApplicationIdForEntity(config, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MonitoringConfigs
                .Where(d => d.Id == id);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var config = await query.FirstOrDefaultAsync();

            if (config == null)
                return;

            _context.MonitoringConfigs.Remove(config);
            await _context.SaveChangesAsync();
        }
    }
}
