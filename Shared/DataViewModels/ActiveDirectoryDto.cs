using System.ComponentModel.DataAnnotations;

namespace Data.ViewModels
{
    /// <summary>
    /// DTO for creating/updating Active Directory configuration
    /// </summary>
    public class ActiveDirectoryConfigCreate
    {
        [Required]
        [StringLength(255)]
        public string Server { get; set; }

        [Range(1, 65535)]
        public int Port { get; set; } = 389;

        public bool UseSsl { get; set; } = false;

        [Required]
        [StringLength(255)]
        public string Domain { get; set; }

        [StringLength(512)]
        public string ServiceAccountDn { get; set; }

        [StringLength(512)]
        public string ServiceAccountPassword { get; set; }

        [StringLength(255)]
        public string SearchBase { get; set; }

        [StringLength(100)]
        public string UserObjectFilter { get; set; } = "(objectClass=user)";

        [Range(1, 10080)]
        public int SyncIntervalMinutes { get; set; } = 60;

        public bool IsEnabled { get; set; } = false;

        public bool AutoCreateMembers { get; set; } = true;

        public Guid? DefaultDepartmentId { get; set; }
    }

    /// <summary>
    /// DTO for updating Active Directory configuration
    /// </summary>
    public class ActiveDirectoryConfigUpdate
    {
        [Required]
        [StringLength(255)]
        public string Server { get; set; }

        [Range(1, 65535)]
        public int Port { get; set; } = 389;

        public bool UseSsl { get; set; } = false;

        [Required]
        [StringLength(255)]
        public string Domain { get; set; }

        [StringLength(512)]
        public string ServiceAccountDn { get; set; }

        [StringLength(512)]
        public string ServiceAccountPassword { get; set; }

        [StringLength(255)]
        public string SearchBase { get; set; }

        [StringLength(100)]
        public string UserObjectFilter { get; set; } = "(objectClass=user)";

        [Range(1, 10080)]
        public int SyncIntervalMinutes { get; set; } = 60;

        public bool IsEnabled { get; set; } = false;

        public bool AutoCreateMembers { get; set; } = true;

        public Guid? DefaultDepartmentId { get; set; }
    }

    /// <summary>
    /// DTO for manual sync trigger
    /// </summary>
    public class AdSyncTrigger
    {
        /// <summary>
        /// Whether to force a full sync (re-sync all users)
        /// </summary>
        public bool ForceFullSync { get; set; } = false;

        /// <summary>
        /// Optional filter for specific users/groups
        /// </summary>
        public string Filter { get; set; }
    }

    /// <summary>
    /// DTO for sync result
    /// </summary>
    public class AdSyncResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int UsersSynced { get; set; }
        public int UsersCreated { get; set; }
        public int UsersUpdated { get; set; }
        public int UsersFailed { get; set; }
        public DateTime SyncStartedAt { get; set; }
        public DateTime SyncCompletedAt { get; set; }
        public double DurationSeconds { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
