using System;

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
        public string? ActionStatus { get; set; }
        public bool? IsActive { get; set; }
        public Guid ApplicationId { get; set; }
    }
    
        public class AlarmTriggersUpdateDto 
    {
        public string? ActionStatus { get; set; }
    }
}