using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers
{
    /// <summary>
    /// License Management API
    /// Provides endpoints for license validation, feature checking, and activation
    /// </summary>
    [Route("api/license")]
    [ApiController]
    [Authorize]
    public class LicenseController : ControllerBase
    {
        private readonly ILicenseService _licenseService;
        private readonly IFeatureService _featureService;

        public LicenseController(ILicenseService licenseService, IFeatureService featureService)
        {
            _licenseService = licenseService;
            _featureService = featureService;
        }

        /// <summary>
        /// Get current license information
        /// </summary>
        /// <returns>License type, expiration, features, customer info</returns>
        [HttpGet("info")]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<IActionResult> GetLicenseInfo()
        {
            try
            {
                var info = await _licenseService.GetLicenseInfoAsync();

                if (!info.IsValid)
                {
                    return Ok(ApiResponse.Success("License information retrieved", new
                    {
                        isValid = false,
                        message = info.ValidationMessage,
                        licenseType = info.LicenseType,
                        licenseTier = info.LicenseTier,
                        customerName = info.CustomerName,
                        expirationDate = info.ExpirationDate,
                        daysRemaining = info.DaysRemaining,
                        features = info.Features
                    }));
                }

                return Ok(ApiResponse.Success("License information retrieved", new
                {
                    isValid = true,
                    message = "License is valid",
                    licenseType = info.LicenseType,
                    licenseTier = info.LicenseTier,
                    customerName = info.CustomerName,
                    expirationDate = info.ExpirationDate,
                    daysRemaining = info.DaysRemaining,
                    features = new
                    {
                        core = new
                        {
                            masterData = info.Features.GetValueOrDefault("core.masterData", false),
                            tracking = info.Features.GetValueOrDefault("core.tracking", false),
                            monitoring = info.Features.GetValueOrDefault("core.monitoring", false),
                            alarm = info.Features.GetValueOrDefault("core.alarm", false),
                            patrol = info.Features.GetValueOrDefault("core.patrol", false),
                            reporting = info.Features.GetValueOrDefault("core.reporting", false)
                        },
                        saas = new
                        {
                            activeDirectory = info.Features.GetValueOrDefault("saas.activeDirectory", false),
                            sso = info.Features.GetValueOrDefault("saas.sso", false)
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to get license info: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get enabled features/modules
        /// </summary>
        /// <returns>List of enabled features with metadata</returns>
        [HttpGet("features")]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<IActionResult> GetFeatures()
        {
            try
            {
                var features = await _featureService.GetFeatureInfoAsync();

                var coreFeatures = new Dictionary<string, object>();
                var saasFeatures = new Dictionary<string, object>();

                foreach (var kvp in features)
                {
                    var featureData = new
                    {
                        key = kvp.Value.Key,
                        displayName = kvp.Value.DisplayName,
                        description = kvp.Value.Description,
                        isEnabled = kvp.Value.IsEnabled
                    };

                    if (kvp.Value.IsCore)
                    {
                        coreFeatures[kvp.Key] = featureData;
                    }
                    else
                    {
                        saasFeatures[kvp.Key] = featureData;
                    }
                }

                return Ok(ApiResponse.Success("Features retrieved", new
                {
                    core = coreFeatures,
                    saas = saasFeatures
                }));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to get features: {ex.Message}"));
            }
        }

        /// <summary>
        /// Check if a specific feature is enabled
        /// </summary>
        /// <param name="featureKey">Feature key (e.g., "saas.activeDirectory", "core.tracking")</param>
        /// <returns>True if feature is enabled</returns>
        [HttpGet("feature/{featureKey}")]
        [Authorize]
        public async Task<IActionResult> IsFeatureEnabled(string featureKey)
        {
            try
            {
                var isEnabled = await _featureService.IsFeatureEnabledAsync(featureKey, Guid.Parse(User.FindFirst("ApplicationId")?.Value ?? Guid.Empty.ToString()));

                return Ok(ApiResponse.Success("Feature check completed", new
                {
                    featureKey,
                    isEnabled
                }));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to check feature: {ex.Message}"));
            }
        }

        /// <summary>
        /// Activate a license file (API alternative to CLI)
        /// </summary>
        /// <param name="request">License activation request with base64 encoded license</param>
        /// <returns>Activation result</returns>
        [HttpPost("activate")]
        [MinLevel(LevelPriority.SuperAdmin)]
        public async Task<IActionResult> ActivateLicense([FromBody] LicenseActivationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.LicenseContent))
                {
                    return Ok(ApiResponse.BadRequest("License content is required"));
                }

                // If base64 encoded, decode it
                string licenseContent = request.LicenseContent;
                if (request.IsBase64)
                {
                    try
                    {
                        var bytes = Convert.FromBase64String(request.LicenseContent);
                        licenseContent = System.Text.Encoding.UTF8.GetString(bytes);
                    }
                    catch
                    {
                        return Ok(ApiResponse.BadRequest("Invalid base64 encoding"));
                    }
                }

                var (success, message) = await _licenseService.ActivateLicenseAsync(licenseContent);

                if (success)
                {
                    return Ok(ApiResponse.Success(message, new
                    {
                        activatedAt = DateTime.UtcNow
                    }));
                }

                return Ok(ApiResponse.BadRequest(message));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to activate license: {ex.Message}"));
            }
        }

        /// <summary>
        /// Validate a license file without activating it
        /// </summary>
        /// <param name="request">License validation request</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate")]
        [MinLevel(LevelPriority.SuperAdmin)]
        public async Task<IActionResult> ValidateLicense([FromBody] LicenseActivationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.LicenseContent))
                {
                    return Ok(ApiResponse.BadRequest("License content is required"));
                }

                string licenseContent = request.LicenseContent;
                if (request.IsBase64)
                {
                    try
                    {
                        var bytes = Convert.FromBase64String(request.LicenseContent);
                        licenseContent = System.Text.Encoding.UTF8.GetString(bytes);
                    }
                    catch
                    {
                        return Ok(ApiResponse.BadRequest("Invalid base64 encoding"));
                    }
                }

                var (isValid, message) = await _licenseService.ValidateLicenseAsync(licenseContent);

                return Ok(ApiResponse.Success("Validation completed", new
                {
                    isValid,
                    message
                }));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to validate license: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get current machine ID
        /// </summary>
        /// <returns>Machine ID for license generation</returns>
        [HttpGet("machine-id")]
        [MinLevel(LevelPriority.SuperAdmin)]
        public IActionResult GetMachineId()
        {
            try
            {
                var machineId = _licenseService.GetMachineId();

                return Ok(ApiResponse.Success("Machine ID retrieved", new
                {
                    machineId,
                    instructions = "Provide this ID when requesting a license file"
                }));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to get machine ID: {ex.Message}"));
            }
        }

        /// <summary>
        /// Refresh feature cache (after license update)
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("feature/refresh")]
        [MinLevel(LevelPriority.SuperAdmin)]
        public async Task<IActionResult> RefreshFeatures()
        {
            try
            {
                await _licenseService.RefreshCacheAsync();
                var appId = Guid.Parse(User.FindFirst("ApplicationId")?.Value ?? Guid.Empty.ToString());
                await _featureService.RefreshCacheAsync(appId);

                return Ok(ApiResponse.Success("Feature cache refreshed"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse.BadRequest($"Failed to refresh cache: {ex.Message}"));
            }
        }
    }

    /// <summary>
    /// License activation/validation request
    /// </summary>
    public class LicenseActivationRequest
    {
        /// <summary>
        /// License file content (can be plain text or base64 encoded)
        /// </summary>
        public string LicenseContent { get; set; }

        /// <summary>
        /// Whether the license content is base64 encoded
        /// </summary>
        public bool IsBase64 { get; set; } = false;
    }
}
