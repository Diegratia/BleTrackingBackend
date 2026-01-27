using Data.ViewModels;
using FluentValidation;

public class MstBleReaderCreateValidator : AbstractValidator<MstBleReaderCreateDto>
{
    public MstBleReaderCreateValidator()
    {
        RuleFor(reader => reader.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.");

        RuleFor(reader => reader.Gmac)
            .NotEmpty().WithMessage("Reader Mac Address is required.")
            .MaximumLength(255).WithMessage("Mac cannot exceed 255 characters.");

    }
}

public class MstBleReaderUpdateValidator : AbstractValidator<MstBleReaderUpdateDto>
{
    public MstBleReaderUpdateValidator()
    {
        RuleFor(brand => brand.Name)
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.")
            .When(x => x.Name != null) 
            .NotEmpty().WithMessage("Name cannot be empty.")
            .When(x => x.Name != null);

        RuleFor(brand => brand.Gmac)
            .MaximumLength(255).WithMessage("Reader Mac cannot exceed 255 characters.")
            .When(x => x.Gmac != null)
            .NotEmpty().WithMessage("Reader Mac cannot be empty.")
            .When(x => x.Gmac != null);
    }
}
