using System;

namespace Repositories.Repository.RepoModel
{
    // 🧾 Input request (digunakan oleh semua summary)
    // public class AlarmAnalyticsRequestRM
    // {
    //     public DateTime? From { get; set; }
    //     public DateTime? To { get; set; }
    //     public string? TimeRange { get; set; }  // ← tambahkan ini
    //     public Guid? FloorplanMaskedAreaId { get; set; }
    //     public string? OperatorName { get; set; }
    //     public Guid? VisitorId { get; set; }
    //     public Guid? BuildingId { get; set; }
    //     public Guid? FloorId { get; set; }
    // }

        public class AlarmAnalyticsRequestRM
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? TimeRange { get; set; }
        public Guid? BuildingId { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public Guid? AreaId { get; set; }
        public Guid? CardId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? ReaderId { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public string? OperatorName { get; set; }
        public bool? IsActive { get; set; }

        public string? ReportTitle { get; set; }
        public string? ExportType { get; set; }

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

    public class AlarmRecordLog
    {
        public string Visitor { get; set; }
        public string Area { get; set; }
        public DateTime? TriggeredAt { get; set; }
        public DateTime? DoneAt { get; set; }
        public string Status { get; set; }
        public string Host { get; set; }
        public string Category { get; set; }
    }
    
    public class AlarmTriggerLogFlatRM
{
    public Guid? VisitorId { get; set; }
    public string? VisitorName { get; set; }

    public Guid? BuildingId { get; set; }
    public string? BuildingName { get; set; }

    public Guid? FloorId { get; set; }
    public string? FloorName { get; set; }

    public Guid? FloorplanId { get; set; }
    public string? FloorplanName { get; set; }

    public string? AlarmStatus { get; set; }
    public string? ActionStatus { get; set; }

    public string? InvestigatedResult { get; set; }

    public DateTime? TriggeredAt { get; set; }
    public DateTime? DoneAt { get; set; }
    public DateTime? LastNotifiedAt { get; set; }

        // public double? IdleDurationMinutes { get; set; }
    public string? AssignedSecurityName { get; set; }
    public double? HandleDurationMinutes { get; set; }

    public bool? IsActive { get; set; }
}

    
    public class AlarmLogResponseRM
    {
        public List<AlarmRecordLog> Data { get; set; } = new();
    }



}
