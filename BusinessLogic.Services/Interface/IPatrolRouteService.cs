using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IPatrolRouteService
    {
        Task<PatrolRouteDto> CreateAsync(PatrolRouteCreateDto dto);
        Task<PatrolRouteDto> UpdateAsync(Guid id, PatrolRouteUpdateDto dto);
        Task DeleteAsync(Guid id);
        Task<PatrolRouteDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<PatrolRouteDto?>> GetAllAsync();
        Task<object> FilterAsync(DataTablesRequest request);
        Task<IEnumerable<PatrolRouteLookUpDto>> GetAllLookUpAsync();
    }
}