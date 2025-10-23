using System;

namespace Data.ViewModels.AlarmAnalytics
{
    public class AlarmAnalyticsDto
    {
        public Guid Id { get; set; }
        public DateTime? Timestamp { get; set; }
        public VisitorDto? Visitor { get; set; }
        public MstBleReaderDto? Reader { get; set; }
        public FloorplanMaskedAreaDto? FloorplanMaskedArea { get; set; }
        public string? AlarmRecordStatus { get; set; }
        public string? ActionStatus { get; set; }
    }

    public class AlarmAnalyticsRequest
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public string? OperatorName { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? BuildingId { get; set; }
        public Guid? FloorId { get; set; }
    }

    // 📅 Harian
    public class AlarmDailySummaryVM
    {
        public DateTime Date { get; set; }
        public int Total { get; set; }
        public int Done { get; set; }
        public int Cancelled { get; set; }
        public double? AvgResponseSeconds { get; set; }
    }

    // 🗺 Area
    public class AlarmAreaSummaryVM
    {
        public Guid? FloorplanMaskedAreaId { get; set; }
        public string? AreaName { get; set; }
        public int Total { get; set; }
        public int Done { get; set; }
        public double? AvgResponseSeconds { get; set; }
    }

    // 👨‍💻 Operator
    public class AlarmOperatorSummaryVM
    {
        public string OperatorName { get; set; } = string.Empty;
        public int TotalHandled { get; set; }
        public double? AvgResponseSeconds { get; set; }
    }

    // 🏢 Building
    public class AlarmBuildingSummaryVM
    {
        public Guid? BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public int Total { get; set; }
        public int Done { get; set; }
        public double? AvgResponseSeconds { get; set; }
    }

    // 🧍 Visitor
    public class AlarmVisitorSummaryVM
    {
        public Guid? VisitorId { get; set; }
        public string? VisitorName { get; set; }
        public int TotalTriggered { get; set; }
        public int Done { get; set; }
    }

    // 🧱 Floor
    public class AlarmFloorSummaryVM
    {
        public Guid? FloorId { get; set; }
        public string? FloorName { get; set; }
        public int Total { get; set; }
        public int Done { get; set; }
        public double? AvgResponseSeconds { get; set; }
    }

    // 📊 Status
    public class AlarmStatusSummaryVM
    {
        public string Status { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    // ⏱ Duration / Histogram
    public class AlarmDurationSummaryVM
    {
        public string DurationRange { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    // 🕐 Time of Day
    public class AlarmTimeOfDaySummaryVM
    {
        public int Hour { get; set; }
        public int Total { get; set; }
    }

    // 📆 Weekly Trend
    public class AlarmWeeklyTrendVM
    {
        public string DayOfWeek { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    // 📈 Trend by Action
    public class AlarmTrendByActionVM
    {
        public DateTime Date { get; set; }
        public string ActionStatus { get; set; } = string.Empty;
        public int Total { get; set; }
    }
}
