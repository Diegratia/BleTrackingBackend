using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsExtension(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            return services;
        }
    }
}
