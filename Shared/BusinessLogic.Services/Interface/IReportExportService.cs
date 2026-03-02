using Shared.Contracts.Reporting;

namespace BusinessLogic.Services.Interface;

public interface IReportExportService
{
    Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, ReportMetadata metadata);
    Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, ReportMetadata metadata);

    // Helper methods (inspired by TrackingSessionService pattern)
    string GenerateReportTitle(string baseTitle, string? customTitle, string? timeRange);
    string GenerateFilterInfo(ReportFilterInfo filters);
    ReportExportRequest ApplyDefaultPagination(ReportExportRequest? request);
}
