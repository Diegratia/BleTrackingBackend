using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IBleReaderNodeService
    {
        Task<BleReaderNodeDto> GetByIdAsync(Guid id);
        Task<IEnumerable<BleReaderNodeDto>> GetAllAsync();
        Task<BleReaderNodeDto> CreateAsync(BleReaderNodeCreateDto createDto);
        Task UpdateAsync(Guid id, BleReaderNodeUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync(); 
    }
}