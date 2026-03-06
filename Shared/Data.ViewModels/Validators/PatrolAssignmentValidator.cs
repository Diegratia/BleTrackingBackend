using System;
using System.Linq;
using FluentValidation;
using Shared.Contracts;

namespace Data.ViewModels.Validators
{
    public class PatrolAssignmentCreateValidator : AbstractValidator<PatrolAssignmentCreateDto>
    {
        public PatrolAssignmentCreateValidator()
        {
            RuleFor(x => x.PatrolRouteId)
                .NotEmpty().WithMessage("PatrolRouteId is required.");

            RuleFor(x => x.TimeGroupId)
                .NotEmpty().WithMessage("TimeGroupId is required.");

            RuleFor(x => x.SecurityIds)
                .NotEmpty().WithMessage("SecurityIds is required.");

            RuleFor(x => x.SecurityHead1Id)
                .NotEmpty().WithMessage("At least 1 Security Head (SecurityHead1Id) is required.");

            RuleFor(x => x.SecurityHead2Id)
                .Must((dto, head2Id) => dto.ApprovalType != PatrolApprovalType.Sequential || dto.SecurityHead1Id != head2Id)
                .WithMessage("SecurityHead1 and SecurityHead2 cannot be the same when ApprovalType is Sequential.")
                .When(x => x.SecurityHead2Id.HasValue);

            RuleFor(x => x.Name)
                .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.")
                .When(x => x.Name != null);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => x.Description != null);

            RuleFor(x => x)
                .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
                .WithMessage("StartDate must be earlier than or equal to EndDate.");
        }
    }

    public class PatrolAssignmentUpdateValidator : AbstractValidator<PatrolAssignmentUpdateDto>
    {
        public PatrolAssignmentUpdateValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.")
                .When(x => x.Name != null);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => x.Description != null);

            RuleForEach(x => x.SecurityIds)
                .NotEmpty().WithMessage("SecurityIds cannot contain empty values.")
                .When(x => x.SecurityIds != null && x.SecurityIds.Any());

            RuleFor(x => x.SecurityHead1Id)
                .NotEmpty().WithMessage("At least 1 Security Head (SecurityHead1Id) is required.");

            RuleFor(x => x.SecurityHead2Id)
                .Must((dto, head2Id) => dto.ApprovalType != PatrolApprovalType.Sequential || dto.SecurityHead1Id != head2Id)
                .WithMessage("SecurityHead1 and SecurityHead2 cannot be the same when ApprovalType is Sequential.")
                .When(x => x.SecurityHead2Id.HasValue);

            RuleFor(x => x)
                .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
                .WithMessage("StartDate must be earlier than or equal to EndDate.");
        }
    }

    public class PatrolShiftReplacementCreateValidator : AbstractValidator<PatrolShiftReplacementCreateDto>
    {
        public PatrolShiftReplacementCreateValidator()
        {
            RuleFor(x => x.PatrolAssignmentId)
                .NotEmpty().WithMessage("PatrolAssignmentId is required.");
            RuleFor(x => x.OriginalSecurityId)
                .NotEmpty().WithMessage("OriginalSecurityId is required.");
            RuleFor(x => x.SubstituteSecurityId)
                .NotEmpty().WithMessage("SubstituteSecurityId is required.");
            RuleFor(x => x.ReplacementStartDate)
                .NotEmpty().WithMessage("ReplacementStartDate is required.");
            RuleFor(x => x.ReplacementEndDate)
                .NotEmpty().WithMessage("ReplacementEndDate is required.");
            RuleFor(x => x)
                .Must(x => x.ReplacementStartDate <= x.ReplacementEndDate)
                .WithMessage("ReplacementStartDate must be earlier than or equal to ReplacementEndDate.");
        }
    }

    public class PatrolShiftReplacementUpdateValidator : AbstractValidator<PatrolShiftReplacementUpdateDto>
    {
        public PatrolShiftReplacementUpdateValidator()
        {
            RuleFor(x => x)
                .Must(x => !x.ReplacementStartDate.HasValue || !x.ReplacementEndDate.HasValue || x.ReplacementStartDate <= x.ReplacementEndDate)
                .WithMessage("ReplacementStartDate must be earlier than or equal to ReplacementEndDate.");
        }
    }
}
