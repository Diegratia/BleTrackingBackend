using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Interface;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers
{
    /// <summary>
    /// License Management API
    /// Provides endpoints for license information and activation
    /// </summary>
    [Route("api/license")]
    [ApiController]
    [Authorize]
    [MinLevel(LevelPriority.PrimaryAdmin)]
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
        public async Task<IActionResult> GetLicenseInfo()
        {
            var info = await _licenseService.GetLicenseInfoAsync();
            
            return Ok(ApiResponse.Success("License information retrieved", info));
        }

        /// <summary>
        /// Get enabled features/modules with categories
        /// </summary>
        /// <returns>List of enabled features categorized by core/module</returns>
        [HttpGet("features")]
        public async Task<IActionResult> GetFeatures()
        {
            var categorizedFeatures = await _featureService.GetCategorizedFeaturesAsync();
            return Ok(ApiResponse.Success("Features retrieved", categorizedFeatures));
        }

        /// <summary>
        /// Activate a license file
        /// </summary>
        /// <param name="file">License file (.lic)</param>
        /// <returns>Activation result</returns>
        [HttpPost("activate")]
        [MinLevel(LevelPriority.SuperAdmin)]
        public async Task<IActionResult> ActivateLicense(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.BadRequest("License file is required"));

            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(content))
                return BadRequest(ApiResponse.BadRequest("License content is empty"));

            var (success, message) = await _licenseService.ActivateLicenseAsync(content);
            return Ok(ApiResponse.Success(message, new { activatedAt = DateTime.UtcNow }));
        }

        /// <summary>
        /// Get current machine ID
        /// </summary>
        /// <returns>Machine ID for license generation</returns>
        [HttpGet("machine-id")]
        [MinLevel(LevelPriority.SuperAdmin)]
        public IActionResult GetMachineId()
        {
            var machineId = _licenseService.GetMachineId();
            return Ok(ApiResponse.Success("Machine ID retrieved", new { machineId }));
        }

        /// <summary>
        /// Toggle an optional module (AD Sync or SSO)
        /// </summary>
        /// <param name="request">Toggle request</param>
        /// <returns>Toggle result</returns>
        [HttpPost("module/toggle")]
        [MinLevel(LevelPriority.SuperAdmin)]
        public async Task<IActionResult> ToggleModule([FromBody] ModuleToggleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FeatureKey))
                return BadRequest(ApiResponse.BadRequest("Feature key is required"));

            var success = await _featureService.ToggleModuleAsync(request.FeatureKey, request.Enabled);
            if (!success)
            {
                return BadRequest(ApiResponse.BadRequest("Failed to toggle module. Ensure it is a valid optional module and permitted by your license."));
            }

            return Ok(ApiResponse.Success($"Module {request.FeatureKey} {(request.Enabled ? "enabled" : "disabled")} successfully"));
        }
    }

    /// <summary>
    /// Module toggle request model
    /// </summary>
    public class ModuleToggleRequest
    {
        public string FeatureKey { get; set; } = string.Empty;
        public bool Enabled { get; set; }
    }
}
