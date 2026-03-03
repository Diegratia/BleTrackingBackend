using Data.ViewModels;
using Shared.Contracts;
using Shared.Contracts.Read;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interface
{
    public interface IMstIntegrationService
    {
        Task<IEnumerable<MstIntegrationRead>> GetAllAsync();
        Task<MstIntegrationRead?> GetByIdAsync(Guid id);
        Task<MstIntegrationDto> CreateAsync(MstIntegrationCreateDto createDto);
        Task<MstIntegrationDto> CreateRawAsync(MstIntegrationCreateDto createDto);
        Task UpdateAsync(Guid id, MstIntegrationUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, MstIntegrationFilter filter);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}
