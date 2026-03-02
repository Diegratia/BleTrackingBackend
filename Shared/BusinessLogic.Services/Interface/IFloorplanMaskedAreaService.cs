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
    public interface IFloorplanMaskedAreaService
    {
        Task<FloorplanMaskedAreaRead> GetByIdAsync(Guid id);
        Task<IEnumerable<FloorplanMaskedAreaRead>> GetAllAsync();
        Task<IEnumerable<OpenFloorplanMaskedAreaDto>> OpenGetAllAsync();
        Task<FloorplanMaskedAreaRead> CreateAsync(FloorplanMaskedAreaCreateDto createDto);
        Task UpdateAsync(Guid id, FloorplanMaskedAreaUpdateDto updateDto);
        // Task DeleteAsync(Guid id);
        Task SoftDeleteAsync(Guid id);
        // Task<IEnumerable<FloorplanMaskedAreaDto>> ImportAsync(IFormFile file);
        Task<object> FilterAsync(DataTablesProjectedRequest request, FloorplanMaskedAreaFilter filter);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task RemoveGroupAsync();
        Task CascadeDeleteAsync(Guid id, string username);
    }
}
