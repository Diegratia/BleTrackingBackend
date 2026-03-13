using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Standard.Licensing;
using Standard.Licensing.Validation;

namespace LicenseChecker
{
    class Program
    {
        // Embedded Public Key - This key is used to verify license signatures
        // Generated once by LicenseGenerator and embedded in the application
        private const string PublicKey = @"MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEmBppZFeGPf2nZHkt+DfMfVKSYSrofrM4IEjYo1lrufC2LWnPHRQFrmCM3x4nTb0WSOM1SW1lqwICc6JGyJpGmQ=="; 

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "--generate-id")
                {
                    string id = MachineIdHelper.GenerateMachineId();
                    Console.WriteLine("=== MACHINE ID ===");
                    Console.WriteLine(id);
                    Console.WriteLine("==================");
                    Console.WriteLine("Send this ID to the Provider to get a license file.");
                    return;
                }
                else if (args[0] == "--activate" && args.Length > 1)
                {
                    ActivateLicense(args[1]);
                    return;
                }
            }

            // If no arguments provided, show interactive menu
            Console.WriteLine("=== BleTracking License Checker & Activator ===");
            Console.WriteLine("1. View Machine ID");
            Console.WriteLine("2. Activate License File (.lic)");
            Console.Write("Choice (1/2): ");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                string id = MachineIdHelper.GenerateMachineId();
                Console.WriteLine("\n=== MACHINE ID ===");
                Console.WriteLine(id);
                Console.WriteLine("==================");
                Console.WriteLine("Send this ID to the Provider to get a license file.");
            }
            else if (choice == "2")
            {
                Console.Write("\nEnter license file path/name (example: license.lic): ");
                string filePath = Console.ReadLine() ?? "";
                ActivateLicense(filePath);
            }
            else
            {
                Console.WriteLine("Invalid choice.");
            }
        }

        static void ActivateLicense(string licenseFilePath)
        {
            if (!File.Exists(licenseFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ License file '{licenseFilePath}' not found!");
                Console.ResetColor();
                Environment.Exit(1);
            }

            try
            {
                var licenseString = File.ReadAllText(licenseFilePath);
                var license = License.Load(licenseString);

                // If PublicKey is empty, skip signature validation temporarily (for development phase only)
                // In production, this field is REQUIRED.
                System.Collections.Generic.IEnumerable<IValidationFailure> validationFailures;
                
                if (!string.IsNullOrEmpty(PublicKey))
                {
                    validationFailures = license.Validate()
                                                .Signature(PublicKey)
                                                .AssertValidLicense();
                }
                else
                {
                    // Fallback for development: signature validation bypassed
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️ WARNING: Public Key is not set. Signature Validation bypassed (Development mode only).");
                    Console.ResetColor();
                    validationFailures = Enumerable.Empty<IValidationFailure>();
                }

                if (validationFailures.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ License File is Invalid or Corrupt!");
                    foreach (var failure in validationFailures)
                    {
                        Console.WriteLine($" - {failure.Message} ({failure.HowToResolve})");
                    }
                    Console.ResetColor();
                    Environment.Exit(1);
                }

                // Check expiration
                if (DateTime.Now > license.Expiration)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ License File expired since {license.Expiration:yyyy-MM-dd}");
                    Console.ResetColor();
                    Environment.Exit(1);
                }

                // Check Machine ID
                string expectedMachineId = license.AdditionalAttributes.Get("MachineID");
                string currentMachineId = MachineIdHelper.GenerateMachineId();

                if (expectedMachineId != currentMachineId)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ This license is not for this computer/server!");
                    Console.WriteLine($"License issued for Machine ID: {expectedMachineId}");
                    Console.WriteLine($"Current Machine ID: {currentMachineId}");
                    Console.ResetColor();
                    Environment.Exit(1);
                }

                // If all validations passed:
                UpdateDatabaseLicenseStatus(license);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Failed to process license file: {ex.Message}");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }

        static void UpdateDatabaseLicenseStatus(License license)
        {
            var services = new ServiceCollection();
            services.AddDbContext<BleTrackingDbContext>(options =>
                options.UseSqlServer("Server= 192.168.1.116,1433;Database=BleTrackingDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True"));
            
            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BleTrackingDbContext>();
                
                // Get MstApplication (takes the first one as this is a single-tenant instance on customer server)
                var app = context.MstApplications.FirstOrDefault();

                if (app == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ MstApplication data not found in database.");
                    Console.ResetColor();
                    Environment.Exit(1);
                }

                // Update Application with full license metadata
                app.ApplicationStatus = 1;
                app.ApplicationExpired = license.Expiration;
                app.CustomerName = license.Customer.Name;
                app.LicenseMachineId = license.AdditionalAttributes.Get("MachineID");

                // Sync License Type & Tier
                var typeStr = license.AdditionalAttributes.Get("LicenseType");
                if (Enum.TryParse<Shared.Contracts.LicenseType>(typeStr, true, out var licenseType))
                    app.LicenseType = licenseType;

                var tierStr = license.AdditionalAttributes.Get("LicenseTier");
                if (Enum.TryParse<Shared.Contracts.LicenseTier>(tierStr, true, out var licenseTier))
                    app.LicenseTier = licenseTier;

                // Sync Limits
                if (int.TryParse(license.AdditionalAttributes.Get("MaxBeacons"), out int maxBeacons))
                    app.MaxBeacons = maxBeacons;
                
                if (int.TryParse(license.AdditionalAttributes.Get("MaxReaders"), out int maxReaders))
                    app.MaxReaders = maxReaders;

                // Sync Features
                var featuresStr = license.AdditionalAttributes.Get("Features");
                if (!string.IsNullOrEmpty(featuresStr))
                {
                    var features = featuresStr.Split(',').Select(f => f.Trim()).Distinct().ToList();
                    app.EnabledFeatures = System.Text.Json.JsonSerializer.Serialize(features);
                }

                context.SaveChanges();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ License validated and activated successfully!");
                Console.WriteLine($"Customer       : {license.Customer.Name}");
                Console.WriteLine($"Tier           : {app.LicenseTier} ({app.LicenseType})");
                Console.WriteLine($"Limits         : {app.MaxBeacons} Beacons, {app.MaxReaders} Readers");
                Console.WriteLine($"Active Until   : {app.ApplicationExpired:yyyy-MM-dd}");
                Console.WriteLine($"Database MstApp: Updated (Status = {app.ApplicationStatus})");
                Console.ResetColor();
            }
        }
    }
}
