using Shared.Contracts;

namespace Data.ViewModels
{
    // EvacuationAssemblyPoint DTOs
    public class EvacuationAssemblyPointCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
        public string? Remarks { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public int IsActive { get; set; } = 1;
    }

    public class EvacuationAssemblyPointUpdateDto
    {
        public string? Name { get; set; }
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
        public string? Remarks { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public int? IsActive { get; set; }
    }

    // EvacuationAlert DTOs
    public class EvacuationAlertCreateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public EvacuationTriggerType TriggerType { get; set; } = EvacuationTriggerType.Manual;
    }

    public class EvacuationAlertUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public EvacuationAlertStatus? AlertStatus { get; set; }
    }

    // EvacuationTransaction DTOs
    public class EvacuationTransactionConfirmDto
    {
        public EvacuationPersonStatus PersonStatus { get; set; }
        public string? ConfirmationNotes { get; set; }
    }

    // Evacuation Summary DTOs
    public class EvacuationSummaryDto
    {
        public Guid EvacuationAlertId { get; set; }
        public string? Title { get; set; }
        public EvacuationAlertStatus AlertStatus { get; set; }
        public DateTime? StartedAt { get; set; }
        public int TotalRequired { get; set; }
        public int TotalEvacuated { get; set; }
        public int TotalConfirmed { get; set; }
        public int TotalSafe { get; set; }
        public int TotalRemaining { get; set; }
        public List<AssemblyPointSummaryDto> ByAssemblyPoint { get; set; } = new();
    }

    public class AssemblyPointSummaryDto
    {
        public Guid AssemblyPointId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Evacuated { get; set; }
        public int Confirmed { get; set; }
    }

    // Evacuation Person Status DTO
    public class EvacuationPersonStatusDto
    {
        public Guid TransactionId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? SecurityId { get; set; }
        public string? PersonName { get; set; }
        public string? PersonIdentity { get; set; }
        public EvacuationPersonCategory PersonCategory { get; set; }
        public EvacuationPersonStatus PersonStatus { get; set; }
        public Guid? AssemblyPointId { get; set; }
        public string? AssemblyPointName { get; set; }
        public DateTime? DetectedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string? ConfirmedBy { get; set; }
    }
}
