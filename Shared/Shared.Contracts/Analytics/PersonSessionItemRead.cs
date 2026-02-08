using System;

namespace Shared.Contracts.Analytics
{
    /// <summary>
    /// Minimal session item for person sessions
    /// Does NOT include person info (use parent PersonSessionsRead for person details)
    /// </summary>
    public class PersonSessionItemRead
    {
        // =====================================================
        // AREA & LOCATION INFO
        // =====================================================

        /// <summary>Area/MaskedArea ID</summary>
        public Guid? AreaId { get; set; }

        /// <summary>Area name</summary>
        public string? AreaName { get; set; }

        /// <summary>Building ID</summary>
        public Guid? BuildingId { get; set; }

        /// <summary>Building name</summary>
        public string? BuildingName { get; set; }

        /// <summary>Floor ID</summary>
        public Guid? FloorId { get; set; }

        /// <summary>Floor name</summary>
        public string? FloorName { get; set; }

        /// <summary>Floorplan ID</summary>
        public Guid? FloorplanId { get; set; }

        /// <summary>Floorplan name</summary>
        public string? FloorplanName { get; set; }

        /// <summary>Floorplan image URL</summary>
        public string? FloorplanImage { get; set; }

        // =====================================================
        // SESSION TIMING
        // =====================================================

        /// <summary>Session enter time (first detection)</summary>
        public DateTime EnterTime { get; set; }

        /// <summary>Session exit time (last detection)</summary>
        public DateTime? ExitTime { get; set; }

        /// <summary>Session duration in minutes</summary>
        public int? DurationMinutes { get; set; }

        /// <summary>Duration formatted in human-readable string</summary>
        public string? DurationFormatted { get; set; }

        /// <summary>Session status: "active" or "completed"</summary>
        public string SessionStatus { get; set; } = "completed";

        // =====================================================
        // INCIDENT INFO
        // =====================================================

        /// <summary>Whether this session has an incident</summary>
        public bool HasIncident { get; set; }

        /// <summary>Incident marker data (only included if IncludeIncident=true)</summary>
        public IncidentMarkerRead? Incident { get; set; }
    }
}
