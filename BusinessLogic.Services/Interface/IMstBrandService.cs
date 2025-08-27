using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IMstBrandService
    {
        Task<MstBrandDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MstBrandDto>> GetAllAsync();
        Task<IEnumerable<OpenMstBrandDto>> OpenGetAllAsync();
        Task<MstBrandDto> CreateAsync(MstBrandCreateDto createDto);
        Task UpdateAsync(Guid id, MstBrandUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}


