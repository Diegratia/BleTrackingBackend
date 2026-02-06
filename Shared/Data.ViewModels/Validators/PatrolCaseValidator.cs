using FluentValidation;

namespace Data.ViewModels.Validators
{
    public class PatrolCaseCreateValidator : AbstractValidator<PatrolCaseCreateDto>
    {
        public PatrolCaseCreateValidator()
        {
            RuleFor(x => x.PatrolSessionId)
                .NotEmpty().WithMessage("PatrolSessionId is required.");

            RuleFor(x => x.CaseType)
                .NotNull().WithMessage("CaseType is required.");

            RuleFor(x => x.Title)
                .MaximumLength(255).WithMessage("Title cannot exceed 255 characters.")
                .When(x => x.Title != null);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => x.Description != null);

            RuleForEach(x => x.Attachments)
                .SetValidator(new PatrolAttachmentCreateValidator())
                .When(x => x.Attachments != null);
        }
    }

    public class PatrolCaseUpdateValidator : AbstractValidator<PatrolCaseUpdateDto>
    {
        public PatrolCaseUpdateValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(255).WithMessage("Title cannot exceed 255 characters.")
                .When(x => x.Title != null);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => x.Description != null);

            // Attachments are appended, not replaced
            RuleForEach(x => x.Attachments)
                .SetValidator(new PatrolAttachmentCreateValidator())
                .When(x => x.Attachments != null);
        }
    }
}
