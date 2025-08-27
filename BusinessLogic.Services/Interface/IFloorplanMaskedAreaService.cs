using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Interface
{
    public interface IFloorplanMaskedAreaService
    {
        Task<FloorplanMaskedAreaDto> GetByIdAsync(Guid id);
        Task<IEnumerable<FloorplanMaskedAreaDto>> GetAllAsync();
        Task<IEnumerable<OpenFloorplanMaskedAreaDto>> OpenGetAllAsync();
        Task<FloorplanMaskedAreaDto> CreateAsync(FloorplanMaskedAreaCreateDto createDto);
        Task UpdateAsync(Guid id, FloorplanMaskedAreaUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<FloorplanMaskedAreaDto>> ImportAsync(IFormFile file);
        Task<object> FilterAsync(DataTablesRequest request); 
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}