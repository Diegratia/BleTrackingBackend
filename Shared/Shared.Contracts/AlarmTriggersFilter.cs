using System;
using System.Text.Json;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class AlarmTriggersFilter : BaseFilter
    {
        // ID filters with JsonElement to support both single Guid and Guid array
        public JsonElement FloorplanId { get; set; }
        public JsonElement VisitorId { get; set; }
        public JsonElement MemberId { get; set; }
        public JsonElement SecurityId { get; set; }

        // Entity-specific filters
        public string? BeaconId { get; set; }
        public int? Alarm { get; set; }
        public int? Action { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsInRestrictedArea { get; set; }
        public string? AlarmColor { get; set; }
        public DateTime? TriggerTimeFrom { get; set; }
        public DateTime? TriggerTimeTo { get; set; }
        public DateTime? ActionUpdatedAtFrom { get; set; }
        public DateTime? ActionUpdatedAtTo { get; set; }
    }
}
