
using System;
using System.Security.Cryptography;
using System.Text;

namespace Hash
{
    internal class Hash
    {
        public static string GenerateSalt()
        {
            var randomBytes = new byte[24];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
        public static string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = string.Concat(password, salt); 
                var salterPasswordBytes = Encoding.UTF8.GetBytes(saltedPassword);
                var hash = sha256.ComputeHash(salterPasswordBytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
