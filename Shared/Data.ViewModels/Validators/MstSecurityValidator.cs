using FluentValidation;
using Shared.Contracts;

namespace Data.ViewModels.Validators
{
    public class MstSecurityCreateValidator : AbstractValidator<MstSecurityCreateDto>
    {
        public MstSecurityCreateValidator()
        {
            // Name validation
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.");

            // Email validation
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.");

            // IdentityId validation
            RuleFor(x => x.IdentityId)
                .NotEmpty().WithMessage("IdentityId is required.")
                .MaximumLength(100).WithMessage("IdentityId cannot exceed 100 characters.");

            // PersonId validation
            RuleFor(x => x.PersonId)
                .MaximumLength(100).WithMessage("PersonId cannot exceed 100 characters.")
                .When(x => x.PersonId != null);

            // Phone validation
            RuleFor(x => x.Phone)
                .MaximumLength(50).WithMessage("Phone cannot exceed 50 characters.")
                .When(x => x.Phone != null);

            // Address validation
            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.")
                .When(x => x.Address != null);

            // Gender validation
            RuleFor(x => x.Gender)
                .Must(BeValidGender).WithMessage("Invalid Gender value. Must be Male, Female, or Other.")
                .When(x => x.Gender != null);

            // StatusEmployee validation
            RuleFor(x => x.StatusEmployee)
                .Must(BeValidStatusEmployee).WithMessage("Invalid StatusEmployee value.")
                .When(x => x.StatusEmployee != null);

            // CardId validation
            RuleFor(x => x.CardId)
                .NotEmpty().WithMessage("CardId is required.");

            // ApplicationId validation
            RuleFor(x => x.ApplicationId)
                .NotEmpty().WithMessage("ApplicationId is required.");

            // BirthDate validation - must be in the past
            RuleFor(x => x.BirthDate)
                .Must(BeInPast).WithMessage("BirthDate must be in the past.")
                .When(x => x.BirthDate.HasValue);

            // JoinDate validation - must be in the past or today
            RuleFor(x => x.JoinDate)
                .Must(BeInPastOrToday).WithMessage("JoinDate cannot be in the future.")
                .When(x => x.JoinDate.HasValue);
        }

        private bool BeValidGender(string? gender)
        {
            return string.IsNullOrEmpty(gender) ||
                   gender.Equals("Male", StringComparison.OrdinalIgnoreCase) ||
                   gender.Equals("Female", StringComparison.OrdinalIgnoreCase) ||
                   gender.Equals("Other", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidStatusEmployee(string? statusEmployee)
        {
            return string.IsNullOrEmpty(statusEmployee) ||
                   Enum.TryParse<StatusEmployee>(statusEmployee, true, out _);
        }

        private bool BeInPast(DateOnly? date)
        {
            return date.HasValue && date.Value < DateOnly.FromDateTime(DateTime.Today);
        }

        private bool BeInPastOrToday(DateOnly? date)
        {
            return date.HasValue && date.Value <= DateOnly.FromDateTime(DateTime.Today);
        }
    }

    public class MstSecurityUpdateValidator : AbstractValidator<MstSecurityUpdateDto>
    {
        public MstSecurityUpdateValidator()
        {
            // Name validation
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.")
                .When(x => x.Name != null);

            // Email validation
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.")
                .When(x => x.Email != null);

            // IdentityId validation
            RuleFor(x => x.IdentityId)
                .NotEmpty().WithMessage("IdentityId is required.")
                .MaximumLength(100).WithMessage("IdentityId cannot exceed 100 characters.")
                .When(x => x.IdentityId != null);

            // PersonId validation
            RuleFor(x => x.PersonId)
                .MaximumLength(100).WithMessage("PersonId cannot exceed 100 characters.")
                .When(x => x.PersonId != null);

            // Phone validation
            RuleFor(x => x.Phone)
                .MaximumLength(50).WithMessage("Phone cannot exceed 50 characters.")
                .When(x => x.Phone != null);

            // Address validation
            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.")
                .When(x => x.Address != null);

            // Gender validation
            RuleFor(x => x.Gender)
                .Must(BeValidGender).WithMessage("Invalid Gender value. Must be Male, Female, or Other.")
                .When(x => x.Gender != null);

            // StatusEmployee validation
            RuleFor(x => x.StatusEmployee)
                .Must(BeValidStatusEmployee).WithMessage("Invalid StatusEmployee value.")
                .When(x => x.StatusEmployee != null);

            // BirthDate validation - must be in the past
            RuleFor(x => x.BirthDate)
                .Must(BeInPast).WithMessage("BirthDate must be in the past.")
                .When(x => x.BirthDate.HasValue);

            // JoinDate validation - must be in the past or today
            RuleFor(x => x.JoinDate)
                .Must(BeInPastOrToday).WithMessage("JoinDate cannot be in the future.")
                .When(x => x.JoinDate.HasValue);
        }

        private bool BeValidGender(string? gender)
        {
            return string.IsNullOrEmpty(gender) ||
                   gender.Equals("Male", StringComparison.OrdinalIgnoreCase) ||
                   gender.Equals("Female", StringComparison.OrdinalIgnoreCase) ||
                   gender.Equals("Other", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidStatusEmployee(string? statusEmployee)
        {
            return string.IsNullOrEmpty(statusEmployee) ||
                   Enum.TryParse<StatusEmployee>(statusEmployee, true, out _);
        }

        private bool BeInPast(DateOnly? date)
        {
            return date.HasValue && date.Value < DateOnly.FromDateTime(DateTime.Today);
        }

        private bool BeInPastOrToday(DateOnly? date)
        {
            return date.HasValue && date.Value <= DateOnly.FromDateTime(DateTime.Today);
        }
    }
}
