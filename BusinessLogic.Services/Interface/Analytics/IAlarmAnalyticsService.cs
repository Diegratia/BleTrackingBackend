using Data.ViewModels;
using Data.ViewModels.AlarmAnalytics;


namespace BusinessLogic.Services.Interface.Analytics;

public interface IAlarmAnalyticsService
{
    Task<ResponseCollection<AlarmDailySummaryVM>> GetDailySummaryAsync(AlarmAnalyticsRequest request);
    Task<ResponseCollection<AlarmAreaSummaryVM>> GetAreaSummaryAsync(AlarmAnalyticsRequest request);
    Task<ResponseCollection<AlarmOperatorSummaryVM>> GetOperatorSummaryAsync(AlarmAnalyticsRequest request);
    Task<ResponseCollection<AlarmStatusSummaryVM>> GetStatusSummaryAsync(AlarmAnalyticsRequest request);
    Task<ResponseCollection<AlarmBuildingSummaryVM>> GetBuildingSummaryAsync(AlarmAnalyticsRequest request);
    Task<ResponseCollection<AlarmVisitorSummaryVM>> GetVisitorSummaryAsync(AlarmAnalyticsRequest request);
    Task<ResponseCollection<AlarmTimeOfDaySummaryVM>> GetTimeOfDaySummaryAsync(AlarmAnalyticsRequest request);
    Task<ResponseCollection<AlarmWeeklyTrendVM>> GetWeeklyTrendAsync(AlarmAnalyticsRequest request);
    Task<ResponseCollection<AlarmFloorSummaryVM>> GetFloorSummaryAsync(AlarmAnalyticsRequest request);
    Task<ResponseCollection<AlarmDurationSummaryVM>> GetDurationSummaryAsync(AlarmAnalyticsRequest request);
    Task<ResponseCollection<AlarmTrendByActionVM>> GetTrendByActionAsync(AlarmAnalyticsRequest request);
}
