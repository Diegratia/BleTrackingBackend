using System;
using System.Collections.Generic;
using System.IO;
using Standard.Licensing;

namespace LicenseGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== BleTracking License Generator ===");
            Console.WriteLine("Pilih aksi:");
            Console.WriteLine("1. Generate Key Pair (Private & Public Key)");
            Console.WriteLine("2. Buat File Lisensi Baru");
            Console.Write("Pilihan (1/2): ");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                GenerateKeys();
            }
            else if (choice == "2")
            {
                GenerateLicense();
            }
            else
            {
                Console.WriteLine("Pilihan tidak valid.");
            }
        }

        static void GenerateKeys()
        {
            Console.Write("Masukkan password untuk Private Key: ");
            var password = Console.ReadLine();

            var keyGenerator = Standard.Licensing.Security.Cryptography.KeyGenerator.Create();
            var keyPair = keyGenerator.GenerateKeyPair();
            
            var privateKey = keyPair.ToEncryptedPrivateKeyString(password);
            var publicKey = keyPair.ToPublicKeyString();

            File.WriteAllText("PrivateKey.txt", privateKey);
            File.WriteAllText("PublicKey.txt", publicKey);

            Console.WriteLine("\n[SUCCESS] Key pair berhasil dibuat!");
            Console.WriteLine("- PrivateKey.txt (SIMPAN BAIK-BAIK, JANGAN BOCOR)");
            Console.WriteLine("- PublicKey.txt (SIMPAN DI APLIKASI CUSTOMER)");
        }

        static void GenerateLicense()
        {
            if (!File.Exists("PrivateKey.txt"))
            {
                Console.WriteLine("Error: PrivateKey.txt tidak ditemukan. Silakan jalankan opsi 1 dulu.");
                return;
            }

            Console.Write("Masukkan password Private Key: ");
            var password = Console.ReadLine();
            var privateKey = File.ReadAllText("PrivateKey.txt");

            Console.Write("Masukkan Nama Customer / Perusahaan: ");
            var customerName = Console.ReadLine();

            Console.Write("Masukkan Machine ID Customer: ");
            var machineId = Console.ReadLine();

            Console.Write("Masa Aktif Lisensi (dalam Tahun, misal: 1): ");
            if (!int.TryParse(Console.ReadLine(), out int years)) years = 1;

            var license = License.New()
                .WithUniqueIdentifier(Guid.NewGuid())
                .As(LicenseType.Standard)
                .ExpiresAt(DateTime.Now.AddYears(years))
                .WithAdditionalAttributes(new Dictionary<string, string>
                {
                    { "MachineID", machineId ?? "" }
                })
                .LicensedTo(customerName, "info@customer.com")
                .CreateAndSignWithPrivateKey(privateKey, password);

            string fileName = $"license_{customerName?.Replace(" ", "")}.lic";
            File.WriteAllText(fileName, license.ToString());

            Console.WriteLine($"\n[SUCCESS] File lisensi '{fileName}' berhasil dibuat untuk {customerName}!");
            Console.WriteLine($"Machine ID: {machineId}");
            Console.WriteLine($"Expired: {DateTime.Now.AddYears(years):yyyy-MM-dd}");
        }
    }
}
