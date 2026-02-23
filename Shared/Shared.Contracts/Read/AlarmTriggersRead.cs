using System;
using System.Text.Json.Serialization;

namespace Shared.Contracts.Read
{
    public class AlarmTriggersRead
    {
        [JsonPropertyOrder(-10)]
        public Guid Id { get; set; }

        // Beacon and location info
        public string? BeaconId { get; set; }
        public Guid? FloorplanId { get; set; }
        public float? PosX { get; set; }
        public float? PosY { get; set; }
        public bool? IsInRestrictedArea { get; set; }

        // Gateway info
        public string? FirstGatewayId { get; set; }
        public string? SecondGatewayId { get; set; }
        public float? FirstDistance { get; set; }
        public float? SecondDistance { get; set; }

        // Person info
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? SecurityId { get; set; }

        // Alarm info
        public DateTime? TriggerTime { get; set; }
        public string? AlarmColor { get; set; }
        public AlarmRecordStatus? Alarm { get; set; }
        public ActionStatus? Action { get; set; }
        public bool? IsActive { get; set; }

        // Timestamps
        public DateTime? IdleTimestamp { get; set; }
        public DateTime? DoneTimestamp { get; set; }
        public DateTime? CancelTimestamp { get; set; }
        public DateTime? WaitingTimestamp { get; set; }
        public DateTime? DispatchedAt { get; set; }
        public DateTime? InvestigatedDoneAt { get; set; }
        public DateTime? ActionUpdatedAt { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public DateTime? LastNotifiedAt { get; set; }

        // Actor info
        public string? IdleBy { get; set; }
        public string? DispatchedBy { get; set; }
        public string? AcceptedBy { get; set; }
        public string? DoneBy { get; set; }
        public string? CancelBy { get; set; }
        public string? WaitingBy { get; set; }
        public string? InvestigatedDoneBy { get; set; }
        public InvestigatedResult? InvestigatedResult { get; set; }
        public string? InvestigatedNotes { get; set; }

        public Guid ApplicationId { get; set; }

        // Navigation properties
        public string? FloorplanName { get; set; }
        public string? FloorName { get; set; }
        public string? FloorplanImage { get; set; }
        public Guid? BuildingId { get; set; }
        public string? BuildingName { get; set; }

        // Person navigation properties
        public string? VisitorName { get; set; }
        public string? VisitorIdentityId { get; set; }
        public string? VisitorCardNumber { get; set; }
        public string? VisitorFaceImage { get; set; }

        public string? MemberName { get; set; }
        public string? MemberIdentityId { get; set; }
        public string? MemberCardNumber { get; set; }
        public string? MemberFaceImage { get; set; }

        public string? SecurityName { get; set; }
        public string? SecurityEmail { get; set; }
    }
}
