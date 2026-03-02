using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Shared.Contracts;

namespace BusinessLogic.Services.Interface
{
    public interface IMstOrganizationService
    {
        Task<IEnumerable<MstOrganizationDto>> GetAllOrganizationsAsync();
        Task<IEnumerable<OpenMstOrganizationDto>> OpenGetAllOrganizationsAsync();
        Task<MstOrganizationDto> GetOrganizationByIdAsync(Guid id);
        Task<MstOrganizationDto> CreateOrganizationAsync(MstOrganizationCreateDto createDto);
        Task UpdateOrganizationAsync(Guid id, MstOrganizationUpdateDto updateDto);
        Task DeleteOrganizationAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, MstOrganizationFilter filter);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        Task<IEnumerable<MstOrganizationDto>> ImportAsync(IFormFile file);
    }
}