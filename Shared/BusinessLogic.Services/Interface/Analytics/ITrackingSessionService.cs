using System.Threading.Tasks;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Interface.Analytics;

public interface ITrackingSessionService
{
    Task<object> GetVisitorSessionSummaryAsync(TrackingAnalyticsRequestRM request);
    Task<byte[]> ExportVisitorSessionSummaryToPdfAsync(TrackingAnalyticsRequestRM request);
    Task<byte[]> ExportVisitorSessionSummaryToExcelAsync(TrackingAnalyticsRequestRM request);
    Task<object> GetVisitorSessionSummaryByPresetAsync(Guid presetId, TrackingAnalyticsRequestRM overrideRequest);
}
