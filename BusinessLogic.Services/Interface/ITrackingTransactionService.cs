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
        Task<TrackingTransactionDto> GetTrackingTransactionByIdAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}