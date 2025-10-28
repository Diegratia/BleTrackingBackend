using System.Threading.Tasks;
using Data.ViewModels;
using Data.ViewModels.AlarmAnalytics;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Interface.Analytics;

public interface ITrackingAnalyticsService
{
    Task<ResponseCollection<TrackingAreaSummaryDto>> GetAreaSummaryAsync(TrackingAnalyticsRequestRM request);
    Task<ResponseCollection<TrackingDailySummaryDto>> GetDailySummaryAsync(TrackingAnalyticsRequestRM request);
    Task<ResponseCollection<TrackingReaderSummaryDto>> GetReaderSummaryAsync(TrackingAnalyticsRequestRM request);
    Task<ResponseCollection<TrackingVisitorSummaryDto>> GetVisitorSummaryAsync(TrackingAnalyticsRequestRM request);
    Task<ResponseCollection<TrackingBuildingSummaryDto>> GetBuildingSummaryAsync(TrackingAnalyticsRequestRM request);
    Task<ResponseCollection<TrackingMovementRM>> GetTrackingMovementByCardIdAsync(Guid cardId);
    Task<ResponseCollection<TrackingHeatmapRM>> GetHeatmapDataAsync(TrackingAnalyticsRequestRM request);
     Task<ResponseCollection<TrackingCardSummaryRM>> GetCardSummaryAsync(TrackingAnalyticsRequestRM request) ;
    }

