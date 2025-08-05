using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface ITrxVisitorService
    {
        Task<TrxVisitorDto> CreateTrxVisitorAsync(TrxVisitorCreateDto createDto);
        Task<TrxVisitorDto> GetTrxVisitorByIdAsync(Guid id);
        Task<IEnumerable<TrxVisitorDto>> GetAllTrxVisitorsAsync();
        Task UpdateTrxVisitorAsync(Guid id, TrxVisitorUpdateDto updateDto);
        // Task DeleteTrxVisitorAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task CheckinVisitorAsync(Guid visitorId);
        Task CheckoutVisitorAsync(Guid visitorId);
        Task DeniedVisitorAsync(Guid visitorId, DenyReasonDto denyReasonDto);
        Task BlockVisitorAsync(Guid visitorId, BlockReasonDto blockVisitorDto);
        Task UnblockVisitorAsync(Guid visitorId);
    }
}