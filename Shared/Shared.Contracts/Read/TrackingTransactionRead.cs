using System;
using System.Text.Json.Serialization;

namespace Shared.Contracts.Read
{
    public class TrackingTransactionRead
    {
        [JsonPropertyOrder(-10)]
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public DateTime? TransTime { get; set; }
        public Guid? ReaderId { get; set; }
        public string? ReaderName { get; set; }
        public Guid? CardId { get; set; }
        public Guid? VisitorId { get; set; }
        public string? VisitorName { get; set; }
        public Guid? MemberId { get; set; }
        public string? MemberName { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public string? FloorplanMaskedAreaName { get; set; }
        public string? AreaShape { get; set; }
        public float? CoordinateX { get; set; }
        public float? CoordinateY { get; set; }
        public float? CoordinatePxX { get; set; }
        public float? CoordinatePxY { get; set; }
        public string? AlarmStatus { get; set; }
        public long? Battery { get; set; }
    }
}
