using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PasswordVaultUSB.Services
{
    public static class PasswordGeneratorService
    {
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private const string Symbols = "!@#$%^&*()_-+=<>?";

        public static string GeneratePassword(int length, bool useLower, bool useUpper, bool useDigits, bool useSymbols)
        {
            if (length < 4) length = 4;
            if (!useLower && !useUpper && !useDigits && !useSymbols)
            {
                useLower = true;
                useDigits = true;
            }

            var charSet = new StringBuilder();
            var password = new StringBuilder();
            var random = RandomNumberGenerator.Create();

            if (useLower)
            {
                password.Append(GetRandomChar(Lowercase, random));
                charSet.Append(Lowercase);
            }
            if (useUpper)
            {
                password.Append(GetRandomChar(Uppercase, random));
                charSet.Append(Uppercase);
            }
            if (useDigits)
            {
                password.Append(GetRandomChar(Digits, random));
                charSet.Append(Digits);
            }
            if (useSymbols)
            {
                password.Append(GetRandomChar(Symbols, random));
                charSet.Append(Symbols);
            }

            string fullCharSet = charSet.ToString();
            while (password.Length < length)
            {
                password.Append(GetRandomChar(fullCharSet, random));
            }

            return ShuffleString(password.ToString(), random);
        }

        private static char GetRandomChar(string charSet, RandomNumberGenerator rng)
        {
            byte[] buffer = new byte[4];
            rng.GetBytes(buffer);
            uint num = BitConverter.ToUInt32(buffer, 0);
            return charSet[(int)(num % (uint)charSet.Length)];
        }

        private static string ShuffleString(string str, RandomNumberGenerator rng)
        {
            char[] array = str.ToCharArray();
            int n = array.Length;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do rng.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                (array[n], array[k]) = (array[k], array[n]);
            }
            return new string(array);
        }
    }
}