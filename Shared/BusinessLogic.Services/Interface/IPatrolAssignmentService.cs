using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Shared.Contracts.Read;
using Shared.Contracts.Shared.Contracts;

namespace BusinessLogic.Services.Interface
{
    public interface IPatrolAssignmentService
    {
        Task<PatrolAssignmentRead?> GetByIdAsync(Guid id);
        Task<IEnumerable<PatrolAssignmentRead>> GetAllAsync();
        Task<PatrolAssignmentRead> CreateAsync(PatrolAssignmentCreateDto createDto);
        Task<PatrolAssignmentRead> UpdateAsync(Guid id, PatrolAssignmentUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request);
        Task<object> FilterProjectedAsync(
            DataTablesProjectedRequest request,
            PatrolAssignmentFilter filter
        );
        Task<IEnumerable<PatrolAssignmentLookUpRead>> GetAllLookUpAsync();

    }
}