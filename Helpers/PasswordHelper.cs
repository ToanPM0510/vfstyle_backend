using System;
using System.Security.Cryptography;
using System.Text;

namespace vfstyle_backend.Helpers
{
    public static class PasswordHelper
    {
        public static string ToHashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
        
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return ToHashPassword(password) == hashedPassword;
        }
        
        public static string RandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new StringBuilder(10);
            
            for (int i = 0; i < 10; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }
            
            return result.ToString();
        }
    }
}