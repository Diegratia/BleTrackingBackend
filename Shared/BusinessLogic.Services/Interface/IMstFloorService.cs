using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IMstFloorService
    {
        Task<MstFloorRead> GetByIdAsync(Guid id);
        Task<IEnumerable<MstFloorRead>> GetAllAsync();
        Task<IEnumerable<MstFloorRead>> OpenGetAllAsync();
        Task<MstFloorRead> CreateAsync(MstFloorCreateDto createDto);
        Task UpdateAsync(Guid id, MstFloorUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task RemoveGroupAsync();
        Task<IEnumerable<MstFloorRead>> ImportAsync(IFormFile file);
        Task<object> FilterAsync(DataTablesProjectedRequest request, MstFloorFilter filter);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task CascadeDeleteAsync(Guid id);
    }
}
