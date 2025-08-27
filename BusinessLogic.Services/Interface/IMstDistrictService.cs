using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IMstDistrictService
    {
        Task<MstDistrictDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MstDistrictDto>> GetAllAsync();
        Task<IEnumerable<OpenMstDistrictDto>> OpenGetAllAsync();
        Task<MstDistrictDto> CreateAsync(MstDistrictCreateDto createDto);
        Task<List<MstDistrictDto>> CreateBatchAsync(List<MstDistrictCreateDto> dtos);
        Task UpdateAsync(Guid id, MstDistrictUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}