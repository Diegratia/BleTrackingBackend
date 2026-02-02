using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Shared.Contracts;

namespace BusinessLogic.Services.Interface
{
    public interface ICardSwapTransactionService
    {
        // CRUD Operations
        Task<IEnumerable<CardSwapTransactionRead>> GetAllAsync();
        Task<CardSwapTransactionRead?> GetByIdAsync(Guid id);
        Task<CardSwapTransactionRead> CreateAsync(CardSwapTransactionCreateDto dto);
        
        // DataTables Filtering
        Task<object> FilterAsync(DataTablesProjectedRequest request, CardSwapTransactionFilter filter);
        
        // Business Logic Operations
        Task<CardSwapTransactionRead?> GetLastActiveSwapAsync(Guid visitorId, Guid swapChainId);
        Task<CardSwapTransactionRead> PerformForwardSwapAsync(ForwardSwapRequest request);
        Task<CardSwapTransactionRead> PerformReverseSwapAsync(ReverseSwapRequest request);
        Task<bool> CanReverseSwapAsync(Guid visitorId, Guid swapChainId);
        Task<IEnumerable<CardSwapTransactionRead>> GetActiveSwapsByChainAsync(Guid swapChainId);
    }
}
