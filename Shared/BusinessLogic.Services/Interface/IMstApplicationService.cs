using Data.ViewModels;
using Shared.Contracts;
using Shared.Contracts.Read;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interface
{
    public interface IMstApplicationService
    {
        Task<IEnumerable<MstApplicationRead>> GetAllApplicationsAsync();
        Task<MstApplicationRead?> GetApplicationByIdAsync(Guid id);
        Task<MstApplicationDto> CreateApplicationAsync(MstApplicationCreateDto dto);
        Task UpdateApplicationAsync(Guid id, MstApplicationUpdateDto dto);
        Task DeleteApplicationAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, MstApplicationFilter filter);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}
