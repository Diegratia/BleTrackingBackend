using System.Threading.Tasks;
using Data.ViewModels;
using Data.ViewModels.AlarmAnalytics;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Interface.Analytics;

public interface IAlarmAnalyticsIncidentService
{
    // Task<ResponseCollection<AlarmAreaSummaryDto>> GetAreaSummaryAsync(AlarmAnalyticsRequestRM request);
    Task<ResponseCollection<AlarmDailySummaryDto>> GetDailySummaryAsync(AlarmAnalyticsRequestRM request);
    Task<ResponseCollection<AlarmStatusSummaryDto>> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request);
    Task<ResponseCollection<AlarmVisitorSummaryDto>> GetVisitorSummaryAsync(AlarmAnalyticsRequestRM request);
    Task<ResponseCollection<AlarmBuildingSummaryDto>> GetBuildingSummaryAsync(AlarmAnalyticsRequestRM request);
    Task<ResponseCollection<AlarmHourlyStatusSummaryDto>> GetHourlyStatusSummaryAsync(AlarmAnalyticsRequestRM request);
    Task<object> GetAreaSummaryChartAsync(
            AlarmAnalyticsRequestRM request
        );
    }

