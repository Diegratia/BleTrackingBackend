using System;
using System.Text.Json.Serialization;

namespace Data.ViewModels
{
    public class AlarmTriggersDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public string? BeaconId { get; set; }
        public Guid? FloorplanId { get; set; }
        public float? PosX { get; set; }
        public float? PosY { get; set; }
        public bool? IsInRestrictedArea { get; set; }
        public string FirstGatewayId { get; set; }
        public string SecondGatewayId { get; set; }
        public DateTime? TriggerTime { get; set; }
        public string? AlarmRecordStatus { get; set; }
        public string? AlarmColor { get; set; }
        public string? ActionStatus { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? IdleTimestamp { get; set; }
        public DateTime? DoneTimestamp { get; set; }
        public DateTime? CancelTimestamp { get; set; }
        public DateTime? WaitingTimestamp { get; set; }
        public DateTime? InvestigatedTimestamp { get; set; }
        public DateTime? InvestigatedDoneAt { get; set; }
        public string? IdleBy { get; set; }
        public string? DoneBy { get; set; }
        public string? CancelBy { get; set; }
        public string? WaitingBy { get; set; }
        public string? InvestigatedBy { get; set; }
        public string? InvestigatedResult { get; set; }
        public Guid ApplicationId { get; set; }
    }
    
    public class AlarmTriggersOpenDto : BaseModelDto
    {
        [JsonPropertyName("alarm_trigger_id")]
        public Guid Id { get; set; }
        public string? BeaconId { get; set; }
        public Guid? FloorplanId { get; set; }
        public float? PosX { get; set; }
        public float? PosY { get; set; }
        public bool? IsInRestrictedArea { get; set; }
        public string FirstGatewayId { get; set; }
        public string SecondGatewayId { get; set; }
        public DateTime? TriggerTime { get; set; }
        public string? AlarmRecordStatus { get; set; }
        public string? AlarmColor { get; set; }
        public string? ActionStatus { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? IdleTimestamp { get; set; }
        public DateTime? DoneTimestamp { get; set; }
        public DateTime? CancelTimestamp { get; set; }
        public DateTime? WaitingTimestamp { get; set; }
        public DateTime? InvestigatedTimestamp { get; set; }
        public DateTime? InvestigatedDoneAt { get; set; }
        public string? IdleBy { get; set; }
        public string? DoneBy { get; set; }
        public string? CancelBy { get; set; }
        public string? WaitingBy { get; set; }
        public string? InvestigatedBy { get; set; }
        public string? InvestigatedResult { get; set; }
        public Guid ApplicationId { get; set; }
    }
    
        public class AlarmTriggersUpdateDto
    {
        public string? ActionStatus { get; set; }
    }
}