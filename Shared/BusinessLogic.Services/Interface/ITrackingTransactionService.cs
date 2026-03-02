using Data.ViewModels;
using Shared.Contracts;
using Shared.Contracts.Read;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interface
{
    public interface ITrackingTransactionService
    {
        Task<List<TrackingTransactionRead>> GetAllAsync();
        Task<TrackingTransactionRead?> GetByIdAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, TrackingTransactionFilter filter);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}
