using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Entities.Models;

class Program
{
    static void Main(string[] args)
    {
        // Setup DI container manual
        var services = new ServiceCollection();

        // Koneksi langsung, bisa diganti ambil dari appsettings.json kalau mau
        services.AddDbContext<BleTrackingDbContext>(options =>
            options.UseSqlServer( "Server= 192.168.1.116,1433;Database=BleTrackingDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True"));
        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BleTrackingDbContext>();

            Console.WriteLine("Please enter your API key:");
            string userInput = (Console.ReadLine() ?? "").Trim();

            Console.WriteLine($"You entered: {userInput}");

            // Cari integration dengan ApiKeyValue cocok dan status masih 0 (belum aktif)
        var integration = context.MstIntegrations
          .IgnoreQueryFilters()
          .FirstOrDefault(i => i.ApiKeyValue == userInput);

            if (integration == null)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("❌ Invalid API Key. Service cannot start.");
        Console.ResetColor();
        Environment.Exit(1);
      }

            if (integration.Status == 0)
            {
                integration.Status = 1;
                context.SaveChanges();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ API Key validated & activated for Application {integration.ApplicationId}");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠️ API Key already activated for Application {integration.ApplicationId}");
                Console.ResetColor();
            }


            // Jika valid → aktifkan
            integration.Status = 1;
            context.SaveChanges();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ API Key validated. Integration {integration.Id} activated for Application {integration.ApplicationId}");
            Console.ResetColor();
        }
    }
}
