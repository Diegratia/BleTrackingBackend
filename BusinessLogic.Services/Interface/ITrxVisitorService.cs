using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Data.ViewModels.Dto.Helpers.MinimalDto;

namespace BusinessLogic.Services.Interface
{
    public interface ITrxVisitorService
    {
        Task<TrxVisitorDto> CreateTrxVisitorAsync(TrxVisitorCreateDto createDto);
        Task<TrxVisitorDto> GetTrxVisitorByIdAsync(Guid id);
        Task<IEnumerable<TrxVisitorDto>> GetAllTrxVisitorsAsync();
        Task<IEnumerable<TrxVisitorDtoz>> GetAllTrxVisitorsAsyncMinimal();
         Task<IEnumerable<OpenTrxVisitorDto>> OpenGetAllTrxVisitorsAsync();
        Task UpdateTrxVisitorAsync(Guid id, TrxVisitorUpdateDto updateDto);
        // Task DeleteTrxVisitorAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<object> MinimalFilterAsync(DataTablesRequest request);
        Task<object> FilterRawAsync(DataTablesRequest request);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        // Task CheckinVisitorAsync(Guid trxVisitorId);
        // Task CheckinVisitorAsync(Guid trxVisitorId, Guid cardId);
        Task CheckinVisitorAsync(TrxVisitorCheckinDto dto);
        Task CheckoutVisitorAsync(Guid trxVisitorId);
        Task DeniedVisitorAsync(Guid trxVisitorId, DenyReasonDto denyReasonDto);
        Task BlockVisitorAsync(Guid trxVisitorId, BlockReasonDto blockVisitorDto);
        Task UnblockVisitorAsync(Guid trxVisitorId);
        Task<TrxVisitorDto> GetTrxVisitorByPublicIdAsync(Guid id);
    }
}