using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PasswordVaultUSB.Services
{
    public static class CryptoService
    {
        private const int SaltSize = 32; // 256 біт
        private const int IvSize = 16;   // 128 біт (стандарт для AES)
        private const int Iterations = 100000; // Кількість ітерацій для PBKDF2

        // Метод повертає масив байтів, а не рядок, для точності даних
        public static byte[] Encrypt(string plainText, string password)
        {
            if (string.IsNullOrEmpty(plainText)) return Array.Empty<byte>();

            using (Aes aes = Aes.Create())
            {
                // 1. Генеруємо випадкову сіль
                byte[] salt = new byte[SaltSize];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                // 2. Генеруємо ключ із пароля та солі
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    aes.Key = pbkdf2.GetBytes(32); // AES-256
                }

                // 3. Генеруємо випадковий IV
                aes.GenerateIV();
                byte[] iv = aes.IV;

                // 4. Шифруємо
                byte[] encryptedBytes;
                using (var encryptor = aes.CreateEncryptor())
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    encryptedBytes = msEncrypt.ToArray();
                }

                // 5. Комбінуємо все в один масив: [Salt] + [IV] + [Cipher]
                byte[] result = new byte[SaltSize + IvSize + encryptedBytes.Length];
                Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
                Buffer.BlockCopy(iv, 0, result, SaltSize, IvSize);
                Buffer.BlockCopy(encryptedBytes, 0, result, SaltSize + IvSize, encryptedBytes.Length);

                return result;
            }
        }

        public static string Decrypt(byte[] cipherData, string password)
        {
            if (cipherData == null || cipherData.Length < SaltSize + IvSize)
                throw new ArgumentException("Invalid data format");

            using (Aes aes = Aes.Create())
            {
                // 1. Витягуємо Сіль та IV з початку масиву
                byte[] salt = new byte[SaltSize];
                byte[] iv = new byte[IvSize];
                byte[] actualCipher = new byte[cipherData.Length - SaltSize - IvSize];

                Buffer.BlockCopy(cipherData, 0, salt, 0, SaltSize);
                Buffer.BlockCopy(cipherData, SaltSize, iv, 0, IvSize);
                Buffer.BlockCopy(cipherData, SaltSize + IvSize, actualCipher, 0, actualCipher.Length);

                // 2. Відновлюємо ключ
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    aes.Key = pbkdf2.GetBytes(32);
                }
                aes.IV = iv;

                // 3. Дешифруємо
                using (var decryptor = aes.CreateDecryptor())
                using (var msDecrypt = new MemoryStream(actualCipher))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
    }
}