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
    public interface IMstFloorplanService
    {
        Task<MstFloorplanRead> CreateAsync(MstFloorplanCreateDto dto);
        Task<MstFloorplanRead> GetByIdAsync(Guid id);
        Task<IEnumerable<MstFloorplanRead>> GetAllAsync();
        Task<IEnumerable<MstFloorplanRead>> OpenGetAllAsync();
        Task UpdateAsync(Guid Id, MstFloorplanUpdateDto dto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<MstFloorplanRead>> ImportAsync(IFormFile file);
        Task<object> FilterAsync(DataTablesProjectedRequest request, MstFloorplanFilter filter);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task RemoveGroupAsync();
        Task CascadeDeleteAsync(Guid id);
    }
}
