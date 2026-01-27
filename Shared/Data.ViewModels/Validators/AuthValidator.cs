using Data.ViewModels;
using FluentValidation;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(LoginDto => LoginDto.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(LoginDto => LoginDto.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}