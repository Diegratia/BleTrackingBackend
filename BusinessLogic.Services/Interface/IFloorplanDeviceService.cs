using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Interface
{
    public interface IFloorplanDeviceService
    {
        Task<FloorplanDeviceDto> CreateAsync(FloorplanDeviceCreateDto dto);
        Task<FloorplanDeviceDto> GetByIdAsync(Guid id);
        Task<IEnumerable<FloorplanDeviceDto>> GetAllAsync();
        Task<IEnumerable<OpenFloorplanDeviceDto>> OpenGetAllAsync();
        Task UpdateAsync(Guid Id, FloorplanDeviceUpdateDto dto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<FloorplanDeviceDto>> ImportAsync(IFormFile file);
        Task<object> FilterAsync(DataTablesRequest request); 
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}