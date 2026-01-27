using Data.ViewModels;
using FluentValidation;

public class MstBrandCreateDtoValidator : AbstractValidator<MstBrandCreateDto>
{
    public MstBrandCreateDtoValidator()
    {
        RuleFor(brand => brand.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.");

        RuleFor(brand => brand.Tag)
            .NotEmpty().WithMessage("Tag is required.")
            .MaximumLength(255).WithMessage("Tag cannot exceed 255 characters.");

    }
}

public class MstBrandUpdateDtoValidator : AbstractValidator<MstBrandUpdateDto>
{
    public MstBrandUpdateDtoValidator()
    {
        RuleFor(brand => brand.Name)
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.")
            .When(x => x.Name != null) 
            .NotEmpty().WithMessage("Name cannot be empty.")
            .When(x => x.Name != null);

        RuleFor(brand => brand.Tag)
            .MaximumLength(255).WithMessage("Tag cannot exceed 255 characters.")
            .When(x => x.Tag != null)
            .NotEmpty().WithMessage("Tag cannot be empty.")
            .When(x => x.Tag != null);
    }
}
