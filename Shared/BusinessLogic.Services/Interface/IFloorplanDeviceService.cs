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
    public interface IFloorplanDeviceService
    {
        Task<FloorplanDeviceRead> CreateAsync(FloorplanDeviceCreateDto dto);
        Task<FloorplanDeviceRead> GetByIdAsync(Guid id);
        Task<IEnumerable<FloorplanDeviceRead>> GetAllAsync();
        Task<IEnumerable<OpenFloorplanDeviceDto>> OpenGetAllAsync();
        Task UpdateAsync(Guid Id, FloorplanDeviceUpdateDto dto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<FloorplanDeviceRead>> ImportAsync(IFormFile file);
        Task<object> FilterAsync(DataTablesProjectedRequest request, FloorplanDeviceFilter filter);
        Task SetDeviceAssignmentAsync(Guid? readerId, Guid? cctvId, Guid? controlId, bool isAssigned, string username);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task RemoveGroupAsync();
        Task CascadeDeleteAsync(Guid id, string username);
    }
}
