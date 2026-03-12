using System.Threading.Tasks;
using Data.ViewModels.AlarmAnalytics;
using Shared.Contracts;
using Shared.Contracts.Analytics;

namespace BusinessLogic.Services.Interface.Analytics;

public interface IAlarmAnalyticsIncidentService
{
    Task<List<AlarmDailyRead>> GetDailySummaryAsync(AlarmAnalyticsFilter request);
    Task<List<AlarmStatusRead>> GetStatusSummaryAsync(AlarmAnalyticsFilter request);
    Task<List<AlarmVisitorRead>> GetVisitorSummaryAsync(AlarmAnalyticsFilter request);
    Task<List<AlarmBuildingRead>> GetBuildingSummaryAsync(AlarmAnalyticsFilter request);
    Task<List<AlarmHourlyStatusRead>> GetHourlyStatusSummaryAsync(AlarmAnalyticsFilter request);
    Task<List<AlarmInvestigatedResultRead>> GetInvestigatedResultSummaryAsync(AlarmAnalyticsFilter request);
    Task<List<AlarmDurationAnalyticsRead>> GetAverageDurationSummaryAsync(AlarmAnalyticsFilter request);
    Task<AlarmAreaChartResponseRead> GetAreaSummaryChartAsync(
        AlarmAnalyticsFilter request,
        AlarmGroupByMode groupByMode = AlarmGroupByMode.Area);
    Task<SecurityHeadDashboardSummaryRead> GetSecurityHeadDashboardSummaryAsync(AlarmAnalyticsFilter request);
}
