using System.Threading.Tasks;
using Shared.Contracts;
using Shared.Contracts.Analytics;

namespace BusinessLogic.Services.Interface.Analytics;

public interface ITrackingSessionService
{
    Task<object> GetVisitorSessionSummaryAsync(TrackingAnalyticsFilter request);
    Task<object> GetVisitorSessionSummaryAsync(TrackingAnalyticsFilter request, bool includeVisualPaths);
    Task<byte[]> ExportVisitorSessionSummaryToPdfAsync(TrackingAnalyticsFilter request);
    Task<byte[]> ExportVisitorSessionSummaryToExcelAsync(TrackingAnalyticsFilter request);
    Task<object> GetVisitorSessionSummaryByPresetAsync(Guid presetId, TrackingAnalyticsFilter overrideRequest);

    /// <summary>
    /// Get peak hours data grouped by area
    /// Returns hourly distribution of visitors across different areas
    /// </summary>
    Task<PeakHoursByAreaRead> GetPeakHoursByAreaAsync(TrackingAnalyticsFilter request);
}
