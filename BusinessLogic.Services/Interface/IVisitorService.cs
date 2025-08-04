using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IVisitorService
    {
        Task<VisitorDto> CreateVisitorAsync(VisitorCreateDto createDto);
        Task<VisitorDto> GetVisitorByIdAsync(Guid id);
        Task<IEnumerable<VisitorDto>> GetAllVisitorsAsync();
        Task<VisitorDto> UpdateVisitorAsync(Guid id, VisitorUpdateDto updateDto);
        Task DeleteVisitorAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task ConfirmVisitorEmailAsync(ConfirmEmailDto confirmDto);
    }
}