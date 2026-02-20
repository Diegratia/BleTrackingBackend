using Shared.Contracts;
using Shared.Contracts.Analytics;
using Data.ViewModels;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interface.Analytics
{
    public interface IPatrolSessionAnalyticsService
    {
        Task<object> GetReportAsync(DataTablesProjectedRequest request, PatrolSessionAnalyticsFilter filter);
        Task<PatrolSessionAnalyticsRead?> GetSessionTimelineAsync(Guid sessionId);
        Task<byte[]> ExportToPdfAsync(PatrolSessionAnalyticsFilter filter);
    }
}
