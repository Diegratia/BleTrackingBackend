using System;

namespace Shared.Contracts.Analytics
{
    /// <summary>
    /// Incident marker for visitor session
    /// Contains alarm trigger details and timeline
    /// </summary>
    public class IncidentMarkerRead
    {
        // =====================================================
        // BASIC INFO
        // =====================================================

        /// <summary>Alarm trigger ID</summary>
        public Guid AlarmTriggerId { get; set; }

        /// <summary>Alarm color: "red", "yellow", "green"</summary>
        public string? AlarmColor { get; set; }

        /// <summary>Alarm status: "Active", "Done", "Cancelled"</summary>
        public string? AlarmStatus { get; set; }

        /// <summary>Action status: "Acknowledged", "Dispatched", "Arrived", "Investigated", "Done"</summary>
        public string? ActionStatus { get; set; }

        /// <summary>Whether alarm is still active</summary>
        public bool IsActive { get; set; }

        // =====================================================
        // TIMESTAMPS
        // =====================================================

        /// <summary>When alarm was triggered</summary>
        public DateTime TriggerTime { get; set; }

        /// <summary>When alarm was acknowledged</summary>
        public DateTime? AcknowledgedAt { get; set; }

        /// <summary>When security was dispatched</summary>
        public DateTime? DispatchedAt { get; set; }

        /// <summary>When security arrived</summary>
        public DateTime? ArrivedAt { get; set; }

        /// <summary>When investigation started</summary>
        public DateTime? InvestigatedAt { get; set; }

        /// <summary>When incident was resolved</summary>
        public DateTime? DoneAt { get; set; }

        // =====================================================
        // ACTORS
        // =====================================================

        /// <summary>Person who acknowledged</summary>
        public string? AcknowledgedBy { get; set; }

        /// <summary>Person who was dispatched</summary>
        public string? DispatchedBy { get; set; }

        /// <summary>Person who arrived</summary>
        public string? ArrivedBy { get; set; }

        /// <summary>Person who investigated</summary>
        public string? InvestigatedBy { get; set; }

        /// <summary>Person who marked as done</summary>
        public string? DoneBy { get; set; }

        // =====================================================
        // SECURITY INFO
        // =====================================================

        /// <summary>Security ID assigned to incident</summary>
        public Guid? SecurityId { get; set; }

        /// <summary>Security name assigned to incident</summary>
        public string? SecurityName { get; set; }

        // =====================================================
        // INVESTIGATION
        // =====================================================

        /// <summary>Investigation result</summary>
        public string? InvestigationResult { get; set; }

        // =====================================================
        // METRICS
        // =====================================================

        /// <summary>Response time in seconds (Trigger → Acknowledged)</summary>
        public int ResponseTimeSeconds { get; set; }

        /// <summary>Response time formatted</summary>
        public string ResponseTimeFormatted { get; set; } = string.Empty;

        /// <summary>Resolution time in seconds (Trigger → Done)</summary>
        public int ResolutionTimeSeconds { get; set; }

        /// <summary>Resolution time formatted</summary>
        public string ResolutionTimeFormatted { get; set; } = string.Empty;

        // =====================================================
        // SUMMARY
        // =====================================================

        /// <summary>Timeline summary string</summary>
        public string TimelineSummary { get; set; } = string.Empty;
    }
}
