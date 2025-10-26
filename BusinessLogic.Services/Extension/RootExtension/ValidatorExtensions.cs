using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogic.Services.Extension
{
    public static class ValidatorExtensions
    {
        public static IServiceCollection AddValidatorExtensions(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining<LoginValidator>();
            return services;
        }
    }
}
