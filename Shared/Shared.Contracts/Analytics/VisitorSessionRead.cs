using System;
using System.Collections.Generic;

namespace Shared.Contracts.Analytics
{
    /// <summary>
    /// Summary statistics for visitor session data
    /// </summary>
    public class VisitorSessionSummaryRead
    {
        /// <summary>Total duration across all sessions in minutes</summary>
        public int TotalDurationMinutes { get; set; }

        /// <summary>First detection time across all sessions</summary>
        public DateTime FirstDetection { get; set; }

        /// <summary>Last detection time across all sessions</summary>
        public DateTime LastDetection { get; set; }

        /// <summary>List of unique areas visited</summary>
        public List<string> AreasVisited { get; set; } = new();

        /// <summary>Total number of detection points</summary>
        public int TotalDetections { get; set; }

        /// <summary>Total number of sessions</summary>
        public int TotalSessions { get; set; }

        /// <summary>Unique visitors count</summary>
        public int UniqueVisitors { get; set; }

        /// <summary>Unique members count</summary>
        public int UniqueMembers { get; set; }
    }

    /// <summary>
    /// Visual path data for floorplan visualization
    /// </summary>
    public class VisualPathsRead
    {
        /// <summary>Dictionary keyed by floorplan ID containing path data</summary>
        public Dictionary<string, FloorplanPathRead> Floorplans { get; set; } = new();
    }

    /// <summary>
    /// Path points for a specific floorplan
    /// </summary>
    public class FloorplanPathRead
    {
        /// <summary>Floorplan ID</summary>
        public Guid FloorplanId { get; set; }

        /// <summary>Floorplan name</summary>
        public string FloorplanName { get; set; }

        /// <summary>Floorplan image URL</summary>
        public string? FloorplanImage { get; set; }

        /// <summary>List of points on this floorplan</summary>
        public List<FloorplanPointRead> Points { get; set; } = new();
    }

    /// <summary>
    /// Single point on a floorplan
    /// </summary>
    public class FloorplanPointRead
    {
        /// <summary>X coordinate</summary>
        public float X { get; set; }

        /// <summary>Y coordinate</summary>
        public float Y { get; set; }

        /// <summary>Detection time</summary>
        public DateTime Time { get; set; }

        /// <summary>Area name where this point is located</summary>
        public string? Area { get; set; }

        /// <summary>Person name (optional, for filtering)</summary>
        public string? PersonName { get; set; }

        /// <summary>Person ID (optional)</summary>
        public Guid? PersonId { get; set; }
    }

    /// <summary>
    /// Enhanced response with summary and visual paths
    /// </summary>
    public class VisitorSessionEnhancedResponseRead
    {
        /// <summary>List of visitor sessions (existing data)</summary>
        public object Data { get; set; } = null!;

        /// <summary>Summary statistics</summary>
        public VisitorSessionSummaryRead? Summary { get; set; }

        /// <summary>Visual paths for floorplan visualization</summary>
        public VisualPathsRead? VisualPaths { get; set; }
    }

    /// <summary>
    /// Individual visitor session record for analytics
    /// Represents a single visit/dwell in an area by a person
    /// </summary>
    public class VisitorSessionRead
    {
        // =====================================================
        // PERSON INFO
        // =====================================================

        /// <summary>
        /// Person ID (VisitorId or MemberId)
        /// </summary>
        public Guid? PersonId { get; set; }

        /// <summary>
        /// Person name (VisitorName or MemberName)
        /// </summary>
        public string? PersonName { get; set; }

        /// <summary>
        /// Person type: "Visitor" or "Member"
        /// </summary>
        public string? PersonType { get; set; }

        /// <summary>
        /// Identity ID (card number, member code, etc.)
        /// </summary>
        public string? IdentityId { get; set; }

        // =====================================================
        // CARD INFO
        // =====================================================

        /// <summary>
        /// Card ID used for detection
        /// </summary>
        public Guid? CardId { get; set; }

        /// <summary>
        /// Card name
        /// </summary>
        public string? CardName { get; set; }

        /// <summary>
        /// Card number
        /// </summary>
        public string? CardNumber { get; set; }

        // =====================================================
        // VISITOR SPECIFIC
        // =====================================================

        /// <summary>
        /// Visitor ID (if person is a visitor)
        /// </summary>
        public Guid? VisitorId { get; set; }

        /// <summary>
        /// Visitor name
        /// </summary>
        public string? VisitorName { get; set; }

        // =====================================================
        // MEMBER SPECIFIC
        // =====================================================

        /// <summary>
        /// Member ID (if person is a member)
        /// </summary>
        public Guid? MemberId { get; set; }

        /// <summary>
        /// Member name
        /// </summary>
        public string? MemberName { get; set; }

        // =====================================================
        // LOCATION INFO
        // =====================================================

        /// <summary>
        /// Building ID where session occurred
        /// </summary>
        public Guid? BuildingId { get; set; }

        /// <summary>
        /// Building name
        /// </summary>
        public string? BuildingName { get; set; }

        /// <summary>
        /// Floor ID where session occurred
        /// </summary>
        public Guid? FloorId { get; set; }

        /// <summary>
        /// Floor name
        /// </summary>
        public string? FloorName { get; set; }

        /// <summary>
        /// Floorplan ID
        /// </summary>
        public Guid? FloorplanId { get; set; }

        /// <summary>
        /// Floorplan name
        /// </summary>
        public string? FloorplanName { get; set; }

        /// <summary>
        /// Floorplan image URL
        /// </summary>
        public string? FloorplanImage { get; set; }

        /// <summary>
        /// Area/MaskedArea ID where session occurred
        /// </summary>
        public Guid? AreaId { get; set; }

        /// <summary>
        /// Area name
        /// </summary>
        public string? AreaName { get; set; }

        // =====================================================
        // SESSION TIMING
        // =====================================================

        /// <summary>
        /// Session enter time (first detection)
        /// </summary>
        public DateTime EnterTime { get; set; }

        /// <summary>
        /// Session exit time (last detection)
        /// Null if session is still active
        /// </summary>
        public DateTime? ExitTime { get; set; }

        /// <summary>
        /// Session duration in minutes
        /// Null if session is still active
        /// </summary>
        public int? DurationInMinutes { get; set; }

        // =====================================================
        // ADDITIONAL INFO
        // =====================================================

        /// <summary>
        /// Session status (optional)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Host name (optional)
        /// </summary>
        public string? HostName { get; set; }

        // =====================================================
        // NEW FIELDS
        // =====================================================

        /// <summary>
        /// Duration formatted in human-readable string
        /// null if session is still active
        /// </summary>
        public string? DurationFormatted { get; set; }

        /// <summary>
        /// Session status: "active" or "completed"
        /// </summary>
        public string SessionStatus { get; set; } = "completed";

        /// <summary>
        /// Whether this session has an incident
        /// </summary>
        public bool HasIncident { get; set; }

        /// <summary>
        /// Incident marker data (only included if IncludeIncident=true)
        /// </summary>
        public IncidentMarkerRead? Incident { get; set; }
    }
}
