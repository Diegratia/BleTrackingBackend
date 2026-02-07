using System.Threading.Tasks;
using Data.ViewModels.AlarmAnalytics;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Interface.Analytics;

public interface ITrackingSummaryService
{
    Task<object> GetAreaSummaryAsync(TrackingAnalyticsRequestRM request);
    Task<object> GetDailySummaryAsync(TrackingAnalyticsRequestRM request);
    Task<object> GetReaderSummaryAsync(TrackingAnalyticsRequestRM request);
    Task<object> GetVisitorSummaryAsync(TrackingAnalyticsRequestRM request);
    Task<object> GetBuildingSummaryAsync(TrackingAnalyticsRequestRM request);
    Task<object> GetTrackingMovementByCardIdAsync(Guid cardId);
    Task<object> GetHeatmapDataAsync(TrackingAnalyticsRequestRM request);
    Task<object> GetCardSummaryAsync(TrackingAnalyticsRequestRM request);
    Task<object> GetAreaAccessedSummaryAsyncV3(TrackingAnalyticsRequestRM request);
}
