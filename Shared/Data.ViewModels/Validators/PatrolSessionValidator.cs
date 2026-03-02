using FluentValidation;

namespace Data.ViewModels.Validators
{
    public class PatrolSessionStartValidator : AbstractValidator<PatrolSessionStartDto>
    {
        public PatrolSessionStartValidator()
        {
            RuleFor(x => x.PatrolAssignmentId)
                .NotEmpty().WithMessage("PatrolAssignmentId is required.");
        }
    }
}
