using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Extension;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Repositories.DbContexts;
using Repositories.Repository;
using Shared.BusinessLogic.Services.Feature;

namespace BusinessLogic.Services.Implementation
{
    /// <summary>
    /// Service for managing feature flags and license-based feature access
    /// </summary>
    public class FeatureService : BaseService, IFeatureService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<FeatureService> _logger;
        private readonly string CacheKeyPrefix = "Feature_";

        public FeatureService(
            BleTrackingDbContext context,
            IMemoryCache cache,
            ILogger<FeatureService> logger,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _context = context;
            _cache = cache;
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
            return IsFeatureEnabledAsync(featureKey, appId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Check if a specific feature is enabled for a specific application
        /// </summary>
        public async Task<bool> IsFeatureEnabledAsync(string featureKey, Guid applicationId)
        {
            if (!FeatureDefinition.IsValidFeature(featureKey))
            {
                return false;
            }

            var enabledFeatures = await GetEnabledFeaturesAsync(applicationId);
            return enabledFeatures.Contains(featureKey);
        }

        /// <summary>
        /// Get all enabled features for the current application
        /// </summary>
        public async Task<List<string>> GetEnabledFeaturesAsync()
        {
            return await GetEnabledFeaturesAsync(AppId);
        }

        /// <summary>
        /// Get all enabled features for a specific application
        /// </summary>
        public async Task<List<string>> GetEnabledFeaturesAsync(Guid applicationId)
        {
            var cacheKey = $"{CacheKeyPrefix}{applicationId}";

            if (_cache.TryGetValue(cacheKey, out List<string> cachedFeatures))
            {
                return cachedFeatures;
            }

            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
            {
                return new List<string>();
            }

            var features = ParseEnabledFeatures(application.EnabledFeatures);

            // Cache for 5 minutes
            _cache.Set(cacheKey, features, TimeSpan.FromMinutes(5));

            return features;
        }

        /// <summary>
        /// Enable a feature for an application
        /// </summary>
        public async Task EnableFeatureAsync(Guid applicationId, string featureKey)
        {
            if (!FeatureDefinition.IsValidFeature(featureKey))
            {
                throw new ArgumentException($"Invalid feature key: {featureKey}");
            }

            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
            {
                throw new ArgumentException($"Application not found: {applicationId}");
            }

            var currentFeatures = ParseEnabledFeatures(application.EnabledFeatures);

            if (!currentFeatures.Contains(featureKey))
            {
                currentFeatures.Add(featureKey);
                application.EnabledFeatures = SerializeEnabledFeatures(currentFeatures);
                await _context.SaveChangesAsync();

                // Clear cache
                RefreshCacheAsync(applicationId).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Disable a feature for an application
        /// </summary>
        public async Task DisableFeatureAsync(Guid applicationId, string featureKey)
        {
            if (!FeatureDefinition.IsValidFeature(featureKey))
            {
                throw new ArgumentException($"Invalid feature key: {featureKey}");
            }

            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
            {
                throw new ArgumentException($"Application not found: {applicationId}");
            }

            var currentFeatures = ParseEnabledFeatures(application.EnabledFeatures);

            if (currentFeatures.Contains(featureKey))
            {
                // Don't allow disabling all core features - must have at least one
                if (FeatureDefinition.IsCoreFeature(featureKey) && currentFeatures.Count(f => FeatureDefinition.IsCoreFeature(f)) <= 1)
                {
                    throw new InvalidOperationException("Cannot disable the last core feature");
                }

                currentFeatures.Remove(featureKey);
                application.EnabledFeatures = SerializeEnabledFeatures(currentFeatures);
                await _context.SaveChangesAsync();

                // Clear cache
                RefreshCacheAsync(applicationId).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Set enabled features for an application (replaces all)
        /// </summary>
        public async Task SetEnabledFeaturesAsync(Guid applicationId, List<string> featureKeys)
        {
            // Validate all feature keys
            foreach (var key in featureKeys)
            {
                if (!FeatureDefinition.IsValidFeature(key))
                {
                    throw new ArgumentException($"Invalid feature key: {key}");
                }
            }

            // Ensure at least one core feature
            var hasCoreFeature = featureKeys.Any(f => FeatureDefinition.IsCoreFeature(f));
            if (!hasCoreFeature)
            {
                throw new ArgumentException("At least one core feature must be enabled");
            }

            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
            {
                throw new ArgumentException($"Application not found: {applicationId}");
            }

            application.EnabledFeatures = SerializeEnabledFeatures(featureKeys);
            await _context.SaveChangesAsync();

            // Clear cache
            RefreshCacheAsync(applicationId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Refresh feature cache for an application
        /// </summary>
        public Task RefreshCacheAsync(Guid applicationId)
        {
            var cacheKey = $"{CacheKeyPrefix}{applicationId}";
            _cache.Remove(cacheKey);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Get feature info (name, description, enabled status)
        /// </summary>
        public async Task<Dictionary<string, FeatureInfo>> GetFeatureInfoAsync()
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
                    Category = isCore ? "core" : "saas"
                };
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
