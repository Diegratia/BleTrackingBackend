using System.Threading.Tasks;
using Data.ViewModels.AlarmAnalytics;
using Shared.Contracts;

namespace BusinessLogic.Services.Interface.Analytics;

public interface ITrackingSummaryService
{
    Task<object> GetAreaSummaryAsync(TrackingAnalyticsFilter request);
    Task<object> GetDailySummaryAsync(TrackingAnalyticsFilter request);
    Task<object> GetReaderSummaryAsync(TrackingAnalyticsFilter request);
    Task<object> GetVisitorSummaryAsync(TrackingAnalyticsFilter request);
    Task<object> GetBuildingSummaryAsync(TrackingAnalyticsFilter request);
    Task<object> GetTrackingMovementByCardIdAsync(Guid cardId);
    Task<object> GetHeatmapDataAsync(TrackingAnalyticsFilter request);
    Task<object> GetCardSummaryAsync(TrackingAnalyticsFilter request);
    Task<object> GetAreaAccessedSummaryAsyncV3(TrackingAnalyticsFilter request);
}
