using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IMstFloorService
    {
        Task<MstFloorDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MstFloorDto>> GetAllAsync();
        Task<MstFloorDto> CreateAsync(MstFloorCreateDto createDto);
        Task<MstFloorDto> UpdateAsync(Guid id, MstFloorUpdateDto updateDto);
        Task DeleteAsync(Guid id);
    }
}