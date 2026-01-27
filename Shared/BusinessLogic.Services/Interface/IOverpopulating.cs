using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IOverpopulatingService
    {
        Task<OverpopulatingDto> GetByIdAsync(Guid id);
        Task<IEnumerable<OverpopulatingDto>> GetAllAsync();
        Task<OverpopulatingDto> CreateAsync(OverpopulatingCreateDto createDto);
        Task UpdateAsync(Guid id, OverpopulatingUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 

    }
}