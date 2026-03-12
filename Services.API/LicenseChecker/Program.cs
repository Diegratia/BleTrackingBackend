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
        // Ganti string ini dengan PublicKey hasil generate dari LicenseGenerator
        private const string PublicKey = @""; 

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
                    Console.WriteLine("Kirimkan ID ini ke Provider untuk mendapatkan file lisensi.");
                    return;
                }
                else if (args[0] == "--activate" && args.Length > 1)
                {
                    ActivateLicense(args[1]);
                    return;
                }
            }

            // Jika tidak ada args, tampilkan menu interaktif
            Console.WriteLine("=== BleTracking License Checker & Activator ===");
            Console.WriteLine("1. Lihat Machine ID");
            Console.WriteLine("2. Aktivasi File Lisensi (.lic)");
            Console.Write("Pilihan (1/2): ");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                string id = MachineIdHelper.GenerateMachineId();
                Console.WriteLine("\n=== MACHINE ID ===");
                Console.WriteLine(id);
                Console.WriteLine("==================");
                Console.WriteLine("Kirimkan ID ini ke Provider untuk mendapatkan file lisensi.");
            }
            else if (choice == "2")
            {
                Console.Write("\nMasukkan path/nama file lisensi (contoh: license.lic): ");
                string filePath = Console.ReadLine() ?? "";
                ActivateLicense(filePath);
            }
            else
            {
                Console.WriteLine("Pilihan tidak valid.");
            }
        }

        static void ActivateLicense(string licenseFilePath)
        {
            if (!File.Exists(licenseFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ File lisensi '{licenseFilePath}' tidak ditemukan!");
                Console.ResetColor();
                Environment.Exit(1);
            }

            try
            {
                var licenseString = File.ReadAllText(licenseFilePath);
                var license = License.Load(licenseString);

                // Jika PublicKey kosong, abaikan validasi Signature sementara (untuk development phase)
                // Di tahap production, ini Wajib ada.
                System.Collections.Generic.IEnumerable<IValidationFailure> validationFailures;
                
                if (!string.IsNullOrEmpty(PublicKey))
                {
                    validationFailures = license.Validate()
                                                .Signature(PublicKey)
                                                .AssertValidLicense();
                }
                else
                {
                    // Fallback development: no signature validation validation
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️ PERINGATAN: Public Key belum di-set. Signature Validation dilewati (Hanya mode Development).");
                    Console.ResetColor();
                    validationFailures = Enumerable.Empty<IValidationFailure>();
                }

                if (validationFailures.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ File Lisensi Invalid atau Corrupt!");
                    foreach (var failure in validationFailures)
                    {
                        Console.WriteLine($" - {failure.Message} ({failure.HowToResolve})");
                    }
                    Console.ResetColor();
                    Environment.Exit(1);
                }

                // Cek Expired
                if (DateTime.Now > license.Expiration)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ File Lisensi sudah kedaluwarsa sejak {license.Expiration:yyyy-MM-dd}");
                    Console.ResetColor();
                    Environment.Exit(1);
                }

                // Cek Machine ID
                string expectedMachineId = license.AdditionalAttributes.Get("MachineID");
                string currentMachineId = MachineIdHelper.GenerateMachineId();

                if (expectedMachineId != currentMachineId)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Lisensi ini bukan untuk komputer/server ini!");
                    Console.WriteLine($"Lisensi diterbitkan untuk Machine ID: {expectedMachineId}");
                    Console.WriteLine($"Machine ID saat ini: {currentMachineId}");
                    Console.ResetColor();
                    Environment.Exit(1);
                }

                // Jika lolos semua validasi:
                UpdateDatabaseLicenseStatus(license);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Gagal memproses file lisensi: {ex.Message}");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }

        static void UpdateDatabaseLicenseStatus(License license)
        {
            var services = new ServiceCollection();
            services.AddDbContext<BleTrackingDbContext>(options =>
                options.UseSqlServer("Server= localhost,1433;Database=BleTrackingDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True"));
            
            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BleTrackingDbContext>();
                
                // Mengambil MstApplication (Default ambil yang pertama karena system ini single-tenant instance di server customer)
                var app = context.MstApplications.FirstOrDefault();

                if (app == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Data MstApplication tidak ditemukan di database.");
                    Console.ResetColor();
                    Environment.Exit(1);
                }

                // Update Status & Expired
                app.ApplicationStatus = 1;
                app.ApplicationExpired = license.Expiration;
                app.LicenseCode = license.AdditionalAttributes.Get("MachineID");
                
                context.SaveChanges();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✅ Lisensi berhasil divalidasi dan diaktivasi!");
                Console.WriteLine($"Customer       : {license.Customer.Name}");
                Console.WriteLine($"Aktif Sampai   : {app.ApplicationExpired:yyyy-MM-dd}");
                Console.WriteLine($"Database MstApp: Diperbarui (Status = {app.ApplicationStatus})");
                Console.ResetColor();
            }
        }
    }
}
