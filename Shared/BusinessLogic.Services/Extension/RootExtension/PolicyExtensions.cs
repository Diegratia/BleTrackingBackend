using Shared.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public static class PolicyExtensions
    {
        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAuthenticatedUser", p => p.RequireAuthenticatedUser());
                options.AddPolicy("RequiredSystemUser", p => p.RequireRole("System"));
                options.AddPolicy("RequirePrimaryRole", p => p.RequireRole("Primary"));
                options.AddPolicy("RequireSuperAdminRole", p => p.RequireRole("SuperAdmin"));
                options.AddPolicy("RequirePrimaryAdminRole", p => p.RequireRole("PrimaryAdmin"));

                options.AddPolicy("RequireSystemOrSuperAdminRole", p =>
                    p.RequireAssertion(c =>
                        c.User.IsInRole("System") || c.User.IsInRole("SuperAdmin")));

                options.AddPolicy("RequirePrimaryOrSystemOrSuperAdminRole", p =>
                    p.RequireAssertion(c =>
                        c.User.IsInRole("System") ||
                        c.User.IsInRole("SuperAdmin") ||
                        c.User.IsInRole("Primary")));

                options.AddPolicy("RequirePrimaryAdminOrSystemOrSuperAdminRole", p =>
                    p.RequireAssertion(c =>
                        c.User.IsInRole("System") ||
                        c.User.IsInRole("SuperAdmin") ||
                        c.User.IsInRole("PrimaryAdmin")));

                options.AddPolicy("RequireAll", p =>
                    p.RequireAssertion(c =>
                        c.User.IsInRole("System") ||
                        c.User.IsInRole("SuperAdmin") ||
                        c.User.IsInRole("PrimaryAdmin") ||
                        c.User.IsInRole("Primary") ||
                        c.User.IsInRole("Secondary")));

                options.AddPolicy("RequirePrimaryAdminOrSystemOrSuperAdminOrSecondaryRole", policy =>
        {
            policy.RequireAssertion(context =>
                context.User.IsInRole("System") ||
                context.User.IsInRole("SuperAdmin") ||
                context.User.IsInRole("PrimaryAdmin") ||
                context.User.IsInRole("Secondary"));
        });

                options.AddPolicy("RequireUserCreatedRole", p => p.RequireRole("UserCreated"));
            });

            return services;
        }
        
        public static IServiceCollection AddAuthorizationNewPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("MinSystem",
                p => p.Requirements.Add(new MinLevelRequirement(LevelPriority.System)));

                options.AddPolicy("MinSuperAdmin",
                    p => p.Requirements.Add(new MinLevelRequirement(LevelPriority.SuperAdmin)));

                options.AddPolicy("MinPrimaryAdmin",
                    p => p.Requirements.Add(new MinLevelRequirement(LevelPriority.PrimaryAdmin)));

                options.AddPolicy("MinPrimary",
                    p => p.Requirements.Add(new MinLevelRequirement(LevelPriority.Primary)));

                options.AddPolicy("MinSecondary",
                    p => p.Requirements.Add(new MinLevelRequirement(LevelPriority.Secondary)));

                options.AddPolicy("MinUserCreated",
                    p => p.Requirements.Add(new MinLevelRequirement(LevelPriority.UserCreated)));
            });

            return services;
        }
    }
}
