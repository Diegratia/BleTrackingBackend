using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IMstAccessControlService
    {
        Task<MstAccessControlDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MstAccessControlDto>> GetAllAsync();
        Task<MstAccessControlDto> CreateAsync(MstAccessControlCreateDto createDto);
        Task UpdateAsync(Guid id, MstAccessControlUpdateDto updateDto);
        Task DeleteAsync(Guid id);
    }
}