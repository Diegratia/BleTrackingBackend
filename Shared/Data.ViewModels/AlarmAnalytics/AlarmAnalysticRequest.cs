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
}
