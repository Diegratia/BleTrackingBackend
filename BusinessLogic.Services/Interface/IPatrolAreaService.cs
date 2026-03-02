using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IPatrolAreaService
    {
        Task<PatrolAreaDto> GetByIdAsync(Guid id);
        Task<IEnumerable<PatrolAreaDto>> GetAllAsync();
        Task<PatrolAreaDto> CreateAsync(PatrolAreaCreateDto createDto);
        Task<PatrolAreaDto> UpdateAsync(Guid id, PatrolAreaUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 
        Task<IEnumerable<PatrolAreaLookUpDto>> GetAllLookUpAsync();

    }
}