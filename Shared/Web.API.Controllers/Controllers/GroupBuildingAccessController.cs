using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Interface;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Web.API.Controllers.Controllers
{
    [MinLevel(LevelPriority.SuperAdmin)]
    [Route("api/access")]
    [ApiController]
    public class GroupBuildingAccessController : ControllerBase
    {
        private readonly IGroupBuildingAccessService _service;

        public GroupBuildingAccessController(IGroupBuildingAccessService service)
        {
            _service = service;
        }

        /// <summary>
        /// Assign multiple buildings to a group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="request">Request containing list of building IDs</param>
        /// <returns></returns>
        [HttpPost("group/{groupId}/assign")]
        public async Task<IActionResult> AssignBuildingsToGroup(Guid groupId, [FromBody] AssignBuildingsToGroupRequest request)
        {
            if (request.BuildingIds == null || !request.BuildingIds.Any())
            {
                return BadRequest(ApiResponse.BadRequest("BuildingIds list is required"));
            }

            await _service.AssignBuildingsToGroupAsync(groupId, request.BuildingIds);
            return Ok(ApiResponse.Success($"Buildings assigned to group {groupId} successfully"));
        }

        /// <summary>
        /// Get all accessible buildings for a specific group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns></returns>
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetGroupAccessibleBuildings(Guid groupId)
        {
            var buildings = await _service.GetGroupAccessibleBuildingsAsync(groupId);
            return Ok(ApiResponse.Success("Group's accessible buildings retrieved successfully", buildings));
        }

        /// <summary>
        /// Get all groups with access to a specific building
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <returns></returns>
        [HttpGet("building/{buildingId}")]
        public async Task<IActionResult> GetGroupsByBuilding(Guid buildingId)
        {
            var groups = await _service.GetGroupsByBuildingAsync(buildingId);
            return Ok(ApiResponse.Success("Groups with access to building retrieved successfully", groups));
        }

        /// <summary>
        /// Revoke building access from a group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="buildingId">Building ID</param>
        /// <returns></returns>
        [HttpDelete("group/{groupId}/building/{buildingId}")]
        public async Task<IActionResult> RevokeBuildingAccess(Guid groupId, Guid buildingId)
        {
            await _service.RevokeBuildingAccessAsync(groupId, buildingId);
            return Ok(ApiResponse.Success($"Building access revoked from group {groupId} successfully"));
        }

        /// <summary>
        /// Revoke all building accesses from a group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns></returns>
        [HttpDelete("group/{groupId}/all")]
        public async Task<IActionResult> RevokeAllBuildingAccess(Guid groupId)
        {
            await _service.RevokeAllBuildingAccessAsync(groupId);
            return Ok(ApiResponse.Success($"All building access revoked from group {groupId} successfully"));
        }

        /// <summary>
        /// Check if group has access to a specific building
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="buildingId">Building ID</param>
        /// <returns></returns>
        [HttpGet("group/{groupId}/building/{buildingId}/check")]
        public async Task<IActionResult> HasAccess(Guid groupId, Guid buildingId)
        {
            var hasAccess = await _service.HasAccessAsync(groupId, buildingId);
            return Ok(ApiResponse.Success("Access check completed", new
            {
                GroupId = groupId,
                BuildingId = buildingId,
                HasAccess = hasAccess
            }));
        }
    }

    /// <summary>
    /// Request model for assigning buildings to group
    /// </summary>
    public class AssignBuildingsToGroupRequest
    {
        public List<Guid> BuildingIds { get; set; }
    }
}
