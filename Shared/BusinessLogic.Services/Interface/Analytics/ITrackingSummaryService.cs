using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels.AlarmAnalytics;
using Shared.Contracts;
using Shared.Contracts.Analytics;

namespace BusinessLogic.Services.Interface.Analytics;

public interface ITrackingSummaryService
{
    Task<List<TrackingAreaRead>> GetAreaSummaryAsync(TrackingAnalyticsFilter request);
    Task<List<TrackingDailyRead>> GetDailySummaryAsync(TrackingAnalyticsFilter request);
    Task<List<TrackingVisitorRead>> GetVisitorSummaryAsync(TrackingAnalyticsFilter request);
    Task<List<TrackingBuildingRead>> GetBuildingSummaryAsync(TrackingAnalyticsFilter request);
    Task<List<TrackingMovementRead>> GetTrackingMovementByCardIdAsync(Guid cardId);
    Task<List<TrackingHeatmapRead>> GetHeatmapDataAsync(TrackingAnalyticsFilter request);
    Task<List<TrackingCardRead>> GetCardSummaryAsync(TrackingAnalyticsFilter request);
    Task<AreaAccessResponseDto> GetAreaAccessedSummaryAsyncV3(TrackingAnalyticsFilter request);
}
