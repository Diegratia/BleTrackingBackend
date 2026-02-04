using Data.ViewModels;
using FluentValidation;
using Shared.Contracts;

public class SwapCreateValidator : AbstractValidator<CardSwapTransactionCreateDto>
{
    public SwapCreateValidator()
    {
        RuleFor(swap => swap.VisitorId)
            .NotEmpty().WithMessage("Visitor Id is required.");
        RuleFor(x => x.SwapMode)
            .NotNull()
            .WithMessage("Swap Mode is required.");
    }
}

public class SwapForwardValidator : AbstractValidator<ForwardSwapRequest>
{
    public SwapForwardValidator()
    {
        RuleFor(swap => swap.VisitorId)
            .NotEmpty().WithMessage("Visitor Id is required.");

        RuleFor(x => x.SwapMode)
        .NotNull()
        .WithMessage("Swap Mode is required.");

        // ToCardId wajib kecuali untuk HoldIdentity dan ExtendAccess
        RuleFor(x => x.ToCardId)
            .NotEmpty()
            .WithMessage("To Card Id is required for this swap mode.")
            .When(x => x.SwapMode != SwapMode.HoldIdentity && x.SwapMode != SwapMode.ExtendAccess);

        // Identity wajib untuk HoldIdentity dan CardAndIdentity
        RuleFor(x => x.IdentityType)
            .NotNull()
            .WithMessage("Identity Type is required for this swap mode.")
            .When(x => x.SwapMode == SwapMode.HoldIdentity || x.SwapMode == SwapMode.CardAndIdentity);

        RuleFor(x => x.IdentityValue)
            .NotEmpty()
            .WithMessage("Identity Value is required for this swap mode.")
            .When(x => x.SwapMode == SwapMode.HoldIdentity || x.SwapMode == SwapMode.CardAndIdentity);
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
