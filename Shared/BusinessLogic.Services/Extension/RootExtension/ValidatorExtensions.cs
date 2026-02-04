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
            services.AddValidatorsFromAssemblyContaining<PatrolRouteCreateValidator>();
            services.AddValidatorsFromAssemblyContaining<PatrolRouteUpdateValidator>();
            services.AddValidatorsFromAssemblyContaining<PatrolAssignmentCreateValidator>();
            services.AddValidatorsFromAssemblyContaining<PatrolAssignmentUpdateValidator>();
            services.AddValidatorsFromAssemblyContaining<PatrolSessionStartValidator>();
            services.AddValidatorsFromAssemblyContaining<PatrolCaseCreateValidator>();
            services.AddValidatorsFromAssemblyContaining<PatrolCaseUpdateValidator>();
            services.AddValidatorsFromAssemblyContaining<PatrolAttachmentCreateValidator>();
            services.AddValidatorsFromAssemblyContaining<PatrolAttachmentUpdateValidator>();
            return services;
        }
    }
}
