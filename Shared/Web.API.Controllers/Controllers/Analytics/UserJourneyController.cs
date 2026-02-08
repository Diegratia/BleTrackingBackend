using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Contracts.Analytics;
using BusinessLogic.Services.Interface.Analytics;
using BusinessLogic.Services.Extension.RootExtension;
using Data.ViewModels.ResponseHelper;

namespace Web.API.Controllers.Controllers.Analytics
{
    /// <summary>
    /// Controller for User Journey Analytics
    /// Provides endpoints for common paths analysis and security checks
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [MinLevel(LevelPriority.PrimaryAdmin)]
    public class UserJourneyController : ControllerBase
    {
        private readonly IUserJourneyService _service;
        private readonly ILogger<UserJourneyController> _logger;

        public UserJourneyController(
            IUserJourneyService service,
            ILogger<UserJourneyController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get common paths analysis - most popular journey sequences
        /// POST /api/UserJourney/common-paths
        /// </summary>
        /// <param name="filter">Filter criteria including date range, building, floor, etc.</param>
        /// <returns>Common paths with journey counts, percentages, and risk levels</returns>
        [HttpPost("common-paths")]
        public async Task<IActionResult> GetCommonPaths([FromBody] UserJourneyFilter filter)
        {
            try
            {
                _logger.LogInformation("Getting common paths analysis");

                var result = await _service.GetCommonPathsAsync(filter);

                return Ok(ApiResponse.Success("Common paths retrieved successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting common paths");
                return Ok(ApiResponse.InternalError($"Error getting common paths: {ex.Message}"));
            }
        }

        /// <summary>
        /// Perform security journey check for a specific visitor
        /// POST /api/UserJourney/security-check/visitor/{visitorId}
        /// </summary>
        /// <param name="visitorId">Visitor ID to check</param>
        /// <param name="filter">Filter criteria including date range</param>
        /// <returns>Security check results with violations and risk level</returns>
        [HttpPost("security-check/visitor/{visitorId}")]
        public async Task<IActionResult> GetSecurityCheckForVisitor(
            Guid visitorId,
            [FromBody] UserJourneyFilter filter)
        {
            try
            {
                _logger.LogInformation("Performing security check for visitor {VisitorId}", visitorId);

                var result = await _service.GetSecurityCheckForVisitorAsync(visitorId, filter);

                if (result == null)
                {
                    return Ok(ApiResponse.NotFound($"Visitor {visitorId} not found or no journey data available"));
                }

                return Ok(ApiResponse.Success("Security check completed successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing security check for visitor {VisitorId}", visitorId);
                return Ok(ApiResponse.InternalError($"Error performing security check: {ex.Message}"));
            }
        }

        /// <summary>
        /// Perform security journey check for a specific member
        /// POST /api/UserJourney/security-check/member/{memberId}
        /// </summary>
        /// <param name="memberId">Member ID to check</param>
        /// <param name="filter">Filter criteria including date range</param>
        /// <returns>Security check results with violations and risk level</returns>
        [HttpPost("security-check/member/{memberId}")]
        public async Task<IActionResult> GetSecurityCheckForMember(
            Guid memberId,
            [FromBody] UserJourneyFilter filter)
        {
            try
            {
                _logger.LogInformation("Performing security check for member {MemberId}", memberId);

                var result = await _service.GetSecurityCheckForMemberAsync(memberId, filter);

                if (result == null)
                {
                    return Ok(ApiResponse.NotFound($"Member {memberId} not found or no journey data available"));
                }

                return Ok(ApiResponse.Success("Security check completed successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing security check for member {MemberId}", memberId);
                return Ok(ApiResponse.InternalError($"Error performing security check: {ex.Message}"));
            }
        }
    }
}
