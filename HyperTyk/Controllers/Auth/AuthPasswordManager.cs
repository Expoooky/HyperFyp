using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HyperTyk.Controllers.Auth
{
    public class AuthPasswordManager
    {
        public static string GenerateSalt()
        {
            const int saltLength = 22;
            const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789./";

            Random random = new Random();
            char[] saltChars = new char[saltLength];

            for (int i = 0; i < saltLength; i++)
            {
                saltChars[i] = allowedChars[random.Next(0, allowedChars.Length)];
            }

            string salt = new string(saltChars);
            return salt;
        }

        public static string HashPassword(string pinput)
        {
            string salt = "$2a$" + 13 + "$" + GenerateSalt();
            return DevOne.Security.Cryptography.BCrypt.BCryptHelper.HashPassword(pinput, salt);
        }

        public static bool VerifyPassword(string pinput, string hashedp) => DevOne.Security.Cryptography.BCrypt.BCryptHelper.CheckPassword(pinput, hashedp);
    }
}