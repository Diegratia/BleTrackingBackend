using Data.ViewModels;
using FluentValidation;

public class CardCreateDtoValidator : AbstractValidator<CardCreateDto>
{
    public CardCreateDtoValidator()
    {
        RuleFor(card => card.CardNumber)
            .NotEmpty().WithMessage("CardNumber is required.");

        RuleFor(card => card.Dmac)
            .NotEmpty().WithMessage("Dmac is required.") ;

        RuleFor(card => card.CardType)
            .NotEmpty().WithMessage("CardType is required.");

    }
}

public class CardUpdateDtoValidator : AbstractValidator<CardUpdateDto>
{
    public CardUpdateDtoValidator()
    {
        RuleFor(card => card.CardNumber)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .When(x => x.CardNumber != null);

        RuleFor(card => card.Dmac)
            .NotEmpty().WithMessage("Tag cannot be empty.")
            .When(x => x.Dmac != null);

        RuleFor(card => card.CardType)
            .NotEmpty().WithMessage("Tag cannot be empty.")
            .When(x => x.Dmac != null);
    }
}
