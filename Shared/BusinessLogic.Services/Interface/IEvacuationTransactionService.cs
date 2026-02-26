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
    }
}
