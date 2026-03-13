using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Extension;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Repositories.DbContexts;
using Standard.Licensing;
using Standard.Licensing.Validation;
using LicenseFile = Standard.Licensing.License;

namespace BusinessLogic.Services.Implementation
{
    /// <summary>
    /// Service for license validation and management
    /// Implements hybrid CLI + API approach for license management
    /// </summary>
    public class LicenseService : BaseService, ILicenseService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IFeatureService _featureService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<LicenseService> _logger;

        // Embedded Public Key - This key is used to verify license signatures
        // Generated once by LicenseGenerator and embedded in the application
        private const string PublicKey = @"MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEmBppZFeGPf2nZHkt+DfMfVKSYSrofrM4IEjYo1lrufC2LWnPHRQFrmCM3x4nTb0WSOM1SW1lqwICc6JGyJpGmQ==";

        private const string CacheKeyPrefix = "LicenseInfo_";

        public LicenseService(
            BleTrackingDbContext context,
            IFeatureService featureService,
            IMemoryCache cache,
            ILogger<LicenseService> logger,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _context = context;
            _featureService = featureService;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Get current license information for the application
        /// </summary>
        public async Task<LicenseInfo> GetLicenseInfoAsync()
        {
            return await GetLicenseInfoAsync(AppId);
        }

        /// <summary>
        /// Get license information for a specific application
        /// </summary>
        public async Task<LicenseInfo> GetLicenseInfoAsync(Guid applicationId)
        {
            var cacheKey = $"{CacheKeyPrefix}{applicationId}";

            if (_cache.TryGetValue(cacheKey, out LicenseInfo cachedInfo))
            {
                return cachedInfo;
            }

            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
            {
                return new LicenseInfo
                {
                    IsValid = false,
                    ValidationMessage = "Application not found"
                };
            }

            var info = new LicenseInfo
            {
                LicenseType = application.LicenseType.ToString(),
                LicenseTier = application.LicenseTier ?? application.LicenseType.ToString().ToLower(),
                CustomerName = application.CustomerName ?? "Unknown",
                ExpirationDate = application.ApplicationExpired,
                DaysRemaining = (int?)(application.ApplicationExpired - DateTime.UtcNow).Days,
                IsValid = await IsLicenseValidInternalAsync(application),
                ValidationMessage = GetValidationMessage(application)
            };

            // Get enabled features
            var enabledFeatures = await _featureService.GetEnabledFeaturesAsync(applicationId);
            info.Features = new Dictionary<string, bool>
            {
                { "core.masterData", enabledFeatures.Contains("core.masterData") },
                { "core.tracking", enabledFeatures.Contains("core.tracking") },
                { "core.monitoring", enabledFeatures.Contains("core.monitoring") },
                { "core.alarm", enabledFeatures.Contains("core.alarm") },
                { "core.patrol", enabledFeatures.Contains("core.patrol") },
                { "core.reporting", enabledFeatures.Contains("core.reporting") },
                { "saas.activeDirectory", enabledFeatures.Contains("saas.activeDirectory") },
                { "saas.sso", enabledFeatures.Contains("saas.sso") }
            };

            // Cache for 5 minutes
            _cache.Set(cacheKey, info, TimeSpan.FromMinutes(5));

            return info;
        }

        /// <summary>
        /// Validate a license file without activating it
        /// </summary>
        public async Task<(bool IsValid, string Message)> ValidateLicenseAsync(string licenseContent)
        {
            if (string.IsNullOrWhiteSpace(licenseContent))
            {
                return (false, "License content is empty");
            }

            try
            {
                var license = License.Load(licenseContent);

                // Validate signature
                var validationFailures = license.Validate()
                    .Signature(PublicKey)
                    .AssertValidLicense();

                if (validationFailures.Any())
                {
                    var messages = string.Join("; ", validationFailures.Select(f => f.Message));
                    return (false, $"License validation failed: {messages}");
                }

                // Check expiration
                if (DateTime.Now > license.Expiration)
                {
                    return (false, $"License expired on {license.Expiration:yyyy-MM-dd}");
                }

                // Verify machine ID
                var expectedMachineId = license.AdditionalAttributes.Get("MachineID");
                var currentMachineId = MachineIdHelper.GenerateMachineId();

                if (expectedMachineId != currentMachineId)
                {
                    return (false, $"License is not for this machine. Expected: {expectedMachineId}, Current: {currentMachineId}");
                }

                return (true, $"License is valid for {license.Customer.Name} until {license.Expiration:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to validate license: {ex.Message}");
            }
        }

        /// <summary>
        /// Activate a license file for the current application
        /// </summary>
        public async Task<(bool Success, string Message)> ActivateLicenseAsync(string licenseContent)
        {
            var (isValid, validationMessage) = await ValidateLicenseAsync(licenseContent);

            if (!isValid)
            {
                return (false, validationMessage);
            }

            try
            {
                var license = License.Load(licenseContent);
                var application = await _context.MstApplications
                    .FirstOrDefaultAsync(a => a.Id == AppId);

                if (application == null)
                {
                    return (false, "Application not found");
                }

                // Update application with license info
                application.ApplicationExpired = license.Expiration;
                application.ApplicationStatus = 1;
                application.CustomerName = license.Customer.Name;
                application.LicenseMachineId = license.AdditionalAttributes.Get("MachineID");

                // Determine license tier
                var licenseTier = DetermineLicenseTier(license);
                application.LicenseTier = licenseTier;

                // Set default features based on license type
                var defaultFeatures = Shared.BusinessLogic.Services.Feature.FeatureDefinition.DefaultFeatures.GetValueOrDefault(licenseTier, Shared.BusinessLogic.Services.Feature.FeatureDefinition.CoreFeatures);
                application.EnabledFeatures = System.Text.Json.JsonSerializer.Serialize(defaultFeatures);

                await _context.SaveChangesAsync();

                // Clear cache
                await RefreshCacheAsync();
                await _featureService.RefreshCacheAsync(AppId);

                _logger.LogInformation($"License activated for application {AppId}. Customer: {license.Customer.Name}, Expires: {license.Expiration}");

                return (true, $"License activated successfully for {license.Customer.Name}. Valid until {license.Expiration:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to activate license: {ex.Message}");
                return (false, $"Failed to activate license: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the current machine ID
        /// </summary>
        public string GetMachineId()
        {
            return MachineIdHelper.GenerateMachineId();
        }

        /// <summary>
        /// Check if the current license is valid (not expired)
        /// </summary>
        public async Task<bool> IsLicenseValidAsync()
        {
            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == AppId);

            if (application == null)
            {
                return false;
            }

            return await IsLicenseValidInternalAsync(application);
        }

        /// <summary>
        /// Check if a specific feature is enabled
        /// </summary>
        public bool IsFeatureEnabled(string featureKey)
        {
            return _featureService.IsFeatureEnabled(featureKey);
        }

        /// <summary>
        /// Get all enabled features
        /// </summary>
        public async Task<List<string>> GetEnabledFeaturesAsync()
        {
            return await _featureService.GetEnabledFeaturesAsync();
        }

        /// <summary>
        /// Refresh license cache
        /// </summary>
        public async Task RefreshCacheAsync()
        {
            var cacheKey = $"{CacheKeyPrefix}{AppId}";
            _cache.Remove(cacheKey);
            await Task.CompletedTask;
        }

        #region Private Methods

        private async Task<bool> IsLicenseValidInternalAsync(Entities.Models.MstApplication application)
        {
            // Check if application is active
            if (application.ApplicationStatus == 0)
            {
                return false;
            }

            // Check expiration
            if (application.ApplicationExpired < DateTime.UtcNow)
            {
                return false;
            }

            // Check if license tier is set
            if (string.IsNullOrWhiteSpace(application.LicenseTier))
            {
                // Fallback: check LicenseType enum
                if (application.LicenseType == Shared.Contracts.LicenseType.Trial)
                {
                    // Trial licenses: check if within 7 days of registration
                    var daysSinceRegistration = (DateTime.UtcNow - application.ApplicationRegistered).TotalDays;
                    return daysSinceRegistration <= 7;
                }
            }

            return true;
        }

        private string GetValidationMessage(Entities.Models.MstApplication application)
        {
            if (application.ApplicationStatus == 0)
            {
                return "Application is inactive";
            }

            if (application.ApplicationExpired < DateTime.UtcNow)
            {
                return $"License expired on {application.ApplicationExpired:yyyy-MM-dd}";
            }

            if (application.LicenseType == Shared.Contracts.LicenseType.Trial)
            {
                var daysSinceRegistration = (DateTime.UtcNow - application.ApplicationRegistered).TotalDays;
                var daysRemaining = 7 - (int)daysSinceRegistration;
                if (daysRemaining > 0)
                {
                    return $"Trial license. {daysRemaining} days remaining";
                }
                return "Trial license has expired";
            }

            return "License is valid";
        }

        private string DetermineLicenseTier(LicenseFile license)
        {
            // Determine tier based on expiration date
            var daysUntilExpiration = (license.Expiration - DateTime.Now).TotalDays;

            if (daysUntilExpiration > 365 * 10) // More than 10 years = perpetual
            {
                return "perpetual";
            }
            else if (daysUntilExpiration > 30) // More than 30 days = annual
            {
                return "annual";
            }
            else // 30 days or less = trial
            {
                return "trial";
            }
        }

        #endregion
    }
}
