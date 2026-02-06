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
        private readonly MonitoringConfigBuildingAccessRepository _buildingAccessRepository;

        public MonitoringConfigRepository(
            BleTrackingDbContext context,
            IHttpContextAccessor httpContextAccessor,
            MonitoringConfigBuildingAccessRepository buildingAccessRepository)
            : base(context, httpContextAccessor)
        {
            _buildingAccessRepository = buildingAccessRepository;
        }

        private IQueryable<MonitoringConfig> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MonitoringConfigs.AsQueryable();

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            // Building access filter untuk PrimaryAdmin (bukan System/SuperAdmin)
            // Now filters via junction table
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                var configIdsWithAccess = _context.MonitoringConfigBuildingAccesses
                    .Where(mcba => mcba.Status != 0 &&
                                  accessibleBuildingIds.Contains(mcba.BuildingId))
                    .Select(mcba => mcba.MonitoringConfigId)
                    .Distinct();

                query = query.Where(x => configIdsWithAccess.Contains(x.Id));
            }

            return query;
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
            return query.AsNoTracking()
                .Include(x => x.BuildingAccesses)
                    .ThenInclude(ba => ba.Building)
                .Select(x => new MonitoringConfigRead
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Config = x.Config,
                    BuildingIds = x.BuildingAccesses
                        .Where(ba => ba.Status != 0)
                        .Select(ba => ba.BuildingId)
                        .ToList(),
                    BuildingNames = x.BuildingAccesses
                        .Where(ba => ba.Status != 0)
                        .Select(ba => ba.Building.Name)
                        .ToList(),
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

            // Building access filter untuk PrimaryAdmin - now via junction table
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                var configIdsWithAccess = _context.MonitoringConfigBuildingAccesses
                    .Where(mcba => mcba.Status != 0 &&
                                  accessibleBuildingIds.Contains(mcba.BuildingId))
                    .Select(mcba => mcba.MonitoringConfigId)
                    .Distinct();

                query = query.Where(x => configIdsWithAccess.Contains(x.Id));
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

            // Filter by buildings via junction table
            var buildingIds = ExtractIds(filter.BuildingId);
            if (buildingIds.Count > 0)
            {
                var configIdsWithBuildings = _context.MonitoringConfigBuildingAccesses
                    .Where(mcba => mcba.Status != 0 &&
                                  buildingIds.Contains(mcba.BuildingId))
                    .Select(mcba => mcba.MonitoringConfigId)
                    .Distinct();

                query = query.Where(x => configIdsWithBuildings.Contains(x.Id));
            }

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

        public async Task<MonitoringConfig> AddAsync(MonitoringConfig config, List<Guid> buildingIds)
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

            // Validate building ownership
            if (buildingIds != null && buildingIds.Any())
            {
                var invalidBuildingIds = await _buildingAccessRepository
                    .CheckInvalidBuildingOwnershipAsync(buildingIds, config.ApplicationId);
                if (invalidBuildingIds.Any())
                    throw new UnauthorizedAccessException(
                        $"BuildingIds do not belong to this Application: {string.Join(", ", invalidBuildingIds)}");
            }

            _context.MonitoringConfigs.Add(config);
            await _context.SaveChangesAsync();

            // Add building accesses
            if (buildingIds != null && buildingIds.Any())
            {
                var buildingAccesses = buildingIds.Select(buildingId =>
                    new MonitoringConfigBuildingAccess
                    {
                        MonitoringConfigId = config.Id,
                        BuildingId = buildingId,
                        ApplicationId = config.ApplicationId,
                        Status = 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = config.CreatedBy,
                        UpdatedBy = config.UpdatedBy
                    }).ToList();

                await _buildingAccessRepository.AddAccessRangeAsync(buildingAccesses);
            }

            return config;
        }

        public async Task UpdateAsync(MonitoringConfig config, List<Guid>? buildingIds)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(config.ApplicationId);
            ValidateApplicationIdForEntity(config, applicationId, isSystemAdmin);

            // Validate building ownership if provided
            if (buildingIds != null && buildingIds.Any())
            {
                var invalidBuildingIds = await _buildingAccessRepository
                    .CheckInvalidBuildingOwnershipAsync(buildingIds, config.ApplicationId);
                if (invalidBuildingIds.Any())
                    throw new UnauthorizedAccessException(
                        $"BuildingIds do not belong to this Application: {string.Join(", ", invalidBuildingIds)}");
            }

            // Update building accesses if provided
            if (buildingIds != null)
            {
                // Remove all existing
                await _buildingAccessRepository.RemoveAllAccessesByConfigAsync(config.Id);

                // Add new ones
                if (buildingIds.Any())
                {
                    var buildingAccesses = buildingIds.Select(buildingId =>
                        new MonitoringConfigBuildingAccess
                        {
                            MonitoringConfigId = config.Id,
                            BuildingId = buildingId,
                            ApplicationId = config.ApplicationId,
                            Status = 1,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedBy = config.UpdatedBy,
                            UpdatedBy = config.UpdatedBy
                        }).ToList();

                    await _buildingAccessRepository.AddAccessRangeAsync(buildingAccesses);
                }
            }

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

            // Soft delete all building accesses
            await _buildingAccessRepository.RemoveAllAccessesByConfigAsync(id);

            _context.MonitoringConfigs.Remove(config);
            await _context.SaveChangesAsync();
        }
    }
}
