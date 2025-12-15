using Newtonsoft.Json;
using PasswordVaultUSB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PasswordVaultUSB.Services
{
    // Сервіс для збереження та завантаження бази даних
    public class StorageService
    {
        public async Task<List<PasswordRecord>> LoadDataAsync(string filePath, string password, string currentHardwareId)
        {
            if (!File.Exists(filePath))
            {
                return new List<PasswordRecord>();
            }

            // 1. Читаємо зашифрований файл у пам'ять
            byte[] encryptedBytes;
            using (FileStream sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                encryptedBytes = new byte[sourceStream.Length];
                await sourceStream.ReadAsync(encryptedBytes, 0, (int)sourceStream.Length);
            }

            // 2. Дешифруємо та розбираємо JSON у фоновому потоці
            return await Task.Run(() =>
            {
                string json = CryptoService.Decrypt(encryptedBytes, password);

                var vaultData = JsonConvert.DeserializeObject<VaultData>(json);

                if (vaultData == null) return new List<PasswordRecord>();

                // 3. ПЕРЕВІРКА БЕЗПЕКИ Це захищає від копіювання файлу бази на інший носій.
                if (vaultData.HardwareID != currentHardwareId)
                {
                    throw new UnauthorizedAccessException("Hardware Mismatch! This file belongs to a different USB drive.");
                }

                // 4. Застосовуємо завантажені налаштування програми
                AppSettings.ApplySettings(vaultData.Settings);

                return vaultData.Records ?? new List<PasswordRecord>();
            });
        }

        public async Task SaveDataAsync(string filePath, string password, IEnumerable<PasswordRecord> records, string currentHardwareId)
        {
            // 1. Підготовка даних та шифрування
            byte[] encryptedBytes = await Task.Run(() =>
            {
                var dataToSave = new VaultData
                {
                    HardwareID = currentHardwareId,
                    Records = new List<PasswordRecord>(records),
                    Settings = AppSettings.GetCurrentSettings() // Зберігаємо поточні налаштування
                };

                string json = JsonConvert.SerializeObject(dataToSave);
                return CryptoService.Encrypt(json, password);
            });

            // 2. Запис на диск
            using (FileStream sourceStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                await sourceStream.WriteAsync(encryptedBytes, 0, encryptedBytes.Length);
            }
        }
    }
}