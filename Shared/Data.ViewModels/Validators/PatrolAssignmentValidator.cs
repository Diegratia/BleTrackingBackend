using System;
using System.Linq;
using FluentValidation;

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

            RuleFor(x => x)
                .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
                .WithMessage("StartDate must be earlier than or equal to EndDate.");
        }
    }
}
