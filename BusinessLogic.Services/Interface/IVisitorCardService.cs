using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IVisitorCardService
    {
        Task<VisitorCardDto> GetByIdAsync(Guid id);
        Task<IEnumerable<VisitorCardDto>> GetAllAsync();
        Task<VisitorCardDto> CreateAsync(VisitorCardCreateDto createDto);
        Task UpdateAsync(Guid id, VisitorCardUpdateDto updateDto);
        Task DeleteAsync(Guid id);
    }
}