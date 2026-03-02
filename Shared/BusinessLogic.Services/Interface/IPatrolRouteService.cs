using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IPatrolRouteService
    {
        Task<PatrolRouteRead> CreateAsync(PatrolRouteCreateDto dto);
        Task<PatrolRouteRead> UpdateAsync(Guid id, PatrolRouteUpdateDto dto);
        Task DeleteAsync(Guid id);
        Task<PatrolRouteRead?> GetByIdAsync(Guid id);
        Task<IEnumerable<PatrolRouteRead?>> GetAllAsync();
        Task<IEnumerable<PatrolRouteLookUpRead>> GetAllLookUpAsync();
        Task<object> FilterAsync(
                DataTablesProjectedRequest request,
                PatrolRouteFilter filter
            );
        }
}