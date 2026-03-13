using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    /// <summary>
    /// Service for Active Directory synchronization
    /// </summary>
    public interface IAdSyncService
    {
        /// <summary>
        /// Get AD configuration for the current application
        /// </summary>
        Task<ActiveDirectoryConfigRead?> GetConfigAsync();

        /// <summary>
        /// Get AD configuration by ID
        /// </summary>
        Task<ActiveDirectoryConfigRead?> GetConfigByIdAsync(Guid id);

        /// <summary>
        /// Create or update AD configuration
        /// </summary>
        Task<ActiveDirectoryConfigRead> SaveConfigAsync(ActiveDirectoryConfigCreate config);

        /// <summary>
        /// Update existing AD configuration
        /// </summary>
        Task<ActiveDirectoryConfigRead> UpdateConfigAsync(Guid id, ActiveDirectoryConfigUpdate config);

        /// <summary>
        /// Delete AD configuration
        /// </summary>
        Task DeleteConfigAsync(Guid id);

        /// <summary>
        /// Trigger manual AD sync
        /// </summary>
        Task<AdSyncResult> TriggerSyncAsync(AdSyncTrigger trigger);

        /// <summary>
        /// Get sync status
        /// </summary>
        Task<AdSyncStatus> GetSyncStatusAsync();

        /// <summary>
        /// Enable or disable AD sync
        /// </summary>
        Task ToggleSyncAsync(Guid id, bool enabled);

        /// <summary>
        /// Test AD connection
        /// </summary>
        Task<(bool Success, string Message)> TestConnectionAsync(Guid id);
    }

    /// <summary>
    /// AD Sync status DTO
    /// </summary>
    public class AdSyncStatus
    {
        public bool IsConfigured { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? LastSyncAt { get; set; }
        public string LastSyncStatus { get; set; }
        public string LastSyncMessage { get; set; }
        public int TotalUsersSynced { get; set; }
        public int SyncIntervalMinutes { get; set; }
        public DateTime? NextSyncAt { get; set; }
    }
}
