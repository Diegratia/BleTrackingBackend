using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IFloorplanDeviceService
    {
        Task<FloorplanDeviceDto> CreateAsync(FloorplanDeviceCreateDto dto);
        Task<FloorplanDeviceDto> GetByIdAsync(Guid id);
        Task<IEnumerable<FloorplanDeviceDto>> GetAllAsync();
        Task UpdateAsync(Guid Id, FloorplanDeviceUpdateDto dto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}