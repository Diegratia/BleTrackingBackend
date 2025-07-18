using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IMstAccessCctvService
    {
        Task<MstAccessCctvDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MstAccessCctvDto>> GetAllAsync();
        Task<MstAccessCctvDto> CreateAsync(MstAccessCctvCreateDto createDto);
        Task UpdateAsync(Guid id, MstAccessCctvUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}