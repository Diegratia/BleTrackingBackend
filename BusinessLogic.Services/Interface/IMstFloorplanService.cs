using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IMstFloorplanService
    {
        Task<MstFloorplanDto> CreateAsync(MstFloorplanCreateDto dto);
        Task<MstFloorplanDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MstFloorplanDto>> GetAllAsync();
        Task UpdateAsync(Guid Id,MstFloorplanUpdateDto dto);
        Task DeleteAsync(Guid id);
    }
}