using Shared.Contracts;
using Shared.Contracts.Analytics;
using Data.ViewModels;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interface.Analytics
{
    public interface IPatrolSessionAnalyticsService
    {
        Task<object> GetReportAsync(DataTablesProjectedRequest request, PatrolSessionAnalyticsFilter filter, bool includeTimeline = true, bool includeCases = true);
        Task<PatrolSessionAnalyticsRead?> GetSessionTimelineAsync(Guid sessionId, bool includeTimeline = true, bool includeCases = true);
        Task<byte[]> ExportToPdfAsync(PatrolSessionAnalyticsFilter filter, bool includeTimeline = false, bool includeCases = false);
    }
}
