using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IMstAccessControlService
    {
        Task<MstAccessControlRead> GetByIdAsync(Guid id);
        Task<IEnumerable<MstAccessControlRead>> GetAllAsync();
        Task<IEnumerable<MstAccessControlRead>> GetAllUnassignedAsync();
        Task<IEnumerable<OpenMstAccessControlDto>> OpenGetAllAsync();
        Task<MstAccessControlRead> CreateAsync(MstAccessControlCreateDto createDto);
        Task UpdateAsync(Guid id, MstAccessControlUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, MstAccessControlFilter filter);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}
