using System;
using System.Text.Json.Serialization;

namespace Shared.Contracts.Read
{
    public class MstApplicationRead
    {
        [JsonPropertyOrder(-10)]
        public long Generate { get; set; }
        [JsonPropertyOrder(-9)]
        public Guid Id { get; set; }
        [JsonPropertyOrder(1)]
        public string ApplicationName { get; set; }
        [JsonPropertyOrder(2)]
        public string OrganizationType { get; set; }
        [JsonPropertyOrder(3)]
        public string OrganizationAddress { get; set; }
        [JsonPropertyOrder(4)]
        public string ApplicationType { get; set; }
        [JsonPropertyOrder(5)]
        public DateTime ApplicationRegistered { get; set; }
        [JsonPropertyOrder(6)]
        public DateTime ApplicationExpired { get; set; }
        [JsonPropertyOrder(7)]
        public string HostName { get; set; }
        [JsonPropertyOrder(8)]
        public string HostPhone { get; set; }
        [JsonPropertyOrder(9)]
        public string HostEmail { get; set; }
        [JsonPropertyOrder(10)]
        public string HostAddress { get; set; }
        [JsonPropertyOrder(11)]
        public string ApplicationCustomName { get; set; }
        [JsonPropertyOrder(12)]
        public string ApplicationCustomDomain { get; set; }
        [JsonPropertyOrder(13)]
        public string ApplicationCustomPort { get; set; }
        [JsonPropertyOrder(14)]
        public string LicenseCode { get; set; }
        [JsonPropertyOrder(15)]
        public string LicenseType { get; set; }
        [JsonPropertyOrder(16)]
        public int ApplicationStatus { get; set; }
    }
}
