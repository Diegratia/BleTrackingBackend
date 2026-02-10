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
    public interface IMstBuildingService
    {
        Task<MstBuildingDto> CreateAsync(MstBuildingCreateDto dto);
        Task<MstBuildingDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MstBuildingDto>> GetAllAsync();
        Task<IEnumerable<OpenMstBuildingDto>> OpenGetAllAsync();
        Task<MstBuildingDto> UpdateAsync(Guid id, MstBuildingUpdateDto dto);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<MstBuildingDto>> ImportAsync(IFormFile file);
        Task<object> FilterAsync(DataTablesProjectedRequest request, MstBuildingFilter filter); 
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
        
    }
}