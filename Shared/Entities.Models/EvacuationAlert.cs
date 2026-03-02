using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;
using Shared.Contracts;

namespace Entities.Models
{
    [Table("evacuation_alerts")]
    public class EvacuationAlert : BaseModelWithTime, IApplicationEntity
    {
        [Column("title")]
        public string? Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("alert_status")]
        public EvacuationAlertStatus AlertStatus { get; set; } = EvacuationAlertStatus.Active;

        [Column("trigger_type")]
        public EvacuationTriggerType TriggerType { get; set; } = EvacuationTriggerType.Manual;

        [Column("triggered_by")]
        public string? TriggeredBy { get; set; }

        [Column("started_at")]
        public DateTime? StartedAt { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("completion_notes")]
        public string? CompletionNotes { get; set; }

        [Column("completed_by")]
        public string? CompletedBy { get; set; }

        [Column("total_required")]
        public int TotalRequired { get; set; } = 0;

        [Column("total_evacuated")]
        public int TotalEvacuated { get; set; } = 0;

        [Column("total_confirmed")]
        public int TotalConfirmed { get; set; } = 0;

        [Column("total_remaining")]
        public int TotalRemaining { get; set; } = 0;

        [Column("status")]
        public int Status { get; set; } = 1;

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public ICollection<EvacuationTransaction> Transactions { get; set; } = new List<EvacuationTransaction>();
    }
}
