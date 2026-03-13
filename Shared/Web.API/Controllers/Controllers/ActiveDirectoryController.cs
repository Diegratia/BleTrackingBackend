using System;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers
{
    /// <summary>
    /// Active Directory Sync API
    /// Provides endpoints for AD configuration and user synchronization
    /// Part of SaaS module: Active Directory Sync
    /// </summary>
    [Route("api/ad")]
    [ApiController]
    [Authorize]
    public class ActiveDirectoryController : ControllerBase
    {
        private readonly IAdSyncService _adSyncService;

        public ActiveDirectoryController(IAdSyncService adSyncService)
        {
            _adSyncService = adSyncService;
        }

        /// <summary>
        /// Get AD configuration for the current application
        /// </summary>
        [HttpGet("config")]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<IActionResult> GetConfig()
        {
            try
            {
                var config = await _adSyncService.GetConfigAsync();

                if (config == null)
                {
                    return Ok(ApiResponse.Success("No AD configuration found", new
                    {
                        isConfigured = false
                    }));
                }

                return Ok(ApiResponse.Success("AD configuration retrieved", config));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to get AD config: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get AD configuration by ID
        /// </summary>
        [HttpGet("config/{id}")]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<IActionResult> GetConfigById(Guid id)
        {
            try
            {
                var config = await _adSyncService.GetConfigByIdAsync(id);

                if (config == null)
                {
                    return Ok(ApiResponse.BadRequest("Configuration not found"));
                }

                return Ok(ApiResponse.Success("AD configuration retrieved", config));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to get AD config: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create or update AD configuration
        /// </summary>
        [HttpPost("config")]
        [MinLevel(LevelPriority.SuperAdmin)]
        public async Task<IActionResult> SaveConfig([FromBody] ActiveDirectoryConfigCreate config)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                    return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
                }

                var result = await _adSyncService.SaveConfigAsync(config);

                return Ok(ApiResponse.Created("AD configuration saved", result));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Ok(ApiResponse.Forbidden(ex.Message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to save AD config: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update existing AD configuration
        /// </summary>
        [HttpPut("config/{id}")]
        [MinLevel(LevelPriority.SuperAdmin)]
        public async Task<IActionResult> UpdateConfig(Guid id, [FromBody] ActiveDirectoryConfigUpdate config)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                    return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
                }

                var result = await _adSyncService.UpdateConfigAsync(id, config);

                return Ok(ApiResponse.Success("AD configuration updated", result));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Ok(ApiResponse.Forbidden(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return Ok(ApiResponse.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to update AD config: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete AD configuration
        /// </summary>
        [HttpDelete("config/{id}")]
        [MinLevel(LevelPriority.SuperAdmin)]
        public async Task<IActionResult> DeleteConfig(Guid id)
        {
            try
            {
                await _adSyncService.DeleteConfigAsync(id);

                return Ok(ApiResponse.Success("AD configuration deleted"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Ok(ApiResponse.Forbidden(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return Ok(ApiResponse.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to delete AD config: {ex.Message}"));
            }
        }

        /// <summary>
        /// Trigger manual AD sync
        /// </summary>
        [HttpPost("sync")]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<IActionResult> TriggerSync([FromBody] AdSyncTrigger trigger)
        {
            try
            {
                var result = await _adSyncService.TriggerSyncAsync(trigger);

                if (result.Success)
                {
                    return Ok(ApiResponse.Success(result.Message, result));
                }

                return Ok(ApiResponse.BadRequest(result.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Ok(ApiResponse.Forbidden(ex.Message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to trigger sync: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get AD sync status
        /// </summary>
        [HttpGet("sync/status")]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<IActionResult> GetSyncStatus()
        {
            try
            {
                var status = await _adSyncService.GetSyncStatusAsync();

                return Ok(ApiResponse.Success("Sync status retrieved", status));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to get sync status: {ex.Message}"));
            }
        }

        /// <summary>
        /// Enable or disable AD sync
        /// </summary>
        [HttpPost("toggle/{id}")]
        [MinLevel(LevelPriority.SuperAdmin)]
        public async Task<IActionResult> ToggleSync(Guid id, [FromBody] ToggleSyncRequest request)
        {
            try
            {
                await _adSyncService.ToggleSyncAsync(id, request.Enabled);

                return Ok(ApiResponse.Success($"AD sync {(request.Enabled ? "enabled" : "disabled")}"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Ok(ApiResponse.Forbidden(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return Ok(ApiResponse.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to toggle sync: {ex.Message}"));
            }
        }

        /// <summary>
        /// Test AD connection
        /// </summary>
        [HttpPost("test/{id}")]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<IActionResult> TestConnection(Guid id)
        {
            try
            {
                var (success, message) = await _adSyncService.TestConnectionAsync(id);

                if (success)
                {
                    return Ok(ApiResponse.Success(message));
                }

                return Ok(ApiResponse.BadRequest(message));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Ok(ApiResponse.Forbidden(ex.Message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to test connection: {ex.Message}"));
            }
        }
    }

    /// <summary>
    /// Toggle sync request
    /// </summary>
    public class ToggleSyncRequest
    {
        public bool Enabled { get; set; }
    }
}
