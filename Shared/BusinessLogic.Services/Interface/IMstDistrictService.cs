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
    public interface IMstDistrictService
    {
        Task<MstDistrictRead> GetByIdAsync(Guid id);
        Task<IEnumerable<MstDistrictRead>> GetAllAsync();
        Task<IEnumerable<MstDistrictRead>> OpenGetAllAsync();
        Task<MstDistrictRead> CreateAsync(MstDistrictCreateDto createDto);
        Task<List<MstDistrictRead>> CreateBatchAsync(List<MstDistrictCreateDto> dtos);
        Task UpdateAsync(Guid id, MstDistrictUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, MstDistrictFilter filter);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task<IEnumerable<MstDistrictRead>> ImportAsync(IFormFile file);
    }
}