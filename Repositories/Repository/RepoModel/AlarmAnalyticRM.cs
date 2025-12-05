using System;

namespace Repositories.Repository.RepoModel
{
    // 🧾 Input request (digunakan oleh semua summary)
    public class AlarmAnalyticsRequestRM
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? TimeRange { get; set; }  // ← tambahkan ini
        public Guid? FloorplanMaskedAreaId { get; set; }
        public string? OperatorName { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? BuildingId { get; set; }
        public Guid? FloorId { get; set; }
    }

    // 📊 Output result models (strongly typed per summary)
    public class AlarmAreaSummaryRM
    {
        public Guid? AreaId { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public string AlarmStatus { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    public class AlarmVisitorSummaryRM
    {
        public Guid? VisitorId { get; set; }
        public string VisitorName { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    public class AlarmBuildingSummaryRM
    {
        public Guid? BuildingId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    public class AlarmDailySummaryRM
    {
        public DateTime Date { get; set; }
        public int Total { get; set; }
    }

    public class AlarmStatusSummaryRM
    {
        public string Status { get; set; } = string.Empty;
        public int Total { get; set; }
    }
    
        public class AlarmHourlyStatusSummaryRM
    {
        public int Hour { get; set; }
        public string HourLabel { get; set; } = string.Empty;
        public Dictionary<string, int> Status { get; set; } = new();
        // public int Total { get; set; }
    }

}
