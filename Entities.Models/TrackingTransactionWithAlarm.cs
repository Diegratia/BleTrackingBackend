namespace Entities.Models
{
    public class TrackingTransactionWithAlarm
    {
        public TrackingTransaction Tracking { get; set; }
        public AlarmRecordTracking? AlarmRecord { get; set; }
    }
}
