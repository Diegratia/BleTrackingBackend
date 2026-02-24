using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IEvacuationAlertService
    {
        Task<EvacuationAlertRead> GetByIdAsync(Guid id);
        Task<IEnumerable<EvacuationAlertRead>> GetAllAsync();
        Task<EvacuationAlertRead> CreateAsync(EvacuationAlertCreateDto createDto);
        Task UpdateAsync(Guid id, EvacuationAlertUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, EvacuationAlertFilter filter);

        // Evacuation actions
        Task<EvacuationAlertRead> StartAsync(Guid id);
        Task<EvacuationAlertRead> PauseAsync(Guid id);
        Task<EvacuationAlertRead> CompleteAsync(Guid id, string? completionNotes);
        Task<EvacuationAlertRead> CancelAsync(Guid id);

        // Summary and status
        Task<EvacuationSummaryDto> GetSummaryAsync(Guid id);
        Task<List<EvacuationPersonStatusDto>> GetPersonStatusAsync(Guid id);

        // Internal method for updating counters
        Task UpdateAlertCountersAsync(Guid alertId, int totalRequired, int totalEvacuated, int totalConfirmed, int totalRemaining);
    }
}
