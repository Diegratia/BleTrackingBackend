using System;

namespace Repositories.Repository.RepoModel
{
    public class RawAlarmIncidentDurationRM
    {
        public string AlarmStatus { get; set; } = string.Empty;
        public DateTime? TriggerTime { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public DateTime? DispatchedAt { get; set; }
        public DateTime? WaitingTimestamp { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? ArrivedAt { get; set; }
        public DateTime? InvestigatedDoneAt { get; set; }
        public DateTime? DoneTimestamp { get; set; }
        public DateTime? CancelTimestamp { get; set; }
    }
}
