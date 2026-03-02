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
    public interface IMstDepartmentService
    {
        Task<MstDepartmentRead> GetByIdAsync(Guid id);
        Task<IEnumerable<MstDepartmentRead>> GetAllAsync();
        Task<IEnumerable<MstDepartmentRead>> OpenGetAllAsync();
        Task<MstDepartmentRead> CreateAsync(MstDepartmentCreateDto createDto);
        Task UpdateAsync(Guid id, MstDepartmentUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, MstDepartmentFilter filter);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task<IEnumerable<MstDepartmentRead>> ImportAsync(IFormFile file);
    }
}