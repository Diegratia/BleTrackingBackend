using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories.DbContexts;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public static class DbContextExtensions
    {
        public static IServiceCollection AddDbContextExtension(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("BleTrackingDbConnection")
                                ?? "Server=localhost,1433;Database=BleTrackingDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True";

            services.AddDbContext<BleTrackingDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }
    }
}
