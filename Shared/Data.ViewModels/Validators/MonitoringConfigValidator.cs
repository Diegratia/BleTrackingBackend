using Data.ViewModels;
using FluentValidation;

namespace Data.ViewModels.Validators
{
    public class ConfigUpdateDtoValidator : AbstractValidator<MonitoringConfigUpdateDto>
    {
        public ConfigUpdateDtoValidator()
        {
            RuleFor(config => config.Config)
                .NotEmpty().WithMessage("Config is required.");
        }
    }
    
    public class ConfigCreateDtoValidator: AbstractValidator<MonitoringConfigCreateDto>
    {
        public ConfigCreateDtoValidator()
        {
            RuleFor(config => config.Config)
                .NotEmpty().WithMessage("Config is required.");
        }
        
    }
}