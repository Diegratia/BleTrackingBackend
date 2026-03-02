using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    /// <summary>
    /// Service interface for managing group building access permissions
    /// </summary>
    public interface IGroupBuildingAccessService
    {
        /// <summary>
        /// Get all accessible buildings for a specific group
        /// </summary>
        Task<List<MstBuildingRead>> GetGroupAccessibleBuildingsAsync(Guid groupId);

        /// <summary>
        /// Get all groups with access to a specific building
        /// </summary>
        Task<List<GroupBuildingAccessRead>> GetGroupsByBuildingAsync(Guid buildingId);

        /// <summary>
        /// Assign multiple buildings to a group
        /// </summary>
        Task AssignBuildingsToGroupAsync(Guid groupId, List<Guid> buildingIds);

        /// <summary>
        /// Revoke building access from a group
        /// </summary>
        Task RevokeBuildingAccessAsync(Guid groupId, Guid buildingId);

        /// <summary>
        /// Revoke all building accesses from a group
        /// </summary>
        Task RevokeAllBuildingAccessAsync(Guid groupId);

        /// <summary>
        /// Check if group has access to a specific building
        /// </summary>
        Task<bool> HasAccessAsync(Guid groupId, Guid buildingId);
    }
}
