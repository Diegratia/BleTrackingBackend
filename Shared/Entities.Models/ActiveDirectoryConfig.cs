using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    /// <summary>
    /// Configuration for Active Directory sync functionality
    /// Part of SaaS module: Active Directory Sync
    /// </summary>
    public class ActiveDirectoryConfig : BaseModelWithTimeApp, IApplicationEntity
    {
        [Required]
        [StringLength(255)]
        [Column("server")]
        public string Server { get; set; }

        [Required]
        [Column("port")]
        public int Port { get; set; } = 389;

        [Column("use_ssl")]
        public bool UseSsl { get; set; } = false;

        [Required]
        [StringLength(255)]
        [Column("domain")]
        public string Domain { get; set; }

        [StringLength(512)]
        [Column("service_account_dn")]
        public string ServiceAccountDn { get; set; }

        [StringLength(512)]
        [Column("service_account_password")]
        public string ServiceAccountPassword { get; set; }

        [StringLength(255)]
        [Column("search_base")]
        public string SearchBase { get; set; }

        [StringLength(100)]
        [Column("user_object_filter")]
        public string UserObjectFilter { get; set; } = "(objectClass=user)";

        [Column("sync_interval_minutes")]
        public int SyncIntervalMinutes { get; set; } = 60;

        [Column("last_sync_at")]
        public DateTime? LastSyncAt { get; set; }

        [Column("last_sync_status")]
        public string LastSyncStatus { get; set; }

        [Column("last_sync_message")]
        public string LastSyncMessage { get; set; }

        [Column("total_users_synced")]
        public int TotalUsersSynced { get; set; } = 0;

        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = false;

        [Column("auto_create_members")]
        public bool AutoCreateMembers { get; set; } = true;

        [StringLength(255)]
        [Column("default_department_id")]
        public Guid? DefaultDepartmentId { get; set; }

        // Navigation properties
        [ForeignKey("ApplicationId")]
        public MstApplication Application { get; set; }

        public MstDepartment DefaultDepartment { get; set; }
    }
}
