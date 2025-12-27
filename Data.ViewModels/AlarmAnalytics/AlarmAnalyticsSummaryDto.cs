namespace Data.ViewModels.AlarmAnalytics
{
    public class AlarmAreaSummaryDto
    {
        public Guid? AreaId { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    public class AlarmVisitorSummaryDto
    {
        public Guid? VisitorId { get; set; }
        public string VisitorName { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    public class AlarmBuildingSummaryDto
    {
        public Guid? BuildingId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    public class AlarmDailySummaryDto
    {
        public DateTime Date { get; set; }
        public int Total { get; set; }
    }

    public class AlarmStatusSummaryDto
    {
        public string Status { get; set; } = string.Empty;
        public int Total { get; set; }
    }
}
