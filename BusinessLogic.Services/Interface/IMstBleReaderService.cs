using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Interface
{
    public interface IMstBleReaderService
    {
        Task<MstBleReaderDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MstBleReaderDto>> GetAllAsync();
        Task<IEnumerable<MstBleReaderDto>> GetAllUnassignedAsync();
        Task<IEnumerable<OpenMstBleReaderDto>> OpenGetAllAsync();
        Task<MstBleReaderDto> CreateAsync(MstBleReaderCreateDto createDto);
        Task UpdateAsync(Guid id, MstBleReaderUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<MstBleReaderDto>> ImportAsync(IFormFile file);
        Task<object> FilterAsync(DataTablesRequest request); 
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}