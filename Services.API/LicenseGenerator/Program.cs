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
            Console.WriteLine("Select action:");
            Console.WriteLine("1. Generate Key Pair (Private & Public Key)");
            Console.WriteLine("2. Create New License File");
            Console.Write("Choice (1/2): ");
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
                Console.WriteLine("Invalid choice.");
            }
        }

        static void GenerateKeys()
        {
            Console.Write("Enter password for Private Key: ");
            var password = Console.ReadLine();

            var keyGenerator = Standard.Licensing.Security.Cryptography.KeyGenerator.Create();
            var keyPair = keyGenerator.GenerateKeyPair();
            
            var privateKey = keyPair.ToEncryptedPrivateKeyString(password);
            var publicKey = keyPair.ToPublicKeyString();

            File.WriteAllText("PrivateKey.txt", privateKey);
            File.WriteAllText("PublicKey.txt", publicKey);

            Console.WriteLine("\n[SUCCESS] Key pair created successfully!");
            Console.WriteLine("- PrivateKey.txt (KEEP SAFE, DO NOT LEAK)");
            Console.WriteLine("- PublicKey.txt (STORE IN CUSTOMER APPLICATION)");
        }

        static void GenerateLicense()
        {
            if (!File.Exists("PrivateKey.txt"))
            {
                Console.WriteLine("Error: PrivateKey.txt not found. Please run option 1 first.");
                return;
            }

            Console.Write("Enter Private Key password: ");
            var password = Console.ReadLine();
            var privateKey = File.ReadAllText("PrivateKey.txt");

            Console.Write("Enter Customer Name / Company: ");
            var customerName = Console.ReadLine();

            Console.Write("Enter Customer Machine ID: ");
            var machineId = Console.ReadLine();

            Console.Write("License Duration (in Years, e.g., 1): ");
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

            Console.WriteLine($"\n[SUCCESS] License file '{fileName}' created for {customerName}!");
            Console.WriteLine($"Machine ID: {machineId}");
            Console.WriteLine($"Expired: {DateTime.Now.AddYears(years):yyyy-MM-dd}");
        }
    }
}
