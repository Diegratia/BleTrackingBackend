using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    /// <summary>
    /// Service interface for managing user building access permissions
    /// </summary>
    public interface IUserBuildingAccessService
    {
        /// <summary>
        /// Get all accessible buildings for a specific user
        /// </summary>
        Task<IEnumerable<MstBuildingRead>> GetUserAccessibleBuildingsAsync(Guid userId);

        /// <summary>
        /// Get all users with access to a specific building
        /// </summary>
        Task<IEnumerable<UserBuildingAccessRead>> GetUsersByBuildingAsync(Guid buildingId);

        /// <summary>
        /// Assign multiple buildings to a user
        /// </summary>
        Task AssignBuildingsToUserAsync(Guid userId, List<Guid> buildingIds);

        /// <summary>
        /// Revoke building access from a user
        /// </summary>
        Task RevokeBuildingAccessAsync(Guid userId, Guid buildingId);

        /// <summary>
        /// Revoke all building accesses from a user
        /// </summary>
        Task RevokeAllBuildingAccessAsync(Guid userId);

        /// <summary>
        /// Check if user has access to a specific building
        /// </summary>
        Task<bool> HasAccessAsync(Guid userId, Guid buildingId);
    }
}
