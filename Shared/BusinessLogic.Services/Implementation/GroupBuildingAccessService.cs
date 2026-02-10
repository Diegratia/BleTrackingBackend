using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Repository;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class GroupBuildingAccessService : BaseService, IGroupBuildingAccessService
    {
        private readonly GroupBuildingAccessRepository _accessRepository;
        private readonly UserGroupRepository _groupRepository;
        private readonly MstBuildingRepository _buildingRepository;
        private readonly UserRepository _userRepository;
        private readonly IAuditEmitter _audit;
        private readonly BleTrackingDbContext _context;

        public GroupBuildingAccessService(
            GroupBuildingAccessRepository accessRepository,
            UserGroupRepository groupRepository,
            MstBuildingRepository buildingRepository,
            UserRepository userRepository,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor,
            BleTrackingDbContext context) : base(httpContextAccessor)
        {
            _accessRepository = accessRepository;
            _groupRepository = groupRepository;
            _buildingRepository = buildingRepository;
            _userRepository = userRepository;
            _audit = audit;
            _context = context;
        }

        public async Task<List<MstBuildingRead>> GetGroupAccessibleBuildingsAsync(Guid groupId)
        {
            // Validate group exists and belongs to same application
            var (applicationId, isSystemAdmin) = _accessRepository.GetApplicationIdAndRole();

            if (!isSystemAdmin && applicationId.HasValue)
            {
                await _accessRepository.ValidateApplicationIdAsync(applicationId.Value);
            }

            // Get group's accessible buildings
            var accesses = await _accessRepository.GetByGroupIdAsync(groupId);

            // Project to MstBuildingRead (direct return, no mapper)
            var buildings = accesses
                .Where(a => a.BuildingId != Guid.Empty)
                .Select(a => new MstBuildingRead
                {
                    Id = a.BuildingId,
                    Name = a.BuildingName,
                    Status = a.Status,
                    ApplicationId = a.ApplicationId,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToList();

            return buildings;
        }

        public async Task<List<GroupBuildingAccessRead>> GetGroupsByBuildingAsync(Guid buildingId)
        {
            // Validate building exists and belongs to same application
            var (applicationId, isSystemAdmin) = _accessRepository.GetApplicationIdAndRole();

            if (!isSystemAdmin && applicationId.HasValue)
            {
                await _accessRepository.ValidateApplicationIdAsync(applicationId.Value);
            }

            // Get groups with access to building
            var groupAccesses = await _accessRepository.GetByBuildingIdAsync(buildingId);

            // Add member count for each group
            var result = new List<GroupBuildingAccessRead>();
            foreach (var access in groupAccesses)
            {
                // Get user count for this group
                var usersQuery = _context.Users
                    .Where(u => u.GroupId == access.GroupId && u.Status != 0);

                // Apply multi-tenancy filter manually
                if (!isSystemAdmin && applicationId.HasValue)
                {
                    usersQuery = usersQuery.Where(u => u.ApplicationId == applicationId.Value);
                }

                var userCount = await usersQuery.CountAsync();

                access.MemberCount = userCount;
                result.Add(access);
            }

            return result;
        }

        public async Task AssignBuildingsToGroupAsync(Guid groupId, List<Guid> buildingIds)
        {
            var (applicationId, isSystemAdmin) = _accessRepository.GetApplicationIdAndRole();

            // Validate group exists and belongs to same application
            var group = await _groupRepository.GetByIdEntityAsync(groupId);
            if (group == null)
                throw new Exception($"Group with ID {groupId} not found");

            if (!isSystemAdmin && applicationId.HasValue)
            {
                _accessRepository.ValidateApplicationIdForEntity(group, applicationId, isSystemAdmin);
            }

            // Validate all buildings exist and belong to same application
            var invalidBuildings = await _accessRepository.CheckInvalidOwnershipIdsAsync<MstBuilding>(
                buildingIds,
                applicationId ?? group.ApplicationId
            );

            if (invalidBuildings.Any())
            {
                throw new UnauthorizedAccessException(
                    $"Buildings {string.Join(", ", invalidBuildings)} do not exist or do not belong to your application"
                );
            }

            // Remove all existing accesses for this group
            await _accessRepository.RemoveAllAccessesByGroupAsync(groupId);

            // Create new accesses
            var newAccesses = buildingIds.Select(buildingId =>
            {
                var access = new GroupBuildingAccess
                {
                    GroupId = groupId,
                    BuildingId = buildingId,
                    ApplicationId = group.ApplicationId
                };

                SetCreateAudit(access);
                return access;
            }).ToList();

            await _accessRepository.AddAccessRangeAsync(newAccesses);

            // Emit audit
             _audit.Updated(
                "GroupBuildingAccess",
                groupId,
                $"Assigned {buildingIds.Count} buildings to group {group.Name}",
                new
                {
                    GroupId = groupId,
                    GroupName = group.Name,
                    BuildingCount = buildingIds.Count,
                    BuildingIds = buildingIds
                }
            );
        }

        public async Task RevokeBuildingAccessAsync(Guid groupId, Guid buildingId)
        {
            var (applicationId, isSystemAdmin) = _accessRepository.GetApplicationIdAndRole();

            // Validate access exists
            var hasAccess = await _accessRepository.HasAccessAsync(groupId, buildingId);
            if (!hasAccess)
                throw new Exception($"Group {groupId} does not have access to building {buildingId}");

            // Get details for audit
            var group = await _groupRepository.GetByIdEntityAsync(groupId);
            var building = await _buildingRepository.GetByIdAsync(buildingId);

            // Revoke access
            await _accessRepository.RemoveAccessAsync(groupId, buildingId);

            // Emit audit
             _audit.Deleted(
                "GroupBuildingAccess",
                $"{groupId}_{buildingId}",
                $"Revoked building {building?.Name} access from group {group?.Name}",
                new
                {
                    GroupId = groupId,
                    GroupName = group?.Name,
                    BuildingId = buildingId,
                    BuildingName = building?.Name
                }
            );
        }

        public async Task RevokeAllBuildingAccessAsync(Guid groupId)
        {
            var (applicationId, isSystemAdmin) = _accessRepository.GetApplicationIdAndRole();

            // Validate group exists
            var group = await _groupRepository.GetByIdEntityAsync(groupId);
            if (group == null)
                throw new Exception($"Group with ID {groupId} not found");

            if (!isSystemAdmin && applicationId.HasValue)
            {
                _accessRepository.ValidateApplicationIdForEntity(group, applicationId, isSystemAdmin);
            }

            // Get current accesses for audit
            var currentAccesses = await _accessRepository.GetByGroupIdAsync(groupId);

            // Revoke all accesses
            await _accessRepository.RemoveAllAccessesByGroupAsync(groupId);

            // Emit audit
             _audit.Deleted(
                "GroupBuildingAccess",
                groupId,
                $"Revoked all building access from group {group.Name}",
                new
                {
                    GroupId = groupId,
                    GroupName = group.Name,
                    RevokedBuildingCount = currentAccesses.Count
                }
            );
        }

        public async Task<bool> HasAccessAsync(Guid groupId, Guid buildingId)
        {
            return await _accessRepository.HasAccessAsync(groupId, buildingId);
        }
    }
}
