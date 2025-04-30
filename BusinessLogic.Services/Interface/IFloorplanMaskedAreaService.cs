using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IFloorplanMaskedAreaService
    {
        Task<FloorplanMaskedAreaDto> GetByIdAsync(Guid id);
        Task<IEnumerable<FloorplanMaskedAreaDto>> GetAllAsync();
        Task<FloorplanMaskedAreaDto> CreateAsync(FloorplanMaskedAreaCreateDto createDto);
        Task UpdateAsync(Guid id, FloorplanMaskedAreaUpdateDto updateDto);
        Task DeleteAsync(Guid id);
    }
}