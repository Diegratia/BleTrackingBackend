using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Extension;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Repositories.Repository;
using DataView;
using Shared.Contracts.Read;
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
        private readonly MstApplicationRepository _applicationRepository;
        private readonly CardRepository _cardRepository;
        private readonly MstBleReaderRepository _readerRepository;
        private readonly IFeatureService _featureService;
        private readonly IAuditEmitter _audit;
        private readonly ILogger<LicenseService> _logger;

        // Embedded Public Key - This key is used to verify license signatures
        // Generated once by LicenseGenerator and embedded in the application
        private const string PublicKey = @"MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEmBppZFeGPf2nZHkt+DfMfVKSYSrofrM4IEjYo1lrufC2LWnPHRQFrmCM3x4nTb0WSOM1SW1lqwICc6JGyJpGmQ==";

        public LicenseService(
            MstApplicationRepository applicationRepository,
            CardRepository cardRepository,
            MstBleReaderRepository readerRepository,
            IFeatureService featureService,
            IAuditEmitter audit,
            ILogger<LicenseService> logger,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _applicationRepository = applicationRepository;
            _cardRepository = cardRepository;
            _readerRepository = readerRepository;
            _featureService = featureService;
            _audit = audit;
            _logger = logger;
        }

        /// <summary>
        /// Get current license information for the application
        /// </summary>
        public async Task<LicenseRead> GetLicenseInfoAsync()
        {
            return await GetLicenseInfoAsync(AppId);
        }

        /// <summary>
        /// Get license information for a specific application
        /// </summary>
        public async Task<LicenseRead> GetLicenseInfoAsync(Guid applicationId)
        {
            var application = await _applicationRepository.GetByIdEntityAsync(applicationId);

            if (application == null)
            {
                return new LicenseRead
                {
                    IsValid = false,
                    ValidationMessage = "Application not found"
                };
            }

            var info = new LicenseRead
            {
                LicenseType = (Shared.Contracts.LicenseType?)application.LicenseType,
                LicenseTier = (Shared.Contracts.LicenseTier?)application.LicenseTier,
                CustomerName = application.CustomerName ?? "Unknown",
                ApplicationName = application.ApplicationName,
                ApplicationCustomName = application.ApplicationCustomName,
                ApplicationCustomDomain = application.ApplicationCustomDomain,
                ApplicationRegistered = application.ApplicationRegistered,
                ExpirationDate = application.ApplicationExpired,
                DaysRemaining = (int?)(application.ApplicationExpired - DateTime.UtcNow).TotalDays,
                MaxBeacons = application.MaxBeacons,
                MaxReaders = application.MaxReaders,
                IsValid = await IsLicenseValidInternalAsync(application),
                ValidationMessage = GetValidationMessage(application)
            };

            // Get categorized features
            if (info.IsValid)
            {
                info.Features = await _featureService.GetCategorizedFeaturesAsync();
            }
            else
            {
                // Return default empty/false categorized features
                info.Features = new CategorizedFeaturesRead();
            }

            return info;
        }

        /// <summary>
        /// Validate a license file without activating it
        /// </summary>
        public async Task<(bool IsValid, string Message)> ValidateLicenseAsync(string content, bool isBase64 = false)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return (false, "License content is empty");
            }

            try
            {
                string licenseContent = isBase64 ? System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(content)) : content;
                var license = LicenseFile.Load(licenseContent);

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
                _logger.LogError(ex, "License validation error");
                return (false, $"License validation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Activate a license file for the current application
        /// </summary>
        public async Task<(bool Success, string Message)> ActivateLicenseAsync(string content, bool isBase64 = false)
        {
            var (isValid, validationMessage) = await ValidateLicenseAsync(content, isBase64);

            if (!isValid)
            {
                throw new BusinessException(validationMessage);
            }

            try
            {
                string licenseContent = isBase64 ? System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(content)) : content;
                var license = LicenseFile.Load(licenseContent);
            var application = await _applicationRepository.GetByIdEntityAsync(AppId);

            if (application == null)
            {
                throw new NotFoundException("Application not found");
            }

            // Update application with license info
            application.ApplicationExpired = license.Expiration;
            application.ApplicationStatus = 1;
            application.CustomerName = license.Customer.Name;
            application.LicenseMachineId = license.AdditionalAttributes.Get("MachineID");

            // Sync License Type & Tier
            var typeStr = license.AdditionalAttributes.Get("LicenseType");
            if (Enum.TryParse<Shared.Contracts.LicenseType>(typeStr, true, out var licenseType))
                application.LicenseType = licenseType;

            var tierStr = license.AdditionalAttributes.Get("LicenseTier");
            if (Enum.TryParse<Shared.Contracts.LicenseTier>(tierStr, true, out var licenseTier))
                application.LicenseTier = licenseTier;

            // Sync Limits
            if (int.TryParse(license.AdditionalAttributes.Get("MaxBeacons"), out int maxBeacons))
                application.MaxBeacons = maxBeacons;
            
            if (int.TryParse(license.AdditionalAttributes.Get("MaxReaders"), out int maxReaders))
                application.MaxReaders = maxReaders;

            // Sync Features (Source of Truth is now the license file)
            var featuresStr = license.AdditionalAttributes.Get("Features");
            if (!string.IsNullOrEmpty(featuresStr))
            {
                var features = featuresStr.Split(',').Select(f => f.Trim()).ToList();
                
                // Always include core features in what is permitted
                var permittedFeatures = Shared.BusinessLogic.Services.Feature.FeatureDefinition.CoreFeatures.ToList();
                permittedFeatures.AddRange(features);
                
                var finalFeaturesJson = System.Text.Json.JsonSerializer.Serialize(permittedFeatures.Distinct());
                
                application.LicensedFeatures = finalFeaturesJson;
                application.EnabledFeatures = finalFeaturesJson; // Initialize enabled with all licensed features
            }
            else
            {
                // Fallback to Tier-based defaults for old licenses
                var currentTier = application.LicenseTier?.ToString() ?? "trial";
                var defaultFeatures = Shared.BusinessLogic.Services.Feature.FeatureDefinition.DefaultFeatures.GetValueOrDefault(currentTier.ToLower(), Shared.BusinessLogic.Services.Feature.FeatureDefinition.CoreFeatures);
                var defaultJson = System.Text.Json.JsonSerializer.Serialize(defaultFeatures);
                
                application.LicensedFeatures = defaultJson;
                application.EnabledFeatures = defaultJson;
            }

            await _applicationRepository.UpdateAsync(application);

            // Audit Log
            _audit.Updated("License", AppId, "License Activated", new { 
                Customer = license.Customer.Name, 
                Tier = application.LicenseTier,
                Expires = license.Expiration 
            });

            _logger.LogInformation($"License activated for application {AppId}. Customer: {license.Customer.Name}, Expires: {license.Expiration}");

            return (true, $"License activated successfully for {license.Customer.Name}. Valid until {license.Expiration:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error activating license for application {AppId}");
                if (ex is BusinessException || ex is NotFoundException) throw;
                throw new BusinessException($"License activation failed: {ex.Message}");
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
            var application = await _applicationRepository.GetByIdEntityAsync(AppId);

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
        /// Validate if adding a new resource is allowed under the current license
        /// </summary>
        public async Task ValidateUsageAsync(string resourceType)
        {
            var application = await _applicationRepository.GetByIdEntityAsync(AppId);

            if (application == null) throw new NotFoundException("Application not found");

            if (resourceType.ToLower() == "beacon")
            {
                var cardCount = await _cardRepository.GetCountEachIdAsync(); // This check already applies AppId filter

                if (cardCount >= application.MaxBeacons)
                {
                    throw new BusinessException($"License limit reached. Maximum Beacons allowed: {application.MaxBeacons}");
                }
            }
            else if (resourceType.ToLower() == "reader")
            {
                var readerCount = await _readerRepository.GetAllQueryable().CountAsync(); // This check already applies AppId filter
                if (readerCount >= application.MaxReaders)
                {
                    throw new BusinessException($"License limit reached. Maximum Readers allowed: {application.MaxReaders}");
                }
            }
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
            if (application.LicenseTier == null)
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

        #endregion
    }
}
