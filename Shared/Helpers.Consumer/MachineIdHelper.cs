using System;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;

namespace Helpers.Consumer
{
    /// <summary>
    /// Helper for generating unique machine identifiers
    /// Used for license binding to specific hardware
    /// </summary>
    public static class MachineIdHelper
    {
        /// <summary>
        /// Generate a unique machine ID based on hardware characteristics
        /// </summary>
        public static string GenerateMachineId()
        {
            string cpuInfo = string.Empty;
            try
            {
#if WINDOWS
                // Windows: Try to use WMI to get CPU ID (if available)
                try
                {
                    var searcher = new System.Management.ManagementObjectSearcher("select ProcessorId from Win32_Processor");
                    foreach (var obj in searcher.Get())
                    {
                        cpuInfo = obj["ProcessorId"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(cpuInfo))
                            break;
                    }
                }
                catch
                {
                    // WMI not available, fall back
                    cpuInfo = Environment.MachineName + Environment.UserName;
                }
#else
                // Non-Windows: Use hostname and username
                cpuInfo = Environment.MachineName + Environment.UserName;
#endif
            }
            catch (Exception)
            {
                // Fallback if WMI fails or lacks access permissions
                cpuInfo = Environment.MachineName + Environment.UserName;
            }

            if (string.IsNullOrEmpty(cpuInfo))
            {
                cpuInfo = Environment.MachineName; // Default to hostname fallback
            }

            // Hash the result for a clean, consistent identifier
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(cpuInfo));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
