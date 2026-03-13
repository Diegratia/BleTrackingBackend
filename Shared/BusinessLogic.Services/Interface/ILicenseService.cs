using System;
using System.Threading.Tasks;
using Shared.Contracts.Read;
using Standard.Licensing;

namespace BusinessLogic.Services.Interface
{
    /// <summary>
    /// Service for license validation and management
    /// </summary>
    public interface ILicenseService
    {
        /// <summary>
        /// Get current license information for the application
        /// </summary>
        Task<LicenseRead> GetLicenseInfoAsync();

        /// <summary>
        /// Get license information for a specific application
        /// </summary>
        Task<LicenseRead> GetLicenseInfoAsync(Guid applicationId);

        /// <summary>
        /// Validate a license file without activating it
        /// </summary>
        Task<(bool IsValid, string Message)> ValidateLicenseAsync(string content, bool isBase64 = false);

        /// <summary>
        /// Activate a license file for the current application
        /// </summary>
        Task<(bool Success, string Message)> ActivateLicenseAsync(string content, bool isBase64 = false);

        /// <summary>
        /// Get the current machine ID
        /// </summary>
        string GetMachineId();

        /// <summary>
        /// Check if the current license is valid (not expired)
        /// </summary>
        Task<bool> IsLicenseValidAsync();

        /// <summary>
        /// Check if a specific feature is enabled
        /// </summary>
        bool IsFeatureEnabled(string featureKey);

        /// <summary>
        /// Get all enabled features
        /// </summary>
        Task<System.Collections.Generic.List<string>> GetEnabledFeaturesAsync();

        /// <summary>
        /// Validate if adding a new resource is allowed under the current license
        /// Throws UnauthorizedException if limit reached
        /// </summary>
        Task ValidateUsageAsync(string resourceType);
    }
}
