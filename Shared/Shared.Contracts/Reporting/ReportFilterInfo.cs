namespace Shared.Contracts.Reporting;

public class ReportFilterInfo
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? TimeRange { get; set; }
    public Guid? BuildingId { get; set; }
    public Guid? FloorId { get; set; }
    public Guid? AreaId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Search { get; set; }
    public int? Status { get; set; }
    public string? CustomFilter { get; set; }
}
