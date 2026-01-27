using Data.ViewModels.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public static class ValidatorExtensions
    {
        public static IServiceCollection AddValidatorExtensions(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining<LoginValidator>();
            services.AddValidatorsFromAssemblyContaining<PatrolAreaCreateValidator>();
            services.AddValidatorsFromAssemblyContaining<PatrolAreaUpdateValidator>();
            services.AddValidatorsFromAssemblyContaining<PatrolRouteUpdateValidator>();
            services.AddValidatorsFromAssemblyContaining<PatrolRouteUpdateValidator>();
            return services;
        }
    }
}
