using System.Threading.Tasks;
using Data.ViewModels;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Interface.Analytics;

    public interface IAlarmAnalyticsIncidentService
    {
        Task<ResponseCollection<object>> GetAreaSummaryAsync(AlarmAnalyticsRequestRM request);
        Task<ResponseCollection<object>> GetDailySummaryAsync(AlarmAnalyticsRequestRM request);
        Task<ResponseCollection<object>> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request);
        Task<ResponseCollection<object>> GetVisitorSummaryAsync(AlarmAnalyticsRequestRM request);
        Task<ResponseCollection<object>> GetBuildingSummaryAsync(AlarmAnalyticsRequestRM request);
    }

