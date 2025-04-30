using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface ITrackingTransactionService
    {
        Task<IEnumerable<TrackingTransactionDto>> GetAllTrackingTransactionsAsync();
        Task<TrackingTransactionDto> CreateTrackingTransactionAsync(TrackingTransactionCreateDto dto);
        Task<TrackingTransactionDto> GetTrackingTransactionByIdAsync(Guid id);
        Task UpdateTrackingTransactionAsync(Guid id, TrackingTransactionUpdateDto dto);
        Task DeleteTrackingTransactionAsync(Guid id);
    }
}