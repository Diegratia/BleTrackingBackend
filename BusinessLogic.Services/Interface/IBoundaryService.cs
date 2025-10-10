using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IBoundaryService
    {
        Task<BoundaryDto> GetByIdAsync(Guid id);
        Task<IEnumerable<BoundaryDto>> GetAllAsync();
        Task<BoundaryDto> CreateAsync(BoundaryCreateDto createDto);
        Task UpdateAsync(Guid id, BoundaryUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 

    }
}