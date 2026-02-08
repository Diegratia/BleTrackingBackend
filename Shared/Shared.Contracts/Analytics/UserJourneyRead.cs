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
    /// Security journey check result
    /// </summary>
    public class SecurityJourneyCheckRead
    {
        /// <summary>Visitor ID</summary>
        public Guid? VisitorId { get; set; }

        /// <summary>Visitor name</summary>
        public string? VisitorName { get; set; }

        /// <summary>Member ID (if applicable)</summary>
        public Guid? MemberId { get; set; }

        /// <summary>Member name (if applicable)</summary>
        public string? MemberName { get; set; }

        /// <summary>Path of areas taken during journey</summary>
        public List<string> PathTaken { get; set; } = new();

        /// <summary>Overall risk level: Low, Medium, High, Critical</summary>
        public string RiskLevel { get; set; } = "Low";

        /// <summary>Whether visitor requires escort</summary>
        public bool RequiresEscort { get; set; }

        /// <summary>List of security violations detected</summary>
        public List<SecurityViolation> Violations { get; set; } = new();

        /// <summary>Total journey duration in minutes</summary>
        public int? TotalDurationMinutes { get; set; }
    }

    /// <summary>
    /// Security violation detail
    /// </summary>
    public class SecurityViolation
    {
        /// <summary>Violation type</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>Area name where violation occurred</summary>
        public string? AreaName { get; set; }

        /// <summary>Description of the violation</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Security zone that was violated</summary>
        public string? ZoneType { get; set; }
    }

    /// <summary>
    /// Next area prediction result
    /// </summary>
    public class NextAreaPredictionRead
    {
        /// <summary>Current area name</summary>
        public string CurrentArea { get; set; } = string.Empty;

        /// <summary>Current area ID</summary>
        public Guid? CurrentAreaId { get; set; }

        /// <summary>Total number of journeys from this area</summary>
        public int TotalFromArea { get; set; }

        /// <summary>Predicted next areas with probabilities</summary>
        public List<AreaProbability> NextAreas { get; set; } = new();
    }

    /// <summary>
    /// Area transition probability
    /// </summary>
    public class AreaProbability
    {
        /// <summary>Next area name</summary>
        public string AreaName { get; set; } = string.Empty;

        /// <summary>Next area ID</summary>
        public Guid? AreaId { get; set; }

        /// <summary>Transition probability (0-1)</summary>
        public double Probability { get; set; }

        /// <summary>Number of transitions to this area</summary>
        public int Count { get; set; }

        /// <summary>Average time in minutes to reach this area</summary>
        public double? AvgTimeToReachMinutes { get; set; }
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
