using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace LicenseChecker
{
    public static class MachineIdHelper
    {
        public static string GenerateMachineId()
        {
            string cpuInfo = string.Empty;
            try
            {
                ManagementClass mc = new ManagementClass("win32_processor");
                ManagementObjectCollection moc = mc.GetInstances();

                foreach (ManagementObject mo in moc)
                {
                    if (string.IsNullOrEmpty(cpuInfo))
                    {
                        cpuInfo = mo.Properties["processorID"].Value.ToString();
                        break;
                    }
                }
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
