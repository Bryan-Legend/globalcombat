using System;
using System.Security.Cryptography;
using System.Text;

namespace LT
{
    public static class UserPage<TAccountId> where TAccountId : new()
    {
        static readonly SHA512 hasher = SHA512.Create();

        public static string CalculateHash(string input)
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes("s&~D$L{a8_" + input);
            byte[] hash = hasher.ComputeHash(inputBytes);
            return Convert.ToBase64String(hash);
        }

        static readonly char[] pwdCharArray = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        static readonly Random random = new Random();

        public static string GeneratePassword(int length)
        {
            var result = new StringBuilder();
            for (int i = 0; i < length; i++)
                result.Append(pwdCharArray[random.Next(pwdCharArray.Length)]);
            return result.ToString();
        }
    }
}
