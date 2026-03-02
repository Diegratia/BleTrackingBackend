// File: BusinessLogic/Services/Interface/Analytics/ITrackingAnalyticsV2Service.cs
using Data.ViewModels;
using Repositories.Repository.RepoModel;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interface.Analytics
{
    public interface ITrackingAnalyticsV2Service
    {
        Task<ResponseCollection<VisitorSessionSummaryRM>> GetVisitorSessionSummaryAsync(TrackingAnalyticsRequestRM request);
        Task<byte[]> ExportVisitorSessionSummaryToPdfAsync(TrackingAnalyticsRequestRM request);
        Task<byte[]> ExportVisitorSessionSummaryToExcelAsync(TrackingAnalyticsRequestRM request);
        Task<ResponseCollection<VisitorSessionSummaryRM>>GetVisitorSessionSummaryByPresetAsync(Guid presetId, TrackingAnalyticsRequestRM overrideRequest);
    }
}