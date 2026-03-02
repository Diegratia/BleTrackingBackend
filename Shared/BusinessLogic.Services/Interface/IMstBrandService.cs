using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IMstBrandService
    {
        Task<MstBrandRead> GetByIdAsync(Guid id);
        Task<IEnumerable<MstBrandRead>> GetAllAsync();
        Task<IEnumerable<MstBrandRead>> OpenGetAllAsync();
        Task<MstBrandRead> CreateAsync(MstBrandCreateDto createDto);
        Task<MstBrandRead> CreateInternalAsync(MstBrandCreateDto createDto);
        Task UpdateAsync(Guid id, MstBrandUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, MstBrandFilter filter);
        Task<byte[]> ExportPdfAsync();
        Task<byte[]> ExportExcelAsync();
    }
}


