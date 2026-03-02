using Data.ViewModels;
using FluentValidation;

namespace Data.ViewModels.Validators
{
    public class VisitorExtendedValidator : AbstractValidator<ExtendedTimeDto>
    {
        public VisitorExtendedValidator()
        {
            RuleFor(trxVisitor => trxVisitor.ExtendedVisitorTime)
                .GreaterThan(0).WithMessage("Extend Time must be greater than 0.")
                .LessThanOrEqualTo(1440).WithMessage("ExtendedVisitorTime cannot exceed 1440 minutes (24 hours).");
        }
    }
}