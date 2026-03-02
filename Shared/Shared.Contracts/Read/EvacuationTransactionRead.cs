using Shared.Contracts;

namespace Shared.Contracts.Read
{
    public class EvacuationTransactionRead
    {
        public Guid Id { get; set; }
        public Guid EvacuationAlertId { get; set; }
        public Guid EvacuationAssemblyPointId { get; set; }
        public EvacuationPersonCategory PersonCategory { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? SecurityId { get; set; }
        public Guid? CardId { get; set; }
        public EvacuationPersonStatus PersonStatus { get; set; }
        public DateTime DetectedAt { get; set; }
        public DateTime? LastDetectedAt { get; set; }
        public string? ConfirmedBy { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string? ConfirmationNotes { get; set; }
        public Guid ApplicationId { get; set; }

        // Audit fields from BaseModelWithTime
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Status { get; set; }

        // Navigation properties (join ke Member/Visitor/Security, bukan snapshot)
        public string? AssemblyPointName { get; set; }
        public string? MemberName { get; set; }
        public string? VisitorName { get; set; }
        public string? SecurityName { get; set; }
        public string? CardNumber { get; set; }
    }
}
