using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace HyperTyk.Controllers.Auth
{
    public class AuthUUIDManager
    {
        public static string GenerateUUID(string username)
        {
            string baseString = DateTime.UtcNow.Ticks.ToString() + username;

            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(baseString));

                var guidBytes = hashBytes.Take(16).ToArray();

                var hashGuid = new Guid(guidBytes);
                return hashGuid.ToString();
            }
        }
    }
}