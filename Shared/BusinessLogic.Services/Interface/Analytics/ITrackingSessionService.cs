using System.Threading.Tasks;
using Shared.Contracts;
using Shared.Contracts.Analytics;

namespace BusinessLogic.Services.Interface.Analytics;

public interface ITrackingSessionService
{
    Task<GroupedSessionsResponse> GetVisitorSessionSummaryAsync(TrackingAnalyticsFilter request);
    Task<byte[]> ExportVisitorSessionSummaryToPdfAsync(TrackingAnalyticsFilter request);
    Task<byte[]> ExportVisitorSessionSummaryToExcelAsync(TrackingAnalyticsFilter request);
    Task<GroupedSessionsResponse> GetVisitorSessionSummaryByPresetAsync(Guid presetId, TrackingAnalyticsFilter overrideRequest);

    /// <summary>
    /// Get peak hours data grouped by area
    /// Returns hourly distribution of visitors across different areas
    /// </summary>
    Task<PeakHoursByAreaRead> GetPeakHoursByAreaAsync(TrackingAnalyticsFilter request);
}
