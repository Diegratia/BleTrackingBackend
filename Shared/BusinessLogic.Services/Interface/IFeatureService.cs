using System.Collections.Generic;
using System.Threading.Tasks;
using QuestPDF.Infrastructure;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    /// <summary>
    /// Service for managing feature flags and license-based feature access
    /// </summary>
    public interface IFeatureService
    {
        /// <summary>
        /// Check if a specific feature is enabled for the current application (Sync)
        /// </summary>
        bool IsFeatureEnabled(string featureKey);

        /// <summary>
        /// Get all enabled feature keys for the current application
        /// </summary>
        Task<List<string>> GetEnabledFeaturesAsync();

        /// <summary>
        /// Get categorized features (core vs module) for UI
        /// </summary>
        Task<CategorizedFeaturesRead> GetCategorizedFeaturesAsync();

        /// <summary>
        /// Manually toggle an optional module (AD Sync or SSO)
        /// Only possible if the license permits the feature.
        /// </summary>
        /// <param name="featureKey">The feature key (module.activeDirectory or module.sso)</param>
        /// <param name="enabled">True to enable, false to disable</param>
        Task<bool> ToggleModuleAsync(string featureKey, bool enabled);

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
        public string Category { get; set; } // "core" or "module"
    }
}
