using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Repositories.DbContexts;
using Helpers.Consumer;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    // Tambahkan DbContext
    services.AddDbContext<BleTrackingDbContext>(options =>
        options.UseSqlServer(hostContext.Configuration.GetConnectionString("BleTrackingDbConnection") ??
                            "Server=192.168.1.116,1433;Database=BleTrackingDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True"));

    // Tambahkan Quartz
    QuartzConfig.AddQuartzServices(services);
});

var host = builder.Build();

// Jalankan host
await host.RunAsync();