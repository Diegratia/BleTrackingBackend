using System;
using System.Collections.Generic;

namespace Shared.Contracts.Analytics
{
    /// <summary>
    /// Filter for User Journey queries
    /// </summary>
    public class UserJourneyFilter
    {
        /// <summary>Start date for journey analysis</summary>
        public DateTime? From { get; set; }

        /// <summary>End date for journey analysis</summary>
        public DateTime? To { get; set; }

        /// <summary>Filter by specific building</summary>
        public Guid? BuildingId { get; set; }

        /// <summary>Filter by specific floor</summary>
        public Guid? FloorId { get; set; }

        /// <summary>Filter by specific area</summary>
        public Guid? AreaId { get; set; }

        /// <summary>Filter by specific visitor</summary>
        public Guid? VisitorId { get; set; }

        /// <summary>Filter by specific member</summary>
        public Guid? MemberId { get; set; }

        /// <summary>Minimum journey length in areas (default: 2)</summary>
        public int MinJourneyLength { get; set; } = 2;

        /// <summary>Maximum number of results to return (default: 20)</summary>
        public int MaxResults { get; set; } = 20;
    }

    /// <summary>
    /// Common path analysis result
    /// </summary>
    public class CommonPathRead
    {
        /// <summary>Unique path identifier (lowercase, hyphenated)</summary>
        public string PathId { get; set; } = string.Empty;

        /// <summary>Sequence of area names in order of visit</summary>
        public List<string> AreaSequence { get; set; } = new();

        /// <summary>Number of journeys matching this path</summary>
        public int JourneyCount { get; set; }

        /// <summary>Percentage of total journeys (0-100)</summary>
        public double Percentage { get; set; }

        /// <summary>Average duration in minutes for this path</summary>
        public double AvgDurationMinutes { get; set; }

        /// <summary>Whether this path is considered anomalous (low occurrence)</summary>
        public bool IsAnomaly { get; set; }

        /// <summary>Risk level: Low, Medium, High</summary>
        public string RiskLevel { get; set; } = "Low";
    }

    /// <summary>
    /// Journey replay with incident markers
    /// </summary>
    public class JourneyReplayRead
    {
        /// <summary>Visitor ID</summary>
        public Guid? VisitorId { get; set; }

        /// <summary>Visitor name</summary>
        public string? VisitorName { get; set; }

        /// <summary>Visitor identity ID</summary>
        public string? VisitorIdentityId { get; set; }

        /// <summary>Member ID (if applicable)</summary>
        public Guid? MemberId { get; set; }

        /// <summary>Member name (if applicable)</summary>
        public string? MemberName { get; set; }

        /// <summary>Member identity ID (if applicable)</summary>
        public string? MemberIdentityId { get; set; }

        /// <summary>Journey steps with incident markers</summary>
        public List<JourneyStepRead> JourneySteps { get; set; } = new();

        /// <summary>Total number of incidents during journey</summary>
        public int TotalIncidents { get; set; }

        /// <summary>Total journey duration in minutes</summary>
        public int? TotalDurationMinutes { get; set; }

        /// <summary>Date range of journey</summary>
        public string DateRange { get; set; } = string.Empty;
    }

    /// <summary>
    /// Single journey step with incident information
    /// </summary>
    public class JourneyStepRead
    {
        /// <summary>Area ID</summary>
        public Guid? AreaId { get; set; }

        /// <summary>Area name</summary>
        public string? AreaName { get; set; }

        /// <summary>Building name</summary>
        public string? BuildingName { get; set; }

        /// <summary>Floor name</summary>
        public string? FloorName { get; set; }

        /// <summary>Enter time</summary>
        public DateTime EnterTime { get; set; }

        /// <summary>Exit time (null if still in area)</summary>
        public DateTime? ExitTime { get; set; }

        /// <summary>Duration in minutes</summary>
        public int? DurationMinutes { get; set; }

        /// <summary>Whether this area had an incident</summary>
        public bool HasIncident { get; set; }

        /// <summary>Incident marker (if hasIncident = true)</summary>
        public IncidentMarkerRead? Incident { get; set; }
    }

    /// <summary>
    /// Incident marker for journey step
    /// </summary>
    public class IncidentMarkerRead
    {
        /// <summary>Alarm trigger ID</summary>
        public Guid AlarmTriggerId { get; set; }

        /// <summary>Alarm record tracking ID</summary>
        public Guid? AlarmRecordTrackingId { get; set; }

        /// <summary>Alarm status (Active, Done, etc.)</summary>
        public string? AlarmStatus { get; set; }

        /// <summary>Action status (Investigated, Done, etc.)</summary>
        public string? ActionStatus { get; set; }

        /// <summary>Whether alarm is still active</summary>
        public bool IsActive { get; set; }

        /// <summary>Trigger time</summary>
        public DateTime TriggerTime { get; set; }

        /// <summary>Person who acknowledged</summary>
        public string? AcknowledgedBy { get; set; }

        /// <summary>Person who investigated</summary>
        public string? InvestigatedBy { get; set; }

        /// <summary>Investigation result</summary>
        public string? InvestigationResult { get; set; }

        /// <summary>Security assigned</summary>
        public string? SecurityName { get; set; }

        /// <summary>Timeline summary (short)</summary>
        public string TimelineSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response wrapper for common paths
    /// </summary>
    public class CommonPathsResponse
    {
        /// <summary>List of common paths</summary>
        public List<CommonPathRead> Data { get; set; } = new();

        /// <summary>Total number of unique journeys analyzed</summary>
        public int TotalJourneys { get; set; }

        /// <summary>Date range of analysis</summary>
        public string DateRange { get; set; } = string.Empty;
    }
}
