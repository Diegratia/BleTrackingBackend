using System;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.Shared.ExceptionHelper;
using DataView;
using Entities.Models;
using Shared.Contracts;

namespace BusinessLogic.Services.ServiceValidators
{
    public class PatrolCaseValidatorService : IPatrolCaseValidatorService
    {
        public Task ValidateCreateAsync(PatrolCaseCreateDto dto, PatrolSession? session)
        {
            if (session == null)
                throw new NotFoundException($"PatrolSession with id {dto.PatrolSessionId} not found");

            if (dto.CaseType != CaseType.PatrolSummary && !dto.ThreatLevel.HasValue)
                throw new BusinessException("ThreatLevel is required for operational cases (Incident, Hazard, Damage, Theft, Report)");

            return Task.CompletedTask;
        }

        public Task ValidateUpdateAsync(PatrolCase patrolCase)
        {
            if (patrolCase.CaseStatus != CaseStatus.Submitted && patrolCase.CaseStatus != CaseStatus.Rejected)
                throw new BusinessException($"Only Submitted or Rejected cases can be updated. Current status: {patrolCase.CaseStatus}");

            return Task.CompletedTask;
        }

        public Task ValidateApproveAsync(PatrolCase patrolCase, Guid currentSecurityId, bool isPrimaryAdmin)
        {
            if (patrolCase.ApprovalType == PatrolApprovalType.WithoutApproval)
                throw new BusinessException("This case does not require approval.");

            if (patrolCase.CaseStatus != CaseStatus.Submitted)
                throw new BusinessException($"Only Submitted cases can be approved. Current status: {patrolCase.CaseStatus}");

            var head1Id = patrolCase.SecurityHead1Id;
            var head2Id = patrolCase.SecurityHead2Id;
            var headsMissing = patrolCase.ApprovalType == PatrolApprovalType.Or
                ? !head1Id.HasValue && !head2Id.HasValue
                : !head1Id.HasValue || !head2Id.HasValue;

            if (headsMissing)
            {
                if (!isPrimaryAdmin)
                    throw new UnauthorizedException("Only PrimaryAdmin can approve cases without assigned heads.");
                
                return Task.CompletedTask;
            }

            var isHead1 = head1Id.HasValue && head1Id.Value == currentSecurityId;
            var isHead2 = head2Id.HasValue && head2Id.Value == currentSecurityId;

            if (!isHead1 && !isHead2)
                throw new UnauthorizedException("Only assigned heads can approve this case.");

            if (patrolCase.ApprovalType == PatrolApprovalType.Sequential
                && isHead2
                && !patrolCase.ApprovedByHead1Id.HasValue)
            {
                throw new BusinessException("Head 1 must approve before Head 2.");
            }

            return Task.CompletedTask;
        }

        public Task ValidateRejectAsync(PatrolCase patrolCase, PatrolCaseApprovalDto dto, Guid currentSecurityId, bool isPrimaryAdmin)
        {
            if (string.IsNullOrEmpty(dto.Reason))
                throw new BusinessException("Reason is required when rejecting a case");

            if (patrolCase.ApprovalType == PatrolApprovalType.WithoutApproval)
                throw new BusinessException("This case does not require approval.");

            if (patrolCase.CaseStatus != CaseStatus.Submitted)
                throw new BusinessException($"Only Submitted cases can be rejected. Current status: {patrolCase.CaseStatus}");

            var head1Id = patrolCase.SecurityHead1Id;
            var head2Id = patrolCase.SecurityHead2Id;
            var headsMissing = patrolCase.ApprovalType == PatrolApprovalType.Or
                ? !head1Id.HasValue && !head2Id.HasValue
                : !head1Id.HasValue || !head2Id.HasValue;

            if (headsMissing)
            {
                if (!isPrimaryAdmin)
                    throw new UnauthorizedException("Only PrimaryAdmin can reject cases without assigned heads.");
                
                return Task.CompletedTask;
            }

            var isHead1 = head1Id.HasValue && head1Id.Value == currentSecurityId;
            var isHead2 = head2Id.HasValue && head2Id.Value == currentSecurityId;

            if (!isHead1 && !isHead2)
                throw new UnauthorizedException("Only assigned heads can reject this case.");

            return Task.CompletedTask;
        }

        public Task ValidateCloseAsync(PatrolCase patrolCase)
        {
            if (patrolCase.CaseStatus != CaseStatus.Approved)
                throw new BusinessException($"Only Approved cases can be closed. Current status: {patrolCase.CaseStatus}");

            return Task.CompletedTask;
        }

        public Task ValidateDeleteAttachmentAsync(PatrolCase patrolCase)
        {
            if (patrolCase.CaseStatus != CaseStatus.Submitted && patrolCase.CaseStatus != CaseStatus.Rejected)
                throw new BusinessException($"Cannot delete attachment. Case status is {patrolCase.CaseStatus}");

            return Task.CompletedTask;
        }
    }
}
