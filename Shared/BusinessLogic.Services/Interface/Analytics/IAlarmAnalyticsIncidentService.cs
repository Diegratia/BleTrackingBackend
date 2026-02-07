using System.Threading.Tasks;
using Data.ViewModels.AlarmAnalytics;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Interface.Analytics;

public interface IAlarmAnalyticsIncidentService
{
    Task<object> GetDailySummaryAsync(AlarmAnalyticsRequestRM request);
    Task<object> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request);
    Task<object> GetVisitorSummaryAsync(AlarmAnalyticsRequestRM request);
    Task<object> GetBuildingSummaryAsync(AlarmAnalyticsRequestRM request);
    Task<object> GetHourlyStatusSummaryAsync(AlarmAnalyticsRequestRM request);
    Task<object> GetAreaSummaryChartAsync(AlarmAnalyticsRequestRM request);
}

