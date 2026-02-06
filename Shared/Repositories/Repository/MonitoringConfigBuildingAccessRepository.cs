using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class MonitoringConfigBuildingAccessRepository : BaseRepository
    {
        public MonitoringConfigBuildingAccessRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        /// <summary>
        /// Base query with multi-tenancy and status filtering
        /// </summary>
        private IQueryable<MonitoringConfigBuildingAccess> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MonitoringConfigBuildingAccesses
                .Where(mcba => mcba.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        /// <summary>
        /// Manual projection to Read DTO (NOT using AutoMapper)
        /// </summary>
        private IQueryable<MonitoringConfigBuildingAccessRead> ProjectToRead(IQueryable<MonitoringConfigBuildingAccess> query)
        {
            return query.AsNoTracking()
                .Include(mcba => mcba.MonitoringConfig)
                .Include(mcba => mcba.Building)
                .Select(mcba => new MonitoringConfigBuildingAccessRead
                {
                    Id = mcba.Id,
                    MonitoringConfigId = mcba.MonitoringConfigId,
                    MonitoringConfigName = mcba.MonitoringConfig.Name,
                    BuildingId = mcba.BuildingId,
                    BuildingName = mcba.Building.Name,
                    CreatedAt = mcba.CreatedAt,
                    CreatedBy = mcba.CreatedBy,
                    UpdatedAt = mcba.UpdatedAt,
                    UpdatedBy = mcba.UpdatedBy,
                    Status = mcba.Status,
                    ApplicationId = mcba.ApplicationId
                });
        }

        /// <summary>
        /// Get all building accesses for a specific monitoring config
        /// </summary>
        public async Task<List<MonitoringConfigBuildingAccessRead>> GetByMonitoringConfigIdAsync(Guid monitoringConfigId)
        {
            var query = BaseEntityQuery()
                .Where(mcba => mcba.MonitoringConfigId == monitoringConfigId);

            return await ProjectToRead(query).ToListAsync();
        }

        /// <summary>
        /// Get list of accessible building IDs for a monitoring config
        /// </summary>
        public async Task<List<Guid>> GetAccessibleBuildingIdsAsync(Guid monitoringConfigId)
        {
            return await BaseEntityQuery()
                .Where(mcba => mcba.MonitoringConfigId == monitoringConfigId)
                .Select(mcba => mcba.BuildingId)
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// Get all monitoring configs with access to a specific building
        /// </summary>
        public async Task<List<MonitoringConfigBuildingAccessRead>> GetByBuildingIdAsync(Guid buildingId)
        {
            var query = BaseEntityQuery()
                .Where(mcba => mcba.BuildingId == buildingId);

            return await ProjectToRead(query).ToListAsync();
        }

        /// <summary>
        /// Get a specific access record by monitoring config and building
        /// Returns entity for update/delete operations
        /// </summary>
        public async Task<MonitoringConfigBuildingAccess?> GetByConfigAndBuildingAsync(Guid monitoringConfigId, Guid buildingId)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(mcba =>
                    mcba.MonitoringConfigId == monitoringConfigId &&
                    mcba.BuildingId == buildingId
                );
        }

        /// <summary>
        /// Get entity by ID for update/delete operations
        /// </summary>
        public async Task<MonitoringConfigBuildingAccess?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(mcba => mcba.Id == id);
        }

        /// <summary>
        /// Add a single building access
        /// </summary>
        public async Task AddAccessAsync(MonitoringConfigBuildingAccess access)
        {
            _context.MonitoringConfigBuildingAccesses.Add(access);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Add multiple building accesses
        /// </summary>
        public async Task AddAccessRangeAsync(IEnumerable<MonitoringConfigBuildingAccess> accesses)
        {
            _context.MonitoringConfigBuildingAccesses.AddRange(accesses);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Remove building access (soft delete by setting Status = 0)
        /// </summary>
        public async Task RemoveAccessAsync(Guid monitoringConfigId, Guid buildingId)
        {
            var access = await GetByConfigAndBuildingAsync(monitoringConfigId, buildingId);

            if (access != null)
            {
                access.Status = 0;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Remove all building accesses for a monitoring config (soft delete)
        /// </summary>
        public async Task RemoveAllAccessesByConfigAsync(Guid monitoringConfigId)
        {
            var accesses = await BaseEntityQuery()
                .Where(mcba => mcba.MonitoringConfigId == monitoringConfigId)
                .ToListAsync();

            foreach (var access in accesses)
            {
                access.Status = 0;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Remove all monitoring config accesses for a building (soft delete)
        /// </summary>
        public async Task RemoveAllAccessesByBuildingAsync(Guid buildingId)
        {
            var accesses = await BaseEntityQuery()
                .Where(mcba => mcba.BuildingId == buildingId)
                .ToListAsync();

            foreach (var access in accesses)
            {
                access.Status = 0;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Check if monitoring config has access to a specific building
        /// </summary>
        public async Task<bool> HasAccessAsync(Guid monitoringConfigId, Guid buildingId)
        {
            return await BaseEntityQuery()
                .AnyAsync(mcba =>
                    mcba.MonitoringConfigId == monitoringConfigId &&
                    mcba.BuildingId == buildingId
                );
        }

        /// <summary>
        /// Check invalid building ownership - validates buildings belong to the same Application
        /// Prevents cross-tenant access
        /// </summary>
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidBuildingOwnershipAsync(
            Guid buildingId, Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstBuilding>(
                new[] { buildingId }, applicationId);
        }

        /// <summary>
        /// Check invalid building ownership for multiple buildings
        /// </summary>
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidBuildingOwnershipAsync(
            IReadOnlyCollection<Guid> buildingIds, Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstBuilding>(
                buildingIds, applicationId);
        }
    }
}
