using System;
using System.IO;
using System.Security.Cryptography;

namespace PasswordVaultUSB.Services
{
    // Сервіс для шифрування та дешифрування даних
    public static class CryptoService
    {
        private const int SaltSize = 32;       // Розмір "солі" для унікальності ключа
        private const int IvSize = 16;         // Розмір вектора ініціалізації(блочна реалізація AES)
        private const int Iterations = 100000; // Кількість проходів хешування

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
                    aes.Key = pbkdf2.GetBytes(32); // 32 байти = 256 біт
                }

                // 3. Генеруємо випадковий вектор ініціалізації
                aes.GenerateIV();
                byte[] iv = aes.IV;

                byte[] encryptedBytes;

                // 4. Шифруємо текст
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

                // 5. Формуємо фінальний пакет: [Salt] + [IV] + [EncryptedData]
                byte[] result = new byte[SaltSize + IvSize + encryptedBytes.Length];
                Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
                Buffer.BlockCopy(iv, 0, result, SaltSize, IvSize);
                Buffer.BlockCopy(encryptedBytes, 0, result, SaltSize + IvSize, encryptedBytes.Length);

                return result;
            }
        }

        public static string Decrypt(byte[] cipherData, string password)
        {
            // Перевірка цілісності даних
            if (cipherData == null || cipherData.Length < SaltSize + IvSize)
                throw new ArgumentException("Invalid data format");

            using (Aes aes = Aes.Create())
            {
                // 1. Розбираємо пакет назад: витягуємо Salt, IV та самі дані
                byte[] salt = new byte[SaltSize];
                byte[] iv = new byte[IvSize];
                byte[] actualCipher = new byte[cipherData.Length - SaltSize - IvSize];

                Buffer.BlockCopy(cipherData, 0, salt, 0, SaltSize);
                Buffer.BlockCopy(cipherData, SaltSize, iv, 0, IvSize);
                Buffer.BlockCopy(cipherData, SaltSize + IvSize, actualCipher, 0, actualCipher.Length);

                // 2. Відновлюємо ключ, використовуючи витягнуту сіль
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