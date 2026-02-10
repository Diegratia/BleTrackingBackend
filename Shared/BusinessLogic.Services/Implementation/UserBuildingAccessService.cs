using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using DataView;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class UserBuildingAccessService : BaseService, IUserBuildingAccessService
    {
        private readonly UserBuildingAccessRepository _accessRepository;
        private readonly UserRepository _userRepository;
        private readonly MstBuildingRepository _buildingRepository;
        private readonly IAuditEmitter _audit;

        public UserBuildingAccessService(
            UserBuildingAccessRepository accessRepository,
            UserRepository userRepository,
            MstBuildingRepository buildingRepository,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _accessRepository = accessRepository;
            _userRepository = userRepository;
            _buildingRepository = buildingRepository;
            _audit = audit;
        }

        public async Task<IEnumerable<MstBuildingRead>> GetUserAccessibleBuildingsAsync(Guid userId)
        {
            // Validate user exists and belongs to same application
            var (applicationId, isSystemAdmin) = _accessRepository.GetApplicationIdAndRole();

            if (!isSystemAdmin && applicationId.HasValue)
            {
                await _accessRepository.ValidateApplicationIdAsync(applicationId.Value);
            }

            // Get user's accessible buildings
            var accesses = await _accessRepository.GetByUserIdAsync(userId);

            // Project to MstBuildingRead
            var buildings = accesses
                .Where(a => a.Building != null && a.Status != 0)
                .Select(a => new MstBuildingRead
                {
                    Id = a.Building.Id,
                    Name = a.Building.Name,
                    Image = a.Building.Image,
                    Status = a.Building.Status,
                    ApplicationId = a.Building.ApplicationId,
                    CreatedAt = a.Building.CreatedAt,
                    UpdatedAt = a.Building.UpdatedAt
                })
                .ToList();

            return buildings;
        }

        public async Task<IEnumerable<UserBuildingAccessRead>> GetUsersByBuildingAsync(Guid buildingId)
        {
            // Validate building exists and belongs to same application
            var (applicationId, isSystemAdmin) = _accessRepository.GetApplicationIdAndRole();

            if (!isSystemAdmin && applicationId.HasValue)
            {
                await _accessRepository.ValidateApplicationIdAsync(applicationId.Value);
            }

            // Get users with access to building
            var accesses = await _accessRepository.GetByBuildingIdAsync(buildingId);

            // Project to UserBuildingAccessRead
            var result = accesses
                .Where(a => a.User != null && a.Status != 0)
                .Select(a => new UserBuildingAccessRead
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User.Username,
                    UserEmail = a.User.Email,
                    BuildingId = a.BuildingId,
                    BuildingName = a.Building?.Name,
                    CreatedAt = a.CreatedAt,
                    CreatedBy = a.CreatedBy
                })
                .ToList();

            return result;
        }

        public async Task AssignBuildingsToUserAsync(Guid userId, List<Guid> buildingIds)
        {
            var (applicationId, isSystemAdmin) = _accessRepository.GetApplicationIdAndRole();

            // Validate user exists and belongs to same application
            var user = await _userRepository.GetByIdEntityAsync(userId);
            if (user == null)
                throw new NotFoundException($"User with ID {userId} not found");

            if (!isSystemAdmin && applicationId.HasValue)
            {
                _accessRepository.ValidateApplicationIdForEntity(user, applicationId, isSystemAdmin);
            }

            // Validate all buildings exist and belong to same application
            var invalidBuildings = await _accessRepository.CheckInvalidOwnershipIdsAsync<MstBuilding>(
                buildingIds,
                applicationId ?? user.ApplicationId
            );

            if (invalidBuildings.Any())
            {
                throw new UnauthorizedAccessException(
                    $"Buildings {string.Join(", ", invalidBuildings)} do not exist or do not belong to your application"
                );
            }

            // Remove all existing accesses for this user
            await _accessRepository.RemoveAllAccessesByUserAsync(userId);

            // Create new accesses
            var newAccesses = buildingIds.Select(buildingId =>
            {
                var access = new UserBuildingAccess
                {
                    UserId = userId,
                    BuildingId = buildingId,
                    ApplicationId = user.ApplicationId
                };

                SetCreateAudit(access);
                return access;
            }).ToList();

            await _accessRepository.AddAccessRangeAsync(newAccesses);

            // Emit audit
             _audit.Updated(
                "UserBuildingAccess",
                userId,
                $"Assigned {buildingIds.Count} buildings to user {user.Username}",
                new
                {
                    UserId = userId,
                    Username = user.Username,
                    BuildingCount = buildingIds.Count,
                    BuildingIds = buildingIds
                }
            );
        }

        public async Task RevokeBuildingAccessAsync(Guid userId, Guid buildingId)
        {
            var (applicationId, isSystemAdmin) = _accessRepository.GetApplicationIdAndRole();

            // Validate access exists
            var hasAccess = await _accessRepository.HasAccessAsync(userId, buildingId);
            if (!hasAccess)
                throw new NotFoundException($"User {userId} does not have access to building {buildingId}");

            // Get details for audit
            var user = await _userRepository.GetByIdEntityAsync(userId);
            var building = await _buildingRepository.GetByIdAsync(buildingId);

            // Revoke access
            await _accessRepository.RemoveAccessAsync(userId, buildingId);

            // Emit audit
             _audit.Deleted(
                "UserBuildingAccess",
                $"{userId}_{buildingId}",
                $"Revoked building {building?.Name} access from user {user?.Username}",
                new
                {
                    UserId = userId,
                    Username = user?.Username,
                    BuildingId = buildingId,
                    BuildingName = building?.Name
                }
            );
        }

        public async Task RevokeAllBuildingAccessAsync(Guid userId)
        {
            var (applicationId, isSystemAdmin) = _accessRepository.GetApplicationIdAndRole();

            // Validate user exists
            var user = await _userRepository.GetByIdEntityAsync(userId);
            if (user == null)
                throw new NotFoundException($"User with ID {userId} not found");

            if (!isSystemAdmin && applicationId.HasValue)
            {
                _accessRepository.ValidateApplicationIdForEntity(user, applicationId, isSystemAdmin);
            }

            // Get current accesses for audit
            var currentAccesses = await _accessRepository.GetByUserIdAsync(userId);

            // Revoke all accesses
            await _accessRepository.RemoveAllAccessesByUserAsync(userId);

            // Emit audit
             _audit.Deleted(
                "UserBuildingAccess",
                userId,
                $"Revoked all building access from user {user.Username}",
                new
                {
                    UserId = userId,
                    Username = user.Username,
                    RevokedBuildingCount = currentAccesses.Count()
                }
            );
        }

        public async Task<bool> HasAccessAsync(Guid userId, Guid buildingId)
        {
            return await _accessRepository.HasAccessAsync(userId, buildingId);
        }
    }
}
