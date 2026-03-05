using System;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;

namespace BusinessLogic.Services.ServiceValidators
{
    public interface IPatrolValidationService
    {
        Task ValidateAssignmentCreateAsync(PatrolAssignmentCreateDto dto, Guid appId);
        Task ValidateAssignmentUpdateAsync(Guid id, PatrolAssignmentUpdateDto dto, Guid appId, PatrolAssignment currentAssignment);
        Task ValidateShiftReplacementAsync(PatrolAssignment assignment, Guid originalSecurityId, Guid substituteSecurityId, DateOnly startDate, DateOnly endDate, Guid? excludeReplacementId = null);
        Task EnsureNoActiveSessionsAsync(Guid assignmentId);
    }
}
