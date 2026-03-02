// File: BusinessLogic/Services/Interface/Analytics/ITrackingAnalyticsV2Service.cs
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BusinessLogic.Services.Interface.Analytics
{
    /// <summary>
    /// V2 Service for flat list visitor session summary
    /// Returns List<VisitorSessionSummaryRM> with flat list format
    /// (Legacy V2 - separate from grouped sessions in ITrackingSessionService)
    /// </summary>
    public interface ITrackingAnalyticsV2Service
    {
        /// <summary>
        /// Get visitor session summary as flat list
        /// Returns List<VisitorSessionSummaryRM> instead of grouped format
        /// </summary>
        Task<List<VisitorSessionSummaryRM>> GetVisitorSessionSummaryAsync(TrackingAnalyticsFilter request);

        /// <summary>
        /// Get visitor session summary by preset with override options
        /// </summary>
        Task<List<VisitorSessionSummaryRM>> GetVisitorSessionSummaryByPresetAsync(Guid presetId, TrackingAnalyticsFilter overrideRequest);

        /// <summary>
        /// Export visitor session summary to PDF (flat format)
        /// </summary>
        Task<byte[]> ExportVisitorSessionSummaryToPdfAsync(TrackingAnalyticsFilter request);

        /// <summary>
        /// Export visitor session summary to Excel (flat format)
        /// </summary>
        Task<byte[]> ExportVisitorSessionSummaryToExcelAsync(TrackingAnalyticsFilter request);
    }
}
