using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Extension;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.Repository;
using Shared.Contracts.Read;
using Shared.BusinessLogic.Services.Feature;

namespace BusinessLogic.Services.Implementation
{
    /// <summary>
    /// Service for managing feature flags and license-based feature access
    /// </summary>
    public class FeatureService : BaseService, IFeatureService
    {
        private readonly MstApplicationRepository _applicationRepository;
        private readonly ILogger<FeatureService> _logger;

        public FeatureService(
            MstApplicationRepository applicationRepository,
            ILogger<FeatureService> logger,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _applicationRepository = applicationRepository;
            _logger = logger;
        }

        /// <summary>
        /// Check if a specific feature is enabled for the current application
        /// </summary>
        public bool IsFeatureEnabled(string featureKey)
        {
            if (!FeatureDefinition.IsValidFeature(featureKey))
            {
                _logger.LogWarning($"Invalid feature key requested: {featureKey}");
                return false;
            }

            var appId = AppId;
            var enabledFeatures = GetEnabledFeaturesAsync(appId).GetAwaiter().GetResult();
            return enabledFeatures.Contains(featureKey);
        }

        /// <summary>
        /// Get all enabled feature keys for the current application
        /// </summary>
        public async Task<List<string>> GetEnabledFeaturesAsync()
        {
            return await GetEnabledFeaturesAsync(AppId);
        }

        /// <summary>
        /// Get all enabled feature keys for a specific application (Internal helper)
        /// </summary>
        private async Task<List<string>> GetEnabledFeaturesAsync(Guid applicationId)
        {
            var application = await _applicationRepository.GetByIdEntityAsync(applicationId);

            if (application == null)
            {
                return new List<string>();
            }

            return ParseEnabledFeatures(application.EnabledFeatures);
        }

        /// <summary>
        /// Manually toggle an optional module (AD Sync or SSO)
        /// Only possible if the license permits the feature.
        /// </summary>
        public async Task<bool> ToggleModuleAsync(string featureKey, bool enabled)
        {
            // 1. Only allow specific modules
            if (featureKey != FeatureDefinition.ModuleActiveDirectory && 
                featureKey != FeatureDefinition.ModuleSso)
            {
                _logger.LogWarning($"Restrict: Cannot toggle non-module feature: {featureKey}");
                return false;
            }

            var applicationId = AppId;
            var application = await _applicationRepository.GetByIdEntityAsync(applicationId);
            if (application == null) return false;

            // 2. Check if permitted by license
            var licensedFeatures = ParseEnabledFeatures(application.LicensedFeatures);
            if (!licensedFeatures.Contains(featureKey))
            {
                _logger.LogWarning($"Restrict: Feature {featureKey} is not permitted by the current license.");
                return false;
            }

            // 3. Update active configuration
            var currentFeatures = ParseEnabledFeatures(application.EnabledFeatures);
            bool changed = false;

            if (enabled && !currentFeatures.Contains(featureKey))
            {
                currentFeatures.Add(featureKey);
                changed = true;
            }
            else if (!enabled && currentFeatures.Contains(featureKey))
            {
                currentFeatures.Remove(featureKey);
                changed = true;
            }

            if (changed)
            {
                application.EnabledFeatures = SerializeEnabledFeatures(currentFeatures);
                await _applicationRepository.UpdateAsync(application);
            }

            return true;
        }

        /// <summary>
        /// Get feature info (name, description, enabled status) - Private helper
        /// </summary>
        private async Task<Dictionary<string, FeatureInfo>> GetFeatureInfoAsync()
        {
            var appId = AppId;
            var enabledFeatures = await GetEnabledFeaturesAsync(appId);
            var result = new Dictionary<string, FeatureInfo>();

            foreach (var featureKey in FeatureDefinition.AllFeatures)
            {
                var isCore = FeatureDefinition.IsCoreFeature(featureKey);
                result[featureKey] = new FeatureInfo
                {
                    Key = featureKey,
                    DisplayName = FeatureDefinition.GetDisplayName(featureKey),
                    Description = FeatureDefinition.GetDescription(featureKey),
                    IsCore = isCore,
                    IsEnabled = enabledFeatures.Contains(featureKey),
                    Category = isCore ? "core" : "module"
                };
            }

            return result;
        }

        /// <summary>
        /// Get categorized features (core vs module)
        /// </summary>
        public async Task<CategorizedFeaturesRead> GetCategorizedFeaturesAsync()
        {
            var features = await GetFeatureInfoAsync();
            var result = new CategorizedFeaturesRead();

            foreach (var kvp in features)
            {
                var featureData = new FeatureItemRead
                {
                    Key = kvp.Value.Key,
                    DisplayName = kvp.Value.DisplayName,
                    Description = kvp.Value.Description,
                    IsEnabled = kvp.Value.IsEnabled
                };

                if (kvp.Value.IsCore) result.Core[kvp.Key] = featureData;
                else result.Modules[kvp.Key] = featureData;
            }

            return result;
        }
        #region Private Methods

        private List<string> ParseEnabledFeatures(string? featuresJson)
        {
            if (string.IsNullOrWhiteSpace(featuresJson))
            {
                // Return default core features if none set
                return new List<string>(FeatureDefinition.CoreFeatures);
            }

            try
            {
                var features = JsonSerializer.Deserialize<List<string>>(featuresJson);
                return features ?? new List<string>(FeatureDefinition.CoreFeatures);
            }
            catch
            {
                return new List<string>(FeatureDefinition.CoreFeatures);
            }
        }

        private string SerializeEnabledFeatures(List<string> features)
        {
            return JsonSerializer.Serialize(features);
        }

        #endregion
    }
}
