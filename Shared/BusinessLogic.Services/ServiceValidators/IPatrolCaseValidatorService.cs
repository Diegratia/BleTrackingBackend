using System;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;

namespace BusinessLogic.Services.ServiceValidators
{
    public interface IPatrolCaseValidatorService
    {
        Task ValidateCreateAsync(PatrolCaseCreateDto dto, PatrolSession? session);
        Task ValidateUpdateAsync(PatrolCase patrolCase);
        Task ValidateApproveAsync(PatrolCase patrolCase, Guid currentSecurityId, bool isPrimaryAdmin);
        Task ValidateRejectAsync(PatrolCase patrolCase, PatrolCaseApprovalDto dto, Guid currentSecurityId, bool isPrimaryAdmin);
        Task ValidateCloseAsync(PatrolCase patrolCase);
        Task ValidateDeleteAttachmentAsync(PatrolCase patrolCase);
    }
}
