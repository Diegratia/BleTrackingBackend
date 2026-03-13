using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Entities.Models;
using Helpers.Consumer;
using Novell.Directory.Ldap;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repositories.DbContexts;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    /// <summary>
    /// Service for Active Directory synchronization
    /// Implements user provisioning from Active Directory to MstMember
    /// </summary>
    public class AdSyncService : BaseService, IAdSyncService
    {
        private readonly BleTrackingDbContext _context;
        private readonly ActiveDirectoryConfigRepository _adConfigRepository;
        private readonly MstMemberRepository _memberRepository;
        private readonly MstDepartmentRepository _departmentRepository;
        private readonly IFeatureService _featureService;
        private readonly IAuditEmitter _audit;
        private readonly ILogger<AdSyncService> _logger;

        public AdSyncService(
            BleTrackingDbContext context,
            ActiveDirectoryConfigRepository adConfigRepository,
            MstMemberRepository memberRepository,
            MstDepartmentRepository departmentRepository,
            IFeatureService featureService,
            IAuditEmitter audit,
            ILogger<AdSyncService> logger,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _context = context;
            _adConfigRepository = adConfigRepository;
            _memberRepository = memberRepository;
            _departmentRepository = departmentRepository;
            _featureService = featureService;
            _audit = audit;
            _logger = logger;
        }

        /// <summary>
        /// Get AD configuration for the current application
        /// </summary>
        public async Task<ActiveDirectoryConfigRead?> GetConfigAsync()
        {
            var config = await _adConfigRepository.GetByApplicationIdAsync(AppId);
            return config;
        }

        /// <summary>
        /// Get AD configuration by ID
        /// </summary>
        public async Task<ActiveDirectoryConfigRead?> GetConfigByIdAsync(Guid id)
        {
            return await _adConfigRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Create or update AD configuration
        /// </summary>
        public async Task<ActiveDirectoryConfigRead> SaveConfigAsync(ActiveDirectoryConfigCreate config)
        {
            // Check if AD sync feature is enabled
            if (!await _featureService.IsFeatureEnabledAsync(Shared.BusinessLogic.Services.Feature.FeatureDefinition.SaasActiveDirectory, AppId))
            {
                throw new UnauthorizedAccessException("Active Directory module is not enabled");
            }

            // Check if config already exists for this application
            var existingConfig = await _adConfigRepository.GetByApplicationIdAsync(AppId);

            ActiveDirectoryConfig entity;

            if (existingConfig != null)
            {
                // Update existing
                entity = await _adConfigRepository.GetByIdEntityAsync(existingConfig.Id);
                if (entity == null)
                {
                    throw new ArgumentException("Configuration not found");
                }

                entity.Server = config.Server;
                entity.Port = config.Port;
                entity.UseSsl = config.UseSsl;
                entity.Domain = config.Domain;
                entity.ServiceAccountDn = config.ServiceAccountDn;
                entity.ServiceAccountPassword = EncryptPassword(config.ServiceAccountPassword);
                entity.SearchBase = config.SearchBase;
                entity.UserObjectFilter = config.UserObjectFilter;
                entity.SyncIntervalMinutes = config.SyncIntervalMinutes;
                entity.IsEnabled = config.IsEnabled;
                entity.AutoCreateMembers = config.AutoCreateMembers;
                entity.DefaultDepartmentId = config.DefaultDepartmentId;

                SetUpdateAudit(entity);
                await _adConfigRepository.UpdateAsync(entity);

                _audit.Updated("ActiveDirectoryConfig", entity.Id, "AD configuration updated");
            }
            else
            {
                // Create new
                entity = new ActiveDirectoryConfig
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = AppId,
                    Server = config.Server,
                    Port = config.Port,
                    UseSsl = config.UseSsl,
                    Domain = config.Domain,
                    ServiceAccountDn = config.ServiceAccountDn,
                    ServiceAccountPassword = EncryptPassword(config.ServiceAccountPassword),
                    SearchBase = config.SearchBase,
                    UserObjectFilter = config.UserObjectFilter,
                    SyncIntervalMinutes = config.SyncIntervalMinutes,
                    IsEnabled = config.IsEnabled,
                    AutoCreateMembers = config.AutoCreateMembers,
                    DefaultDepartmentId = config.DefaultDepartmentId,
                    TotalUsersSynced = 0
                };

                SetCreateAudit(entity);
                entity = await _adConfigRepository.AddAsync(entity);

                _audit.Created("ActiveDirectoryConfig", entity.Id, "AD configuration created");
            }

            return await _adConfigRepository.GetByIdAsync(entity.Id);
        }

        /// <summary>
        /// Update existing AD configuration
        /// </summary>
        public async Task<ActiveDirectoryConfigRead> UpdateConfigAsync(Guid id, ActiveDirectoryConfigUpdate config)
        {
            var entity = await _adConfigRepository.GetByIdEntityAsync(id);
            if (entity == null)
            {
                throw new ArgumentException("Configuration not found");
            }

            // Verify ownership
            var (appId, isSystemAdmin) = _adConfigRepository.GetApplicationIdAndRole();
            if (!isSystemAdmin && entity.ApplicationId != appId)
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            entity.Server = config.Server;
            entity.Port = config.Port;
            entity.UseSsl = config.UseSsl;
            entity.Domain = config.Domain;
            entity.ServiceAccountDn = config.ServiceAccountDn;
            if (!string.IsNullOrEmpty(config.ServiceAccountPassword))
            {
                entity.ServiceAccountPassword = EncryptPassword(config.ServiceAccountPassword);
            }
            entity.SearchBase = config.SearchBase;
            entity.UserObjectFilter = config.UserObjectFilter;
            entity.SyncIntervalMinutes = config.SyncIntervalMinutes;
            entity.IsEnabled = config.IsEnabled;
            entity.AutoCreateMembers = config.AutoCreateMembers;
            entity.DefaultDepartmentId = config.DefaultDepartmentId;

            SetUpdateAudit(entity);
            await _adConfigRepository.UpdateAsync(entity);

            _audit.Updated("ActiveDirectoryConfig", entity.Id, "AD configuration updated");

            return await _adConfigRepository.GetByIdAsync(entity.Id);
        }

        /// <summary>
        /// Delete AD configuration
        /// </summary>
        public async Task DeleteConfigAsync(Guid id)
        {
            var entity = await _adConfigRepository.GetByIdEntityAsync(id);
            if (entity == null)
            {
                throw new ArgumentException("Configuration not found");
            }

            // Verify ownership
            var (appId, isSystemAdmin) = _adConfigRepository.GetApplicationIdAndRole();
            if (!isSystemAdmin && entity.ApplicationId != appId)
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            SetDeleteAudit(entity);
            await _adConfigRepository.DeleteAsync(entity);

            _audit.Deleted("ActiveDirectoryConfig", entity.Id, "AD configuration deleted");
        }

        /// <summary>
        /// Trigger manual AD sync
        /// </summary>
        public async Task<AdSyncResult> TriggerSyncAsync(AdSyncTrigger trigger)
        {
            // Check if AD sync feature is enabled
            if (!await _featureService.IsFeatureEnabledAsync(Shared.BusinessLogic.Services.Feature.FeatureDefinition.SaasActiveDirectory, AppId))
            {
                return new AdSyncResult
                {
                    Success = false,
                    Message = "Active Directory module is not enabled"
                };
            }

            var config = await _adConfigRepository.GetByApplicationIdAsync(AppId);
            if (config == null)
            {
                return new AdSyncResult
                {
                    Success = false,
                    Message = "Active Directory is not configured"
                };
            }

            if (!config.IsEnabled)
            {
                return new AdSyncResult
                {
                    Success = false,
                    Message = "Active Directory sync is disabled"
                };
            }

            var result = new AdSyncResult
            {
                SyncStartedAt = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation($"Starting AD sync for application {AppId}");

                // Fetch users from Active Directory
                var adUsers = await GetAdUsersAsync(config);

                // Get existing members to determine create/update
                var existingMembers = await _context.MstMembers
                    .Where(m => m.ApplicationId == AppId && m.Status != 0)
                    .Select(m => new { m.Id, m.IdentityId })
                    .ToDictionaryAsync(m => m.IdentityId, m => m.Id);

                var usersCreated = 0;
                var usersUpdated = 0;
                var usersFailed = 0;

                foreach (var adUser in adUsers)
                {
                    try
                    {
                        // Skip if no valid identity
                        if (string.IsNullOrWhiteSpace(adUser.IdentityId))
                            continue;

                        // Check if member exists
                        if (existingMembers.ContainsKey(adUser.IdentityId))
                        {
                            // Update existing member
                            var memberId = existingMembers[adUser.IdentityId];
                            var member = await _context.MstMembers.FindAsync(memberId);

                            if (member != null)
                            {
                                member.Name = adUser.Name;
                                member.Email = adUser.Email ?? member.Email;
                                member.Phone = adUser.Phone ?? member.Phone;
                                member.Address = adUser.Address ?? member.Address;
                                member.Gender = adUser.Gender ?? member.Gender;
                                member.StatusEmployee = adUser.StatusEmployee;
                                member.UpdatedAt = DateTime.UtcNow;
                                member.UpdatedBy = UsernameFormToken;

                                usersUpdated++;
                            }
                        }
                        else
                        {
                            // Create new member
                            var newMember = new MstMember
                            {
                                Id = Guid.NewGuid(),
                                ApplicationId = AppId,
                                OrganizationId = AppId, // Use current application as organization
                                DepartmentId = config.DefaultDepartmentId ?? Guid.Empty,
                                DistrictId = GetDefaultDistrictId(),
                                PersonId = Guid.NewGuid().ToString(),
                                IdentityId = adUser.IdentityId,
                                Name = adUser.Name,
                                Email = adUser.Email,
                                Phone = adUser.Phone,
                                Gender = adUser.Gender,
                                StatusEmployee = adUser.StatusEmployee,
                                Address = adUser.Address,
                                CardNumber = adUser.CardNumber,
                                BirthDate = adUser.BirthDate,
                                JoinDate = adUser.JoinDate,
                                Status = 1,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                CreatedBy = UsernameFormToken,
                                UpdatedBy = UsernameFormToken
                            };

                            _context.MstMembers.Add(newMember);
                            usersCreated++;
                        }
                    }
                    catch (Exception ex)
                    {
                        usersFailed++;
                        result.Errors.Add($"Failed to sync user {adUser.IdentityId}: {ex.Message}");
                        _logger.LogWarning(ex, $"Failed to sync AD user {adUser.IdentityId}");
                    }
                }

                // Save all changes
                await _context.SaveChangesAsync();

                // Audit the sync operation
                _audit.Created("AdSync", AppId, $"AD Sync completed: {usersCreated} created, {usersUpdated} updated, {usersFailed} failed");

                result.Success = usersFailed == 0 || adUsers.Count > usersFailed;
                result.Message = result.Success
                    ? $"Sync completed successfully"
                    : $"Sync completed with {usersFailed} errors";
                result.UsersSynced = adUsers.Count;
                result.UsersCreated = usersCreated;
                result.UsersUpdated = usersUpdated;
                result.UsersFailed = usersFailed;
                result.SyncCompletedAt = DateTime.UtcNow;
                result.DurationSeconds = (result.SyncCompletedAt - result.SyncStartedAt).TotalSeconds;

                // Update sync status in database
                await _adConfigRepository.UpdateSyncStatusAsync(
                    config.Id,
                    result.SyncCompletedAt,
                    result.Success ? "success" : "partial_success",
                    result.Message,
                    result.UsersSynced
                );

                _logger.LogInformation($"AD sync completed for application {AppId}: {result.Message}");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Sync failed: {ex.Message}";
                result.SyncCompletedAt = DateTime.UtcNow;
                result.DurationSeconds = (result.SyncCompletedAt - result.SyncStartedAt).TotalSeconds;
                result.Errors.Add(ex.Message);

                // Update sync status in database
                await _adConfigRepository.UpdateSyncStatusAsync(
                    config.Id,
                    result.SyncCompletedAt,
                    "failed",
                    result.Message,
                    0
                );

                _logger.LogError(ex, $"AD sync failed for application {AppId}");
            }

            return result;
        }

        /// <summary>
        /// Get sync status
        /// </summary>
        public async Task<AdSyncStatus> GetSyncStatusAsync()
        {
            var config = await _adConfigRepository.GetByApplicationIdAsync(AppId);

            if (config == null)
            {
                return new AdSyncStatus
                {
                    IsConfigured = false,
                    IsEnabled = false
                };
            }

            return new AdSyncStatus
            {
                IsConfigured = true,
                IsEnabled = config.IsEnabled,
                LastSyncAt = config.LastSyncAt,
                LastSyncStatus = config.LastSyncStatus ?? "Never synced",
                LastSyncMessage = config.LastSyncMessage,
                TotalUsersSynced = config.TotalUsersSynced,
                SyncIntervalMinutes = config.SyncIntervalMinutes,
                NextSyncAt = config.LastSyncAt?.AddMinutes(config.SyncIntervalMinutes)
            };
        }

        /// <summary>
        /// Enable or disable AD sync
        /// </summary>
        public async Task ToggleSyncAsync(Guid id, bool enabled)
        {
            var entity = await _adConfigRepository.GetByIdEntityAsync(id);
            if (entity == null)
            {
                throw new ArgumentException("Configuration not found");
            }

            // Verify ownership
            var (appId, isSystemAdmin) = _adConfigRepository.GetApplicationIdAndRole();
            if (!isSystemAdmin && entity.ApplicationId != appId)
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            entity.IsEnabled = enabled;
            SetUpdateAudit(entity);
            await _adConfigRepository.UpdateAsync(entity);

            _audit.Updated("ActiveDirectoryConfig", entity.Id, $"AD sync {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Test AD connection
        /// </summary>
        public async Task<(bool Success, string Message)> TestConnectionAsync(Guid id)
        {
            var config = await _adConfigRepository.GetByIdAsync(id);
            if (config == null)
            {
                return (false, "Configuration not found");
            }

            // Verify ownership
            var (appId, isSystemAdmin) = _adConfigRepository.GetApplicationIdAndRole();
            if (!isSystemAdmin && config.ApplicationId != appId)
            {
                return (false, "Access denied");
            }

            try
            {
                // Create LDAP connection options
                var options = new LdapConnectionOptions
                {
                    ServerName = config.Server,
                    ServerPort = config.Port,
                    UseSsl = config.UseSsl,
                    AutoReconnect = false,
                    ConnectionTimeout = TimeSpan.FromSeconds(10)
                };

                using var connection = new LdapConnection(options);

                // Try to connect
                await connection.ConnectAsync();

                // Try to bind
                await connection.BindAsync(
                    new LdapCredential(
                        config.ServiceAccountDn,
                        DecryptPassword(config.ServiceAccountPassword)
                    )
                );

                await connection.DisconnectAsync();

                return (true, $"Connection successful. Server: {config.Server}, Port: {config.Port}, SSL: {config.UseSsl}");
            }
            catch (LdapException ldapEx)
            {
                return (false, $"LDAP connection failed: {ldapEx.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Connection test failed: {ex.Message}");
            }
        }

        #region Private Methods

        /// <summary>
        /// Encrypt password for storage (placeholder - use proper encryption in production)
        /// </summary>
        private string EncryptPassword(string plainPassword)
        {
            if (string.IsNullOrEmpty(plainPassword))
                return plainPassword;

            // TODO: Implement proper encryption
            // For now, just base64 encode as placeholder
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainPassword));
        }

        /// <summary>
        /// Decrypt password from storage (placeholder)
        /// </summary>
        private string DecryptPassword(string encryptedPassword)
        {
            if (string.IsNullOrEmpty(encryptedPassword))
                return encryptedPassword;

            // TODO: Implement proper decryption
            // For now, just base64 decode as placeholder
            try
            {
                return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encryptedPassword));
            }
            catch
            {
                return encryptedPassword;
            }
        }

        /// <summary>
        /// Get users from Active Directory using LDAP
        /// </summary>
        private async Task<List<AdUser>> GetAdUsersAsync(ActiveDirectoryConfig config)
        {
            var users = new List<AdUser>();

            try
            {
                // Create LDAP connection options
                var options = new LdapConnectionOptions
                {
                    ServerName = config.Server,
                    ServerPort = config.Port,
                    UseSsl = config.UseSsl,
                    AutoReconnect = false,
                    ConnectionTimeout = TimeSpan.FromSeconds(30)
                };

                using var connection = new LdapConnection(options);

                _logger.LogInformation($"Connecting to LDAP server {config.Server}:{config.Port}");

                // Connect to LDAP server
                await connection.ConnectAsync();

                // Bind with service account
                await connection.BindAsync(
                    new LdapCredential(
                        config.ServiceAccountDn,
                        DecryptPassword(config.ServiceAccountPassword)
                    )
                );

                _logger.LogInformation($"LDAP bind successful for {config.Domain}");

                // Build search filter
                var searchFilter = string.IsNullOrWhiteSpace(config.UserObjectFilter)
                    ? "(objectClass=user)"
                    : config.UserObjectFilter;

                // Apply additional filter if provided
                if (!string.IsNullOrEmpty(config.Domain) && !searchFilter.Contains("(sAMAccountName="))
                {
                    // Add domain\* filter if not present
                    if (!searchFilter.Contains("sAMAccountName"))
                    {
                        searchFilter = $"(&{searchFilter}(sAMAccountName=*{config.Domain}\\*))";
                    }
                }

                _logger.LogInformation($"Searching users with filter: {searchFilter}");

                // Search for users
                var searchOptions = new SearchOptions
                {
                    Filter = searchFilter,
                    Scope = SearchScope.Subtree,
                    SizeLimit = 0 // No limit
                };

                // Attributes to retrieve
                var attributes = new[]
                {
                    "distinguishedName",
                    "sAMAccountName",
                    "userPrincipalName",
                    "employeeID",
                    "givenName",
                    "sn",
                    "displayName",
                    "mail",
                    "mobile",
                    "telephoneNumber",
                    "streetAddress",
                    "l",
                    "st",
                    "postalCode",
                    "co",
                    "title",
                    "department",
                    "company",
                    "objectClass"
                };

                var searchResponse = await connection.SearchAsync(
                    new LdapDirectoryIdentifier(config.SearchBase),
                    searchOptions,
                    attributes
                );

                _logger.LogInformation($"Found {searchResponse.Entries.Count} users in AD");

                foreach (var entry in searchResponse.Entries)
                {
                    try
                    {
                        var adUser = new AdUser
                        {
                            DistinguishedName = entry.Dn,
                            PersonId = GetAttributeValue(entry, "sAMAccountName"),
                            IdentityId = GetAttributeValue(entry, "employeeID", "sAMAccountName"),
                            Name = GetAttributeValue(entry, "displayName", $"{GetAttributeValue(entry, "givenName", "")} {GetAttributeValue(entry, "sn", "")}"),
                            Email = GetAttributeValue(entry, "mail"),
                            Phone = GetAttributeValue(entry, "mobile", "telephoneNumber"),
                            Address = BuildAddress(entry),
                            Gender = "", // AD doesn't typically store gender
                            StatusEmployee = "active",
                            CardNumber = "",
                            BirthDate = "",
                            JoinDate = ""
                        };

                        // Skip if no valid identifier
                        if (string.IsNullOrWhiteSpace(adUser.IdentityId))
                        {
                            _logger.LogWarning($"Skipping user {adUser.DistinguishedName} - no valid ID");
                            continue;
                        }

                        users.Add(adUser);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to parse AD entry {entry.Dn}");
                    }
                }

                await connection.DisconnectAsync();
                _logger.LogInformation($"LDAP connection closed. Retrieved {users.Count} valid users");
            }
            catch (LdapException ldapEx)
            {
                _logger.LogError(ldapEx, $"LDAP error: {ldapEx.Message}");
                throw new Exception($"LDAP connection failed: {ldapEx.Message}", ldapEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve AD users: {ex.Message}");
                throw;
            }

            return users;
        }

        /// <summary>
        /// Get attribute value from LDAP entry
        /// </summary>
        private string GetAttributeValue(SearchResultEntry entry, string attributeName, string fallback = null)
        {
            if (entry.Attributes.TryGetValue(attributeName, out var values) && values.Count > 0)
            {
                return values[0]?.ToString() ?? fallback;
            }
            return fallback;
        }

        /// <summary>
        /// Build address string from LDAP attributes
        /// </summary>
        private string BuildAddress(SearchResultEntry entry)
        {
            var parts = new List<string>();

            var street = GetAttributeValue(entry, "streetAddress");
            var city = GetAttributeValue(entry, "l"); // locality
            var state = GetAttributeValue(entry, "st"); // state/province
            var postalCode = GetAttributeValue(entry, "postalCode");
            var country = GetAttributeValue(entry, "co"); // country

            if (!string.IsNullOrWhiteSpace(street))
                parts.Add(street);
            if (!string.IsNullOrWhiteSpace(city))
                parts.Add(city);
            if (!string.IsNullOrWhiteSpace(state))
                parts.Add(state);
            if (!string.IsNullOrWhiteSpace(postalCode))
                parts.Add(postalCode);
            if (!string.IsNullOrWhiteSpace(country))
                parts.Add(country);

            return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        /// <summary>
        /// Get default district ID for synced users
        /// </summary>
        private Guid GetDefaultDistrictId()
        {
            // Try to get first district for the application
            return Task.Run(async () =>
            {
                var district = await _context.MstDistricts
                    .FirstOrDefaultAsync(d => d.ApplicationId == AppId && d.Status != 0);
                return district?.Id ?? Guid.Empty;
            }).GetAwaiter().GetResult();
        }

        #endregion

        /// <summary>
        /// AD User DTO for sync operation
        /// </summary>
        private class AdUser
        {
            public string DistinguishedName { get; set; }
            public string PersonId { get; set; }
            public string IdentityId { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
            public string Gender { get; set; }
            public string StatusEmployee { get; set; }
            public string CardNumber { get; set; }
            public string BirthDate { get; set; }
            public string JoinDate { get; set; }
        }
    }
}
