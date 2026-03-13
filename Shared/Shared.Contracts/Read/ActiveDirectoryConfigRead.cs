using System;
using System.Text.Json.Serialization;

namespace Shared.Contracts.Read
{
    /// <summary>
    /// Read DTO for Active Directory configuration
    /// </summary>
    public class ActiveDirectoryConfigRead
    {
        [JsonPropertyOrder(-9)]
        public Guid Id { get; set; }

        [JsonPropertyOrder(1)]
        public Guid ApplicationId { get; set; }

        [JsonPropertyOrder(2)]
        public string Server { get; set; }

        [JsonPropertyOrder(3)]
        public int Port { get; set; }

        [JsonPropertyOrder(4)]
        public bool UseSsl { get; set; }

        [JsonPropertyOrder(5)]
        public string Domain { get; set; }

        [JsonPropertyOrder(6)]
        public string ServiceAccountDn { get; set; }

        [JsonPropertyOrder(7)]
        public string SearchBase { get; set; }

        [JsonPropertyOrder(8)]
        public string UserObjectFilter { get; set; }

        [JsonPropertyOrder(9)]
        public int SyncIntervalMinutes { get; set; }

        [JsonPropertyOrder(10)]
        public DateTime? LastSyncAt { get; set; }

        [JsonPropertyOrder(11)]
        public string LastSyncStatus { get; set; }

        [JsonPropertyOrder(12)]
        public string LastSyncMessage { get; set; }

        [JsonPropertyOrder(13)]
        public int TotalUsersSynced { get; set; }

        [JsonPropertyOrder(14)]
        public bool IsEnabled { get; set; }

        [JsonPropertyOrder(15)]
        public bool AutoCreateMembers { get; set; }

        [JsonPropertyOrder(16)]
        public Guid? DefaultDepartmentId { get; set; }

        [JsonPropertyOrder(17)]
        public string DefaultDepartmentName { get; set; }

        [JsonPropertyOrder(18)]
        public int Status { get; set; }

        [JsonPropertyOrder(19)]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyOrder(20)]
        public DateTime UpdatedAt { get; set; }
    }
}
