using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Security.Cryptography;
using System.Management;
using System.Net;

namespace HyperTyk.Controllers.Auth
{
    public class AuthOTPManager
    {
        private string GetMachineId()
        {
            // Get MAC address of the first network interface
            string macAddress = "";
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if (mo["MacAddress"] != null)
                {
                    macAddress = mo["MacAddress"].ToString();
                    break;
                }
            }

            // Compute hash of the MAC address
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(macAddress));
                return Convert.ToBase64String(hashBytes);
            }
        }

        public string GetUserAgentHash(HttpContextBase context)
        {
            if (context == null || context.Request == null || string.IsNullOrEmpty(context.Request.UserAgent))
            {
                return string.Empty;
            }

            string userAgent = context.Request.UserAgent;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(userAgent));
                return Convert.ToBase64String(hashBytes);
            }
        }

        //public string GetClientIp(HttpContextBase context)
        //{
        //    string ip = context.Request.UserHostAddress;
        //    return ip;
        //}

        public string GenerateTokenWithExpiration(string id,int expirationMinutes, HttpContextBase context)
        {
            var guid = Guid.NewGuid().ToString();
            var machineId = GetMachineId();
            var userAgentHash = GetUserAgentHash(context);
            var expiration = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes).ToUnixTimeSeconds();

            var token = $"{id}&{guid}&{machineId}&{userAgentHash}&{expiration}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
        }

        public string GenerateOTP()
        {
            var randomGenerator = new Random();
            var otp = new StringBuilder();

            for (int i = 0; i < 6; i++)
            {
                otp.Append(randomGenerator.Next(1, 10).ToString());
            }

            return otp.ToString();
        }
        public bool IsTokenValid(string token, string userAgentHash)
        {
            try
            {
                var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = decodedToken.Split('&');

                if (parts.Length != 5) // Token format: userid:guid:machineId:userAgentHash:expiration
                {
                    return false;
                }

                var expiration = long.Parse(parts[4]);
                if (expiration <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    return false; // Token has expired
                }

                // Validate machine ID and user agent hash
                if (parts[2] != GetMachineId() || parts[3] != userAgentHash)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetUserIdfromToken(string token)
        {
            try
            {
                var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = decodedToken.Split('&');

                if (parts.Length != 5) // Token format: id:guid:machineId:userAgentHash:expiration
                {
                    return null; // Indicate invalid token 
                }

                // Extract the user ID (first part)
                return parts[0];
            }
            catch (Exception)
            {
                return null; // Indicate error during extraction
            }
        }
        public long GetCurrentEpochTime()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            return now.ToUnixTimeSeconds();
        }
    }
}