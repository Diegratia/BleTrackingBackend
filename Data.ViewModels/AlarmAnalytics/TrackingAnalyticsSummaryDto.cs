
namespace Data.ViewModels.AlarmAnalytics
{
    public class TrackingTransactionDto
    {
        public Guid Id { get; set; }
        public DateTime? TransTime { get; set; }
        public Guid? ReaderId { get; set; }
        public string? ReaderName { get; set; }
        public Guid? VisitorId { get; set; }
        public string? VisitorName { get; set; }
        public Guid? MemberId { get; set; }
        public string? MemberName { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public string? FloorplanMaskedAreaName { get; set; }
        public string? AreaShape { get; set; }
        public float? CoordinateX { get; set; }
        public float? CoordinateY { get; set; }
        public string AlarmStatus { get; set; }
        public string ActionStatus { get; set; }
        public string AlarmRecprdStatus { get; set; }
        public long? Battery { get; set; }
    }

    public class TrackingAnalyticsRequestDto
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? TimeRange { get; set; }

        public Guid? BuildingId { get; set; }
        public Guid? FloorId { get; set; }
        public Guid? AreaId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? ReaderId { get; set; }
    }

    public class TrackingAreaSummaryDto
    {
        public Guid? AreaId { get; set; }
        public string? AreaName { get; set; }
        public int TotalRecords { get; set; }
    }

    public class TrackingPermissionCountDto
    {
        public int WithPermission { get; set; }
        public int WithoutPermission { get; set; }
        public int TotalRecords { get; set; }

    }

    public class TrackingAccessPermissionSummaryDto
    {
        public int AccessedAreaTotal { get; set; }
        public int WithPermission { get; set; }
        public int WithoutPermission { get; set; }
    }

    public class AreaAccessAggregateRow
    {
        public DateTime Date { get; set; }
        public string RestrictedStatus { get; set; } // restrict / non-restrict
        public int Total { get; set; }
    }


    public class TrackingDailySummaryDto
    {
        public DateTime Date { get; set; }
        public int TotalRecords { get; set; }
    }

    public class TrackingBuildingSummaryDto
    {
        public Guid? BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public int TotalRecords { get; set; }
    }

    public class TrackingVisitorSummaryDto
    {
        public Guid? VisitorId { get; set; }
        public string? VisitorName { get; set; }
        public int TotalRecords { get; set; }
    }

    public class TrackingReaderSummaryDto
    {
        public Guid? ReaderId { get; set; }
        public string? ReaderName { get; set; }
        public int TotalRecords { get; set; }
    }
    
        public class AreaAccessChartDto
    {
        public List<string> Labels { get; set; }
        public List<ChartSeriesDto> Series { get; set; }
    }

    public class ChartSeriesDto
    {
        public string Name { get; set; }
        public List<int> Data { get; set; }
    }

    public class AreaAccessSummaryDto
    {
        public int AccessedAreaTotal { get; set; }
        public int WithPermission { get; set; }
        public int WithoutPermission { get; set; }
    }

    public class AreaAccessResponseDto
    {
        public AreaAccessSummaryDto Summary { get; set; }
        public AreaAccessChartDto Chart { get; set; }
    }


}
