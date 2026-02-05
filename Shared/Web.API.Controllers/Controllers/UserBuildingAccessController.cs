using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Interface;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Read;

namespace Web.API.Controllers.Controllers
{
    [MinLevel(LevelPriority.SuperAdmin)]
    [Route("api/[controller]")]
    [ApiController]
    public class UserBuildingAccessController : ControllerBase
    {
        private readonly IUserBuildingAccessService _service;

        public UserBuildingAccessController(IUserBuildingAccessService service)
        {
            _service = service;
        }

        /// <summary>
        /// Assign multiple buildings to a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="request">Request containing list of building IDs</param>
        /// <returns></returns>
        [HttpPost("user/{userId}/assign")]
        public async Task<IActionResult> AssignBuildingsToUser(Guid userId, [FromBody] AssignBuildingsRequest request)
        {
            if (request.BuildingIds == null || !request.BuildingIds.Any())
            {
                return BadRequest(ApiResponse.BadRequest("BuildingIds list is required"));
            }

            await _service.AssignBuildingsToUserAsync(userId, request.BuildingIds);
            return Ok(ApiResponse.Success($"Buildings assigned to user {userId} successfully"));
        }

        /// <summary>
        /// Get all accessible buildings for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns></returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserAccessibleBuildings(Guid userId)
        {
            var buildings = await _service.GetUserAccessibleBuildingsAsync(userId);
            return Ok(ApiResponse.Success("User's accessible buildings retrieved successfully", buildings));
        }

        /// <summary>
        /// Get all users with access to a specific building
        /// </summary>
        /// <param name="buildingId">Building ID</param>
        /// <returns></returns>
        [HttpGet("building/{buildingId}")]
        public async Task<IActionResult> GetUsersByBuilding(Guid buildingId)
        {
            var users = await _service.GetUsersByBuildingAsync(buildingId);
            return Ok(ApiResponse.Success("Users with access to building retrieved successfully", users));
        }

        /// <summary>
        /// Revoke building access from a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="buildingId">Building ID</param>
        /// <returns></returns>
        [HttpDelete("user/{userId}/building/{buildingId}")]
        public async Task<IActionResult> RevokeBuildingAccess(Guid userId, Guid buildingId)
        {
            await _service.RevokeBuildingAccessAsync(userId, buildingId);
            return Ok(ApiResponse.Success($"Building access revoked from user {userId} successfully"));
        }

        /// <summary>
        /// Revoke all building accesses from a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns></returns>
        [HttpDelete("user/{userId}/all")]
        public async Task<IActionResult> RevokeAllBuildingAccess(Guid userId)
        {
            await _service.RevokeAllBuildingAccessAsync(userId);
            return Ok(ApiResponse.Success($"All building access revoked from user {userId} successfully"));
        }

        /// <summary>
        /// Check if user has access to a specific building
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="buildingId">Building ID</param>
        /// <returns></returns>
        [HttpGet("user/{userId}/building/{buildingId}/check")]
        public async Task<IActionResult> HasAccess(Guid userId, Guid buildingId)
        {
            var hasAccess = await _service.HasAccessAsync(userId, buildingId);
            return Ok(ApiResponse.Success("Access check completed", new
            {
                UserId = userId,
                BuildingId = buildingId,
                HasAccess = hasAccess
            }));
        }
    }

    /// <summary>
    /// Request model for assigning buildings to user
    /// </summary>
    public class AssignBuildingsRequest
    {
        public List<Guid> BuildingIds { get; set; }
    }
}
