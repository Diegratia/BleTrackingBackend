using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interface
{
    /// <summary>
    /// Service for managing feature flags and license-based feature access
    /// </summary>
    public interface IFeatureService
    {
        /// <summary>
        /// Check if a specific feature is enabled for the current application
        /// </summary>
        bool IsFeatureEnabled(string featureKey);

        /// <summary>
        /// Check if a specific feature is enabled for a specific application
        /// </summary>
        Task<bool> IsFeatureEnabledAsync(string featureKey, Guid applicationId);

        /// <summary>
        /// Get all enabled features for the current application
        /// </summary>
        Task<List<string>> GetEnabledFeaturesAsync();

        /// <summary>
        /// Get all enabled features for a specific application
        /// </summary>
        Task<List<string>> GetEnabledFeaturesAsync(Guid applicationId);

        /// <summary>
        /// Enable a feature for an application
        /// </summary>
        Task EnableFeatureAsync(Guid applicationId, string featureKey);

        /// <summary>
        /// Disable a feature for an application
        /// </summary>
        Task DisableFeatureAsync(Guid applicationId, string featureKey);

        /// <summary>
        /// Set enabled features for an application (replaces all)
        /// </summary>
        Task SetEnabledFeaturesAsync(Guid applicationId, List<string> featureKeys);

        /// <summary>
        /// Refresh feature cache for an application
        /// </summary>
        Task RefreshCacheAsync(Guid applicationId);

        /// <summary>
        /// Get feature info (name, description, enabled status)
        /// </summary>
        Task<Dictionary<string, FeatureInfo>> GetFeatureInfoAsync();
    }

    /// <summary>
    /// Feature information DTO
    /// </summary>
    public class FeatureInfo
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsCore { get; set; }
        public bool IsEnabled { get; set; }
        public string Category { get; set; } // "core" or "saas"
    }

    /// <summary>
    /// License information DTO
    /// </summary>
    public class LicenseInfo
    {
        public string LicenseType { get; set; }
        public string LicenseTier { get; set; }
        public string CustomerName { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? DaysRemaining { get; set; }
        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; }
        public Dictionary<string, bool> Features { get; set; } = new Dictionary<string, bool>();
    }
}
