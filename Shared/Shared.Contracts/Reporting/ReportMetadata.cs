namespace Shared.Contracts.Reporting;

public class ReportMetadata
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? FilterInfo { get; set; }
    public int TotalRecords { get; set; }
    public List<ReportColumn> Columns { get; set; } = new();
    public ReportOrientation Orientation { get; set; } = ReportOrientation.Landscape;
    public bool IncludeRowNumbers { get; set; } = true;
    public bool IncludePageNumbers { get; set; } = true;
    public string? TimeZone { get; set; } = "UTC";
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}

public class ReportColumn
{
    public string Header { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public int Width { get; set; } = 2; // Relative width for PDF
    public string? Format { get; set; } // Optional format string (e.g., "yyyy-MM-dd HH:mm:ss")
    public ColumnAlign Align { get; set; } = ColumnAlign.Left;
}

public enum ReportOrientation
{
    Portrait,
    Landscape
}

public enum ColumnAlign
{
    Left,
    Center,
    Right
}
