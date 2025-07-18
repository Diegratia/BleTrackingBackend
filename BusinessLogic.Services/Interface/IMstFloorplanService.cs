using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;


namespace BusinessLogic.Services.Interface
{
    public interface IMstFloorplanService
    {
        Task<MstFloorplanDto> CreateAsync(MstFloorplanCreateDto dto);
        Task<MstFloorplanDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MstFloorplanDto>> GetAllAsync();
        Task UpdateAsync(Guid Id, MstFloorplanUpdateDto dto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<MstFloorplanDto>> ImportAsync(IFormFile file);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}