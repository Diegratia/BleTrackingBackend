using Data.ViewModels;
using FluentValidation;

public class SwapCreateValidator : AbstractValidator<CardSwapTransactionCreateDto>
{
    public SwapCreateValidator()
    {
        RuleFor(swap => swap.VisitorId)
            .NotEmpty().WithMessage("Visitor Id is required.");

        RuleFor(swap => swap.SwapMode)
            .NotEmpty().WithMessage("Swap Mode is required.");
    }
}

public class SwapForwardValidator : AbstractValidator<ForwardSwapRequest>
{
    public SwapForwardValidator()
    {
        RuleFor(swap => swap.VisitorId)
            .NotEmpty().WithMessage("Visitor Id is required.");

        RuleFor(swap => swap.SwapMode)
            .NotEmpty().WithMessage("Swap Mode is required.");
    }
}
public class SwapReverseValidator : AbstractValidator<ReverseSwapRequest>
{
    public SwapReverseValidator()
    {
        RuleFor(swap => swap.VisitorId)
            .NotEmpty().WithMessage("Visitor Id is required.");

        RuleFor(swap => swap.SwapChainId)
            .NotEmpty().WithMessage("Swap Chain Id is required.");
    }
}
