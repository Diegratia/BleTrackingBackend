using FluentValidation;

namespace Data.ViewModels.Validators
{
    public class PatrolAttachmentCreateValidator : AbstractValidator<PatrolAttachmentCreateDto>
    {
        public PatrolAttachmentCreateValidator()
        {
            RuleFor(x => x.FileUrl)
                .NotEmpty().WithMessage("FileUrl is required.");

            RuleFor(x => x.FileType)
                .NotNull().WithMessage("FileType is required.");

            RuleFor(x => x.PatrolCaseId)
                .NotEmpty().WithMessage("PatrolCaseId is required.");
        }
    }

    public class PatrolAttachmentUpdateValidator : AbstractValidator<PatrolAttachmentUpdateDto>
    {
        public PatrolAttachmentUpdateValidator()
        {
            RuleFor(x => x.FileUrl)
                .NotEmpty().WithMessage("FileUrl is required.")
                .When(x => x.FileUrl != null);

            RuleFor(x => x.FileType)
                .NotNull().WithMessage("FileType is required.")
                .When(x => x.FileType.HasValue);
        }
    }
}
