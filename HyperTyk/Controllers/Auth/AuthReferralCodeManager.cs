using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HyperTyk.Controllers.Auth
{
    public class AuthReferralCodeManager
    {
        private static readonly Random random = new Random();

        public static string GenerateReferralCode()
        {
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            string randomPart = new string(Enumerable.Repeat(characters, 5)
                                     .Select(s => s[random.Next(s.Length)])
                                     .ToArray());

            return $"HYPRFYP-{randomPart}";
        }
    }
}