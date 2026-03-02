using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using DataView;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IMstAccessCctvService
    {
        Task<MstAccessCctvRead> GetByIdAsync(Guid id);
        Task<IEnumerable<MstAccessCctvRead>> GetAllAsync();
        Task<IEnumerable<MstAccessCctvRead>> GetAllUnassignedAsync();
        Task<IEnumerable<OpenMstAccessCctvDto>> OpenGetAllAsync();
        Task<MstAccessCctvDto> CreateAsync(MstAccessCctvCreateDto createDto);
        Task UpdateAsync(Guid id, MstAccessCctvUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, MstAccessCctvFilter filter);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}