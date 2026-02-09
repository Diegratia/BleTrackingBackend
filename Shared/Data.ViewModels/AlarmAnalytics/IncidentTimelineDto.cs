using System;
using System.Collections.Generic;

namespace Data.ViewModels.AlarmAnalytics
{
    /// <summary>
    /// Complete incident timeline response showing journey from trigger to resolution
    /// </summary>
    public class IncidentTimelineResponseDto
    {
        public IncidentInfoDto IncidentInfo { get; set; }
        public List<IncidentTimelineEventDto> Timeline { get; set; }
        public IncidentDurationDto Duration { get; set; }
        public IncidentInvestigationDto Investigation { get; set; }
    }

    /// <summary>
    /// Basic incident information
    /// </summary>
    public class IncidentInfoDto
    {
        public Guid AlarmTriggerId { get; set; }
        public DateTime? TriggerTime { get; set; }
        public string AlarmColor { get; set; }
        public string AlarmStatus { get; set; }
        public string ActionStatus { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsInRestrictedArea { get; set; }
        public IncidentLocationDto Location { get; set; }
        public IncidentPersonDto Person { get; set; }
        public IncidentSecurityDto Security { get; set; }
    }

    /// <summary>
    /// Location information for the incident
    /// </summary>
    public class IncidentLocationDto
    {
        public Guid? FloorplanId { get; set; }
        public string? FloorplanName { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public string? AreaName { get; set; }
        public IncidentPositionDto Position { get; set; }
    }

    /// <summary>
    /// Position coordinates
    /// </summary>
    public class IncidentPositionDto
    {
        public float? X { get; set; }
        public float? Y { get; set; }
        public string? BeaconId { get; set; }
    }

    /// <summary>
    /// Person who triggered the alarm
    /// </summary>
    public class IncidentPersonDto
    {
        public string Type { get; set; }  // "Visitor" or "Member" or "Unknown"
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? IdentityId { get; set; }
        public string? CardNumber { get; set; }
    }

    /// <summary>
    /// Security assigned to the incident
    /// </summary>
    public class IncidentSecurityDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }

    /// <summary>
    /// Single timeline event/stage
    /// </summary>
    public class IncidentTimelineEventDto
    {
        public string Stage { get; set; }  // "triggered", "notified", "waiting", "investigating", "resolved", "cancelled"
        public DateTime Timestamp { get; set; }
        public string? Actor { get; set; }
        public Guid? ActorId { get; set; }
        public double? DurationInSeconds { get; set; }
        public string? DurationFormatted { get; set; }  // "2 minutes 30 seconds"
        public string Description { get; set; }
    }

    /// <summary>
    /// Duration metrics for the incident
    /// </summary>
    public class IncidentDurationDto
    {
        public double? TotalSeconds { get; set; }
        public string TotalFormatted { get; set; }  // "5 minutes 30 seconds"
        public double? ResponseTimeSeconds { get; set; }  // Trigger to first action
        public string ResponseTimeFormatted { get; set; }
        public double? ResolutionTimeSeconds { get; set; }  // Start to finish
        public string ResolutionTimeFormatted { get; set; }
    }

    /// <summary>
    /// Investigation details
    /// </summary>
    public class IncidentInvestigationDto
    {
        public string? Result { get; set; }
        public string? DispatchedPerson { get; set; }
        public Guid? DispatchedPersonId { get; set; }
        public DateTime? InvestigatedAt { get; set; }
        public DateTime? DoneAt { get; set; }
        public string? Notes { get; set; }
        public bool? WasInvestigated { get; set; }
    }
}
