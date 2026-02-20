using System;
using System.Text.Json;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    /// <summary>
    /// Filter for patrol session analytics and reporting
    /// Uses DataTables Pattern (BaseFilter)
    /// </summary>
    public class PatrolSessionAnalyticsFilter : BaseFilter
    {
        // =====================================================
        // ENTITY FILTERS (using JsonElement for array support)
        // =====================================================

        public JsonElement SecurityId { get; set; }
        public JsonElement RouteId { get; set; }
        public JsonElement AreaId { get; set; }
        public JsonElement IsCompleted { get; set; }

        // =====================================================
        // INCLUDE OPTIONS (for shaping response)
        // =====================================================

        public bool IncludeTimeline { get; set; } = true;
        public bool IncludeIncidents { get; set; } = true;

        // =====================================================
        // EXPORT OPTIONS
        // =====================================================

        public string? ReportTitle { get; set; }
        public string? Timezone { get; set; }  // "WIB", "UTC+7", "Asia/Jakarta"
    }
}
