namespace Shared.Contracts.Reporting;

public class ReportExportRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10000; // Default max rows per export
    public string? SortColumn { get; set; }
    public string? SortDir { get; set; }

    // Optional filter parameters for generating filter info
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? TimeRange { get; set; }
    public string? Search { get; set; }
    public int? Status { get; set; }
}
