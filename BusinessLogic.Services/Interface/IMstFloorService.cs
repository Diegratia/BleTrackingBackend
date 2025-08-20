using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Interface
{
    public interface IMstFloorService
    {
        Task<MstFloorDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MstFloorDto>> GetAllAsync();
        Task<MstFloorDto> CreateAsync(MstFloorCreateDto createDto);
        Task UpdateAsync(Guid id, MstFloorUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<MstFloorDto>> ImportAsync(IFormFile file);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}