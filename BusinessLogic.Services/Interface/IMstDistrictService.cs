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
        Task<MstDistrictDto> CreateAsync(MstDistrictCreateDto createDto);
        Task UpdateAsync(Guid id, MstDistrictUpdateDto updateDto);
        Task DeleteAsync(Guid id);
    }
}