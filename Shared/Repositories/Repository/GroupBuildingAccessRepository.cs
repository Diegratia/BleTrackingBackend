using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class GroupBuildingAccessRepository : BaseRepository
    {
        public GroupBuildingAccessRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        /// <summary>
        /// Base query with multi-tenancy and status filtering
        /// </summary>
        private IQueryable<GroupBuildingAccess> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.GroupBuildingAccesses
                .Where(gba => gba.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        /// <summary>
        /// Manual projection to Read DTO (NOT using AutoMapper)
        /// </summary>
        private IQueryable<GroupBuildingAccessRead> ProjectToRead(IQueryable<GroupBuildingAccess> query)
        {
            return query.AsNoTracking()
                .Include(gba => gba.Group)
                .Include(gba => gba.Building)
                .Select(gba => new GroupBuildingAccessRead
                {
                    Id = gba.Id,
                    GroupId = gba.GroupId,
                    GroupName = gba.Group.Name,
                    GroupLevelPriority = gba.Group.LevelPriority.ToString(),
                    BuildingId = gba.BuildingId,
                    BuildingName = gba.Building.Name,
                    CreatedAt = gba.CreatedAt,
                    CreatedBy = gba.CreatedBy,
                    UpdatedAt = gba.UpdatedAt,
                    UpdatedBy = gba.UpdatedBy,
                    Status = gba.Status,
                    ApplicationId = gba.ApplicationId
                });
        }

        /// <summary>
        /// Get all building accesses for a specific group
        /// </summary>
        public async Task<List<GroupBuildingAccessRead>> GetByGroupIdAsync(Guid groupId)
        {
            var query = BaseEntityQuery()
                .Where(gba => gba.GroupId == groupId);

            return await ProjectToRead(query).ToListAsync();
        }

        /// <summary>
        /// Get list of accessible building IDs for a group
        /// Used for filtering in JWT token generation
        /// </summary>
        public async Task<List<Guid>> GetAccessibleBuildingIdsAsync(Guid groupId)
        {
            return await BaseEntityQuery()
                .Where(gba => gba.GroupId == groupId)
                .Select(gba => gba.BuildingId)
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// Get all groups with access to a specific building
        /// </summary>
        public async Task<List<GroupBuildingAccessRead>> GetByBuildingIdAsync(Guid buildingId)
        {
            var query = BaseEntityQuery()
                .Where(gba => gba.BuildingId == buildingId);

            return await ProjectToRead(query).ToListAsync();
        }

        /// <summary>
        /// Get a specific access record by group and building
        /// Returns entity for update/delete operations
        /// </summary>
        public async Task<GroupBuildingAccess?> GetByGroupAndBuildingAsync(Guid groupId, Guid buildingId)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(gba =>
                    gba.GroupId == groupId &&
                    gba.BuildingId == buildingId
                );
        }

        /// <summary>
        /// Get entity by ID for update/delete operations
        /// </summary>
        public async Task<GroupBuildingAccess?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(gba => gba.Id == id);
        }

        /// <summary>
        /// Add a single building access
        /// </summary>
        public async Task AddAccessAsync(GroupBuildingAccess access)
        {
            _context.GroupBuildingAccesses.Add(access);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Add multiple building accesses
        /// </summary>
        public async Task AddAccessRangeAsync(IEnumerable<GroupBuildingAccess> accesses)
        {
            _context.GroupBuildingAccesses.AddRange(accesses);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Remove building access (soft delete by setting Status = 0)
        /// </summary>
        public async Task RemoveAccessAsync(Guid groupId, Guid buildingId)
        {
            var access = await GetByGroupAndBuildingAsync(groupId, buildingId);

            if (access != null)
            {
                access.Status = 0;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Remove all building accesses for a group (soft delete)
        /// </summary>
        public async Task RemoveAllAccessesByGroupAsync(Guid groupId)
        {
            var accesses = await BaseEntityQuery()
                .Where(gba => gba.GroupId == groupId)
                .ToListAsync();

            foreach (var access in accesses)
            {
                access.Status = 0;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Remove all group accesses for a building (soft delete)
        /// </summary>
        public async Task RemoveAllAccessesByBuildingAsync(Guid buildingId)
        {
            var accesses = await BaseEntityQuery()
                .Where(gba => gba.BuildingId == buildingId)
                .ToListAsync();

            foreach (var access in accesses)
            {
                access.Status = 0;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Check if group has access to a specific building
        /// </summary>
        public async Task<bool> HasAccessAsync(Guid groupId, Guid buildingId)
        {
            return await BaseEntityQuery()
                .AnyAsync(gba =>
                    gba.GroupId == groupId &&
                    gba.BuildingId == buildingId
                );
        }
    }
}
