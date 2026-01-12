using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BusinessLogic.Services.Extension.Encrypt
{
    public class EncryptService : IEncryptService
    {
        private readonly IConfiguration _configuration;
    

        public EncryptService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // public string Encrypt(string plaintext)
        // {
        //     try
        //     {
        //         string key = _configuration["Encryption:Key"] ?? throw new ArgumentNullException("Encryption:Key is not configured."); // 32-byte key
        //         string iv = _configuration["Encryption:IV"] ?? throw new ArgumentNullException("Encryption:IV is not configured."); // 16-byte IV

        //         // cek length key dan iv
        //         if (key.Length != 32 || iv.Length != 16)
        //         {
        //             throw new Exception("Key and IV must be 32 and 16 characters long, respectively.");
        //         }

        //         using Aes aes = Aes.Create();
        //         aes.Key = Encoding.UTF8.GetBytes(key);
        //         aes.IV = Encoding.UTF8.GetBytes(iv);
        //         aes.Mode = CipherMode.CBC;
        //         aes.Padding = PaddingMode.PKCS7;

        //         using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        //         byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
        //         byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        //         string encryptedHex = BitConverter.ToString(encryptedBytes).Replace("-", "").ToLower(); // Hex format
        //         string convertToHex = HexToBase64(encryptedHex); // Base64 format
        //         string convertToBase64 = ConvertBtoa(convertToHex); // Convert to Base64
                
        //         return convertToBase64;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine(ex.Message);
        //         return ex.Message;
        //     }
        // }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            var key = Convert.FromBase64String(
                _configuration["Encryption:Key"] ?? Environment.GetEnvironmentVariable("ENCRYPTION_KEY") // 32 bytes BASE64
            );

            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV(); // 🔥 RANDOM
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(
                plainBytes, 0, plainBytes.Length
            );

            var output = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, output, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, output, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(output);
        }


        // Fungsi Decrypt
        // public string Decrypt(string ciphertext)
        // {
        //     try
        //     {
        //         string key = _configuration["Encryption:Key"] ?? throw new ArgumentNullException("Encryption:Key is not configured."); // 32-byte key
        //         string iv = _configuration["Encryption:IV"] ?? throw new ArgumentNullException("Encryption:IV is not configured."); // 16-byte IV

        //         // Cek panjang key dan IV
        //         if (key.Length != 32 || iv.Length != 16)
        //         {
        //             throw new Exception("Key and IV must be 32 and 16 characters long, respectively.");
        //         }

        //         // Balik dari ConvertBtoa ke Base64
        //         string base64String = ConvertAtob(ciphertext);

        //         // Balik dari Base64 ke Hex
        //         string hexString = Base64ToHex(base64String);

        //         // Balik dari Hex ke Byte Array
        //         byte[] encryptedBytes = HexToBytes(hexString);

        //         // AES Decrypt
        //         using Aes aes = Aes.Create();
        //         aes.Key = Encoding.UTF8.GetBytes(key);
        //         aes.IV = Encoding.UTF8.GetBytes(iv);
        //         aes.Mode = CipherMode.CBC;
        //         aes.Padding = PaddingMode.PKCS7;

        //         using ICryptoTransform decryptor = aes.CreateDecryptor();
        //         byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        //         string decryptedText = Encoding.UTF8.GetString(decryptedBytes);

        //         return decryptedText;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine(ex.Message);
        //         return ex.Message;
        //     }
        // }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            var key = Convert.FromBase64String(
                _configuration["Encryption:Key"]
                ?? throw new Exception("Encryption:Key missing")
            );

            var fullBytes = Convert.FromBase64String(cipherText);

            var iv = fullBytes.Take(16).ToArray();
            var cipherBytes = fullBytes.Skip(16).ToArray();

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(
                cipherBytes, 0, cipherBytes.Length
            );

            return Encoding.UTF8.GetString(plainBytes);
        }


        // Convert Base64 yang diubah dengan ConvertBtoa ke normal
        public string ConvertAtob(string base64Custom)
        {
            var tableStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            var table = tableStr.ToCharArray();
            var bin = new List<byte>();

            for (int i = 0; i < base64Custom.Length; i += 4)
            {
                int a = Array.IndexOf(table, base64Custom[i]);
                int b = Array.IndexOf(table, base64Custom[i + 1]);
                int c = base64Custom[i + 2] == '=' ? 0 : Array.IndexOf(table, base64Custom[i + 2]);
                int d = base64Custom[i + 3] == '=' ? 0 : Array.IndexOf(table, base64Custom[i + 3]);

                bin.Add((byte)((a << 2) | (b >> 4)));
                if (base64Custom[i + 2] != '=') bin.Add((byte)(((b & 15) << 4) | (c >> 2)));
                if (base64Custom[i + 3] != '=') bin.Add((byte)(((c & 3) << 6) | d));
            }

            return Encoding.UTF8.GetString(bin.ToArray());
        }


        // Convert dari Base64 ke Hex
        public string Base64ToHex(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        // Helper method untuk mengkonversi string hex ke byte array
        public static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Panjang string hex harus genap.", nameof(hex));

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string byteValue = hex.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(byteValue, 16);
            }
            return bytes;
        }

        public string ConvertBtoa(string bin)
        {
            var tableStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";  // Tabel Base64
            var table = tableStr.ToCharArray();  // Ubah string ke dalam array karakter

            var base64 = new StringBuilder(); // Untuk menyimpan hasil Base64
            int len = bin.Length / 3;

            for (int i = 0, j = 0; i < len; ++i)
            {
                byte a = (byte)bin[j++];
                byte b = (byte)bin[j++];
                byte c = (byte)bin[j++];

                if ((a | b | c) > 255)
                {
                    throw new Exception("String contains an invalid character");
                }

                base64.Append(table[a >> 2]);
                base64.Append(table[((a << 4) & 63) | (b >> 4)]);

                base64.Append(IsNaN(b) ? "=" : table[((b << 2) & 63) | (c >> 6)]);
                base64.Append(IsNaN((byte)(b + c)) ? "=" : table[c & 63]);
            }

            return base64.ToString();
        }

        private static bool IsNaN(byte value)
        {
            return value == 0;
        }

        public string HexToBase64(string hex)
        {
            byte[] bytes = HexToBytes(hex); // Mengonversi HEX ke byte array
            return Convert.ToBase64String(bytes); // Mengonversi byte array ke Base64
        }

        // Fungsi untuk mengonversi HEX ke byte array
        private byte[] HexToBytes(string hex)
        {
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}