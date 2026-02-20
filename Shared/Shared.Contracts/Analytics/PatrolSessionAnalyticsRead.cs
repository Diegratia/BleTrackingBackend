using System;
using System.Collections.Generic;

namespace Shared.Contracts.Analytics
{
    /// <summary>
    /// Main response for patrol session analytics
    /// </summary>
    public class PatrolSessionAnalyticsResponse
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<PatrolSessionAnalyticsRead> Data { get; set; }
        public PatrolSessionSummaryRead? Summary { get; set; }
    }

    /// <summary>
    /// Patrol session read with timeline, metrics, and incidents
    /// </summary>
    public class PatrolSessionAnalyticsRead
    {
        public Guid SessionId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string? DurationFormatted { get; set; }

        // Security info
        public Guid SecurityId { get; set; }
        public string SecurityName { get; set; }
        public string? SecurityEmployeeNumber { get; set; }

        // Assignment info
        public Guid AssignmentId { get; set; }
        public string AssignmentName { get; set; }

        // Route info
        public Guid RouteId { get; set; }
        public string RouteName { get; set; }

        // Metrics
        public PatrolSessionMetrics Metrics { get; set; }

        // Timeline (optional)
        public List<PatrolTimelineEvent>? Timeline { get; set; }

        // Incidents (optional)
        public List<PatrolIncidentSummary>? Incidents { get; set; }
    }

    /// <summary>
    /// Timeline event for patrol session
    /// </summary>
    public class PatrolTimelineEvent
    {
        public string Stage { get; set; }       // "started", "checkpoint_1", "checkpoint_2", ..., "completed"
        public string StageName { get; set; }   // "Started Patrol", "Checkpoint: Lobby", etc.
        public DateTime Timestamp { get; set; }
        public int? DurationSeconds { get; set; }
        public string? DurationFormatted { get; set; }
        public bool IsDelayed { get; set; }
        public int? DelaySeconds { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Incident summary during patrol
    /// </summary>
    public class PatrolIncidentSummary
    {
        public Guid CaseId { get; set; }
        public DateTime ReportedAt { get; set; }
        public string Title { get; set; }
        public string CaseType { get; set; }      // Incident, Hazard, Damage, Theft, Report, PatrolSummary
        public string ThreatLevel { get; set; }   // Low, Medium, High, Critical
        public string CaseStatus { get; set; }    // Pending, Approved, Rejected
        public string? AreaName { get; set; }
    }

    /// <summary>
    /// Session metrics calculated from checkpoint logs
    /// </summary>
    public class PatrolSessionMetrics
    {
        public int TotalCheckpoints { get; set; }
        public int CompletedCheckpoints { get; set; }
        public int CompletionPercentage { get; set; }
        public int TotalIncidents { get; set; }
        public string? TotalDuration { get; set; }
        public string? AverageCheckpointTime { get; set; }
        public bool IsCompletedOnTime { get; set; }
    }

    /// <summary>
    /// Summary statistics across all sessions
    /// </summary>
    public class PatrolSessionSummaryRead
    {
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int IncompleteSessions { get; set; }
        public double AverageCompletionRate { get; set; }
        public int TotalIncidents { get; set; }
        public int TotalCheckpointsVisited { get; set; }
    }
}
