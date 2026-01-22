using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IPatrolAssignmentService
    {
        Task<PatrolAssignmentDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<PatrolAssignmentDto>> GetAllAsync();
        Task<PatrolAssignmentDto> CreateAsync(PatrolAssignmentCreateDto createDto);
        Task<PatrolAssignmentDto> UpdateAsync(Guid id, PatrolAssignmentUpdateDto updateDto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesRequest request); 
        Task<IEnumerable<PatrolAssignmentLookUpDto>> GetAllLookUpAsync();

    }
}