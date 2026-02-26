using Shared.Contracts;

namespace Shared.Contracts.Read
{
    public class EvacuationAlertRead : BaseRead
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public EvacuationAlertStatus AlertStatus { get; set; }
        public EvacuationTriggerType TriggerType { get; set; }
        public string? TriggeredBy { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CompletionNotes { get; set; }
        public string? CompletedBy { get; set; }
        public int TotalRequired { get; set; }
        public int TotalEvacuated { get; set; }
        public int TotalConfirmed { get; set; }
        public int TotalRemaining { get; set; }
    }
}
