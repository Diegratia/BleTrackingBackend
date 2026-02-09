using System;

namespace Shared.Contracts
{
    /// <summary>
    /// Filter for tracking analytics queries
    /// Supports time-based filtering, entity filtering, and data export options
    /// Uses DataTables format for pagination (draw, start, length)
    /// </summary>
    public class TrackingAnalyticsFilter
    {
        // =====================================================
        // DATATABLES PAGINATION
        // =====================================================

        /// <summary>
        /// Draw counter from DataTables request (echoed back in response)
        /// </summary>
        public int Draw { get; set; } = 1;

        /// <summary>
        /// Paging first record indicator (0-based index)
        /// Calculated as: Page = (Start / Length) + 1
        /// </summary>
        public int Start { get; set; } = 0;

        /// <summary>
        /// Number of records per page
        /// </summary>
        public int Length { get; set; } = 10;

        /// <summary>
        /// Search string for filtering
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Sort column name
        /// </summary>
        public string? SortColumn { get; set; }

        /// <summary>
        /// Sort direction: "asc" or "desc"
        /// </summary>
        public string? SortDir { get; set; }

        // =====================================================
        // TIME FILTERS
        // =====================================================

        /// <summary>
        /// Start datetime for query range (overrides TimeRange)
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// End datetime for query range (overrides TimeRange)
        /// </summary>
        public DateTime? To { get; set; }

        /// <summary>
        /// Predefined time range: today, yesterday, daily, weekly, monthly, etc.
        /// </summary>
        public string? TimeRange { get; set; }

        // =====================================================
        // ENTITY FILTERS
        // =====================================================

        /// <summary>
        /// Filter by Building ID
        /// </summary>
        public Guid? BuildingId { get; set; }

        /// <summary>
        /// Filter by Floor ID
        /// </summary>
        public Guid? FloorId { get; set; }

        /// <summary>
        /// Filter by Floorplan ID (legacy alias for FloorId)
        /// </summary>
        public Guid? FloorplanId { get; set; }

        /// <summary>
        /// Filter by Area/MaskedArea ID
        /// </summary>
        public Guid? AreaId { get; set; }

        /// <summary>
        /// Filter by Card ID
        /// </summary>
        public Guid? CardId { get; set; }

        /// <summary>
        /// Filter by Visitor ID
        /// </summary>
        public Guid? VisitorId { get; set; }

        /// <summary>
        /// Filter by Member ID
        /// </summary>
        public Guid? MemberId { get; set; }

        /// <summary>
        /// Filter by Reader/BLE ID
        /// </summary>
        public Guid? ReaderId { get; set; }

        // =====================================================
        // STRING FILTERS
        // =====================================================

        /// <summary>
        /// Filter by identity ID (card number, member code, etc.)
        /// </summary>
        public string? IdentityId { get; set; }

        /// <summary>
        /// Filter by person type: "visitor", "member", "all"
        /// </summary>
        public string? PersonType { get; set; }

        // =====================================================
        // NEW FILTERS
        // =====================================================

        /// <summary>
        /// Person type filter: "visitor" (default), "member", "security"
        /// </summary>
        public string Type { get; set; } = "visitor";

        /// <summary>
        /// Filter sessions with/without incidents
        /// true = only sessions with incidents
        /// false = only sessions without incidents
        /// null = all sessions (default)
        /// </summary>
        public bool? HasIncident { get; set; }

        /// <summary>
        /// Include summary statistics in response
        /// </summary>
        public bool IncludeSummary { get; set; } = false;

        /// <summary>
        /// Include visual paths per floorplan in response
        /// </summary>
        public bool IncludeVisualPaths { get; set; } = false;

        /// <summary>
        /// Maximum points per floorplan for visual paths (sampling)
        /// Reduces data payload for better performance
        /// Default: 1000 points per floorplan
        /// Set to 0 or null for no limit (not recommended for large datasets)
        /// </summary>
        public int? MaxPointsPerFloorplan { get; set; } = 1000;

        /// <summary>
        /// Include incident data in sessions
        /// false = only hasIncident boolean flag
        /// true = include full incident object (default)
        /// </summary>
        public bool IncludeIncident { get; set; } = true;

        // =====================================================
        // EXPORT OPTIONS
        // =====================================================

        /// <summary>
        /// Report title for export files
        /// </summary>
        public string? ReportTitle { get; set; }

        /// <summary>
        /// Export type: "pdf", "excel", "csv"
        /// </summary>
        public string? ExportType { get; set; }

        /// <summary>
        /// Target timezone for response datetimes
        /// Default: UTC
        /// Examples: "WIB", "UTC+7", "Asia/Jakarta"
        /// </summary>
        public string? Timezone { get; set; }
    }
}
