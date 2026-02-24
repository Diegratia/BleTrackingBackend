using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IEvacuationTransactionService
    {
        Task<EvacuationTransactionRead> GetByIdAsync(Guid id);
        Task<IEnumerable<EvacuationTransactionRead>> GetAllAsync();
        Task<EvacuationTransactionRead> GetByAlertIdAsync(Guid alertId);
        Task<object> FilterAsync(DataTablesProjectedRequest request, EvacuationTransactionFilter filter);

        // Confirmation
        Task ConfirmAsync(Guid transactionId, EvacuationTransactionConfirmDto confirmDto);

        // Engine integration
        Task ProcessDetectionAsync(EvacuationDetectionDto detectionDto);
        Task UpdateSummaryAsync(Guid alertId, EvacuationSummaryUpdateDto summaryDto);
    }

    public class EvacuationDetectionDto
    {
        public Guid EvacuationAlertId { get; set; }
        public Guid AssemblyPointId { get; set; }
        public DateTime DetectedAt { get; set; }
        public List<PersonDetectionDto> Persons { get; set; } = new();
    }

    public class PersonDetectionDto
    {
        public EvacuationPersonCategory PersonCategory { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? SecurityId { get; set; }
        public Guid? CardId { get; set; }
    }

    public class EvacuationSummaryUpdateDto
    {
        public int TotalRequired { get; set; }
        public int TotalEvacuated { get; set; }
        public int TotalConfirmed { get; set; }
        public int TotalRemaining { get; set; }
    }
}
