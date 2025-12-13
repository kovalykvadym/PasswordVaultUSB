using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PasswordVaultUSB.Services
{
    public static class CryptoService
    {
        private const int SaltSize = 16;
        private const int Iterations = 10000;

        public static string Encrypt(string plainText, string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] clearBytes = Encoding.Unicode.GetBytes(plainText);

            using (Aes encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(password, salt, Iterations);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(salt, 0, salt.Length);

                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText, string password)
        {
            cipherText = cipherText.Replace(" ", "+");
            byte[] fullCipherBytes = Convert.FromBase64String(cipherText);

            byte[] salt = new byte[SaltSize];
            if (fullCipherBytes.Length < SaltSize)
            {
                throw new ArgumentException("Encrypted data is too short/corrupted.");
            }
            Array.Copy(fullCipherBytes, 0, salt, 0, SaltSize);

            using (Aes encryptor = Aes.Create())
            {
                var pdb = new Rfc2898DeriveBytes(password, salt, Iterations);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(fullCipherBytes, SaltSize, fullCipherBytes.Length - SaltSize);
                        cs.Close();
                    }
                    return Encoding.Unicode.GetString(ms.ToArray());
                }
            }
        }
    }
}