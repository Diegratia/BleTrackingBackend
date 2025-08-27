using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

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
        Task<object> FilterAsync(DataTablesRequest request); 
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}