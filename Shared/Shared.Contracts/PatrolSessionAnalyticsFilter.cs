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
        public JsonElement AssignmentId { get; set; }
        public JsonElement AreaId { get; set; }
        public JsonElement IsCompleted { get; set; }

        // =====================================================
        // EXPORT OPTIONS
        // =====================================================

        public string? ReportTitle { get; set; }
        public string? TimeRange { get; set; }
    }
}
