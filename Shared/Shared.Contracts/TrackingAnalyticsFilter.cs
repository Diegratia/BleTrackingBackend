using System;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    /// <summary>
    /// Filter for tracking analytics queries
    /// Supports time-based filtering, entity filtering, and data export options
    /// </summary>
    public class TrackingAnalyticsFilter : BaseFilter
    {
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
