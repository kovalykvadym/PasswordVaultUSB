using System;
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

            // Якщо нічого не вибрано, примусово вмикаємо малі літери та цифри
            if (!useLower && !useUpper && !useDigits && !useSymbols)
            {
                useLower = true;
                useDigits = true;
            }

            var charSet = new StringBuilder();
            var password = new StringBuilder();

            // Використовуємо using для правильного звільнення ресурсів крипто-генератора
            using (var rng = RandomNumberGenerator.Create())
            {
                // 1. Спочатку гарантуємо, що в паролі буде хоча б по одному символу з кожної обраної групи
                if (useLower)
                {
                    password.Append(GetRandomChar(Lowercase, rng));
                    charSet.Append(Lowercase);
                }
                if (useUpper)
                {
                    password.Append(GetRandomChar(Uppercase, rng));
                    charSet.Append(Uppercase);
                }
                if (useDigits)
                {
                    password.Append(GetRandomChar(Digits, rng));
                    charSet.Append(Digits);
                }
                if (useSymbols)
                {
                    password.Append(GetRandomChar(Symbols, rng));
                    charSet.Append(Symbols);
                }

                // 2. Доповнюємо решту довжини випадковими символами з усіх доступних
                string fullCharSet = charSet.ToString();
                while (password.Length < length)
                {
                    password.Append(GetRandomChar(fullCharSet, rng));
                }

                // 3. Перемішуємо результат, щоб гарантовані символи не йшли підряд на початку
                return ShuffleString(password.ToString(), rng);
            }
        }

        private static char GetRandomChar(string charSet, RandomNumberGenerator rng)
        {
            byte[] buffer = new byte[4];
            rng.GetBytes(buffer);
            uint num = BitConverter.ToUInt32(buffer, 0);

            return charSet[(int)(num % (uint)charSet.Length)];
        }

        // Реалізація алгоритму тасування Фішера-Єйтса
        private static string ShuffleString(string str, RandomNumberGenerator rng)
        {
            char[] array = str.ToCharArray();
            int n = array.Length;

            while (n > 1)
            {
                byte[] box = new byte[1];
                do
                {
                    rng.GetBytes(box);
                }
                while (!(box[0] < n * (byte.MaxValue / n))); // Захист від зміщення ймовірності (Modulo Bias)

                int k = (box[0] % n);
                n--;
                (array[n], array[k]) = (array[k], array[n]);
            }

            return new string(array);
        }
    }
}