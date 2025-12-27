namespace Data.ViewModels
{
    public class TrackingTransactionWithAlarmDto
    {
        public TrackingTransactionDto Tracking { get; set; }
        public AlarmRecordTrackingDto? AlarmRecord { get; set; }
    }
}
