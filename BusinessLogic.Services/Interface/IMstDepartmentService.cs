using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Interface
{
    public interface IMstDepartmentService
    {
        Task<MstDepartmentDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MstDepartmentDto>> GetAllAsync();
        Task<IEnumerable<OpenMstDepartmentDto>> OpenGetAllAsync();
        Task<MstDepartmentDto> CreateAsync(MstDepartmentCreateDto createDto);
        Task UpdateAsync(Guid id, MstDepartmentUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task<IEnumerable<MstDepartmentDto>> ImportAsync(IFormFile file);
    }
}