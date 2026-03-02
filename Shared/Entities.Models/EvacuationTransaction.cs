using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;
using Shared.Contracts;

namespace Entities.Models
{
    [Table("evacuation_transactions")]
    public class EvacuationTransaction : BaseModelWithTime, IApplicationEntity
    {
        [Required]
        [ForeignKey(nameof(EvacuationAlert))]
        [Column("evacuation_alert_id")]
        public Guid EvacuationAlertId { get; set; }

        [Required]
        [ForeignKey(nameof(EvacuationAssemblyPoint))]
        [Column("evacuation_assembly_point_id")]
        public Guid EvacuationAssemblyPointId { get; set; }

        [Column("person_category")]
        public EvacuationPersonCategory PersonCategory { get; set; }

        [ForeignKey(nameof(Member))]
        [Column("member_id")]
        public Guid? MemberId { get; set; }

        [ForeignKey(nameof(Visitor))]
        [Column("visitor_id")]
        public Guid? VisitorId { get; set; }

        [ForeignKey(nameof(Security))]
        [Column("security_id")]
        public Guid? SecurityId { get; set; }

        [ForeignKey(nameof(Card))]
        [Column("card_id")]
        public Guid? CardId { get; set; }

        [Column("person_status")]
        public EvacuationPersonStatus PersonStatus { get; set; } = EvacuationPersonStatus.Remaining;

        [Required]
        [Column("detected_at")]
        public DateTime DetectedAt { get; set; }

        [Column("last_detected_at")]
        public DateTime? LastDetectedAt { get; set; }

        [Column("confirmed_by")]
        public string? ConfirmedBy { get; set; }

        [Column("confirmed_at")]
        public DateTime? ConfirmedAt { get; set; }

        [Column("confirmation_notes")]
        public string? ConfirmationNotes { get; set; }

        [Column("status")]
        public int Status { get; set; } = 1;

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public EvacuationAlert EvacuationAlert { get; set; } = null!;
        public EvacuationAssemblyPoint EvacuationAssemblyPoint { get; set; } = null!;
        public MstMember? Member { get; set; }
        public Visitor? Visitor { get; set; }
        public MstSecurity? Security { get; set; }
        public Card? Card { get; set; }
    }
}
