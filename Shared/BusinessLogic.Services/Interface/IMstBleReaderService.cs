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
    public interface IMstBleReaderService
    {
        Task<MstBleReaderRead> GetByIdAsync(Guid id);
        Task<IEnumerable<MstBleReaderRead>> GetAllAsync();
        Task<IEnumerable<MstBleReaderRead>> GetAllUnassignedAsync();
        Task<IEnumerable<OpenMstBleReaderDto>> OpenGetAllAsync();
        Task<MstBleReaderRead> CreateAsync(MstBleReaderCreateDto createDto);
        Task UpdateAsync(Guid id, MstBleReaderUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<MstBleReaderRead>> ImportAsync(IFormFile file);
        Task<object> FilterAsync(DataTablesProjectedRequest request, MstBleReaderFilter filter);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}
