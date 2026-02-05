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
        /// <param name="request">Request containing groupId and list of building IDs</param>
        /// <returns></returns>
        [HttpPost("assign")]
        public async Task<IActionResult> Assign([FromBody] AssignBuildingsToGroupRequest request)
        {
            if (request.BuildingIds == null || !request.BuildingIds.Any())
            {
                return BadRequest(ApiResponse.BadRequest("BuildingIds list is required"));
            }

            await _service.AssignBuildingsToGroupAsync(request.GroupId, request.BuildingIds);
            return Ok(ApiResponse.Success($"Buildings assigned to group {request.GroupId} successfully"));
        }

        /// <summary>
        /// Get all accessible buildings for a specific group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns></returns>
        [HttpGet("buildings")]
        public async Task<IActionResult> GetBuildings([FromQuery] Guid groupId)
        {
            var buildings = await _service.GetGroupAccessibleBuildingsAsync(groupId);
            return Ok(ApiResponse.Success("Group's accessible buildings retrieved successfully", buildings));
        }

        /// <summary>
        /// Get all groups with access to a specific building
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <returns></returns>
        [HttpGet("groups")]
        public async Task<IActionResult> GetGroups([FromQuery] Guid buildingId)
        {
            var groups = await _service.GetGroupsByBuildingAsync(buildingId);
            return Ok(ApiResponse.Success("Groups with access to building retrieved successfully", groups));
        }

        /// <summary>
        /// Revoke building access from a group
        /// </summary>
        /// <param name="request">Request containing groupId and buildingId</param>
        /// <returns></returns>
        [HttpDelete("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeAccessRequest request)
        {
            await _service.RevokeBuildingAccessAsync(request.GroupId, request.BuildingId);
            return Ok(ApiResponse.Success($"Building access revoked from group {request.GroupId} successfully"));
        }

        /// <summary>
        /// Revoke all building accesses from a group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns></returns>
        [HttpDelete("revoke-all")]
        public async Task<IActionResult> RevokeAll([FromQuery] Guid groupId)
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
        [HttpGet("check")]
        public async Task<IActionResult> Check([FromQuery] Guid groupId, [FromQuery] Guid buildingId)
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
        public Guid GroupId { get; set; }
        public List<Guid> BuildingIds { get; set; }
    }

    /// <summary>
    /// Request model for revoking building access from group
    /// </summary>
    public class RevokeAccessRequest
    {
        public Guid GroupId { get; set; }
        public Guid BuildingId { get; set; }
    }
}
