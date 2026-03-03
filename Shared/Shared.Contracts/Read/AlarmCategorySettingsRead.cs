using System;
using System.Text.Json.Serialization;

namespace Shared.Contracts.Read
{
    public class AlarmCategorySettingsRead
    {
        [JsonPropertyOrder(-10)]
        public Guid Id { get; set; }
        [JsonPropertyOrder(1)]
        public string? AlarmCategory { get; set; }
        [JsonPropertyOrder(2)]
        public string? Remarks { get; set; }
        [JsonPropertyOrder(3)]
        public string? AlarmColor { get; set; }
        [JsonPropertyOrder(4)]
        public string? AlarmLevelPriority { get; set; }
        [JsonPropertyOrder(5)]
        public int? NotifyIntervalSec { get; set; }
        [JsonPropertyOrder(6)]
        public int? IsEnabled { get; set; }
    }
}
