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

            Console.WriteLine("\nSelect License Type:");
            Console.WriteLine("1. Trial (7 Days)");
            Console.WriteLine("2. Annual (1 Year)");
            Console.WriteLine("3. Perpetual (Permanent)");
            Console.Write("Choice (1/2/3): ");
            var typeChoice = Console.ReadLine();
            var licenseType = typeChoice == "1" ? "Trial" : (typeChoice == "3" ? "Perpetual" : "Annual");
            var expiration = licenseType == "Trial" ? DateTime.Now.AddDays(7) : (licenseType == "Perpetual" ? DateTime.Now.AddYears(100) : DateTime.Now.AddYears(1));

            Console.WriteLine("\nSelect License Tier:");
            Console.WriteLine("1. Core (20 Beacons, 5 Readers)");
            Console.WriteLine("2. Professional (200 Beacons, 50 Readers)");
            Console.WriteLine("3. Enterprise (Unlimited)");
            Console.WriteLine("4. Custom (Manual Override)");
            Console.Write("Choice (1/2/3/4): ");
            var tierChoice = Console.ReadLine();
            var tier = tierChoice == "2" ? "Professional" : (tierChoice == "3" ? "Enterprise" : (tierChoice == "4" ? "Custom" : "Core"));
            
            int maxBeacons, maxReaders;
            if (tier == "Custom")
            {
                Console.Write("Enter Max Beacons limit: ");
                if (!int.TryParse(Console.ReadLine(), out maxBeacons)) maxBeacons = 20;
                Console.Write("Enter Max Readers limit: ");
                if (!int.TryParse(Console.ReadLine(), out maxReaders)) maxReaders = 5;
            }
            else
            {
                maxBeacons = tier == "Professional" ? 200 : (tier == "Enterprise" ? 999999 : 20);
                maxReaders = tier == "Professional" ? 50 : (tier == "Enterprise" ? 999999 : 5);
            }

            Console.WriteLine("\nEnable Optional Features?");
            Console.Write("Enable AD Sync? (y/n): ");
            bool enableAd = Console.ReadLine()?.ToLower() == "y";
            Console.Write("Enable SSO? (y/n): ");
            bool enableSso = Console.ReadLine()?.ToLower() == "y";

            var features = new List<string>();
            if (enableAd) features.Add("module.activeDirectory");
            if (enableSso) features.Add("module.sso");

            var attributes = new Dictionary<string, string>
            {
                { "MachineID", machineId ?? "" },
                { "LicenseType", licenseType },
                { "LicenseTier", tier },
                { "MaxBeacons", maxBeacons.ToString() },
                { "MaxReaders", maxReaders.ToString() },
                { "Features", string.Join(",", features) }
            };

            var license = License.New()
                .WithUniqueIdentifier(Guid.NewGuid())
                .As(LicenseType.Standard)
                .ExpiresAt(expiration)
                .WithAdditionalAttributes(attributes)
                .LicensedTo(customerName, "info@customer.com")
                .CreateAndSignWithPrivateKey(privateKey, password);

            string fileName = $"license_{customerName?.Replace(" ", "")}_{tier}.lic";
            File.WriteAllText(fileName, license.ToString());

            Console.WriteLine($"\n[SUCCESS] License file '{fileName}' created!");
            Console.WriteLine($"Customer: {customerName}");
            Console.WriteLine($"Machine ID: {machineId}");
            Console.WriteLine($"Type: {licenseType}, Tier: {tier}");
            Console.WriteLine($"Limits: {maxBeacons} Beacons, {maxReaders} Readers");
            Console.WriteLine($"Features: {(features.Count > 0 ? string.Join(", ", features) : "None")}");
            Console.WriteLine($"Expired: {expiration:yyyy-MM-dd}");
        }
    }
}
