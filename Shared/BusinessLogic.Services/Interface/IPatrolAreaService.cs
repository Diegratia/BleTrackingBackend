using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IPatrolAreaService
    {
        Task<PatrolAreaDto> GetByIdAsync(Guid id);
        Task<IEnumerable<PatrolAreaRead>> GetAllAsync();
        Task<PatrolAreaDto> CreateAsync(PatrolAreaCreateDto createDto);
        Task<PatrolAreaDto> UpdateAsync(Guid id, PatrolAreaUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, PatrolAreaFilter filter);
        Task<IEnumerable<PatrolAreaLookUpDto>> GetAllLookUpAsync();

    }
}