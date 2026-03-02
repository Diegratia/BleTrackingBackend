using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IMstIntegrationService
    {
        Task<MstIntegrationDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MstIntegrationDto>> GetAllAsync();
        Task<MstIntegrationDto> CreateAsync(MstIntegrationCreateDto createDto);
        Task UpdateAsync(Guid id, MstIntegrationUpdateDto updateDto); // Returns Task
        Task DeleteAsync(Guid id); // Returns Task
        Task<object> FilterAsync(DataTablesRequest request); 
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}