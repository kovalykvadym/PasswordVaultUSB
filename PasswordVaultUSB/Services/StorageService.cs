using Newtonsoft.Json;
using PasswordVaultUSB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PasswordVaultUSB.Services
{
    public class StorageService
    {
        public async Task<List<PasswordRecord>> LoadDataAsync(string filePath, string password, string currentHardwareId)
        {
            if (!File.Exists(filePath))
            {
                return new List<PasswordRecord>();
            }

            try
            {
                byte[] encryptedBytes;

                using (FileStream sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                {
                    encryptedBytes = new byte[sourceStream.Length];
                    await sourceStream.ReadAsync(encryptedBytes, 0, (int)sourceStream.Length);
                }

                return await Task.Run(() =>
                {
                    string json = CryptoService.Decrypt(encryptedBytes, password);
                    var vaultData = JsonConvert.DeserializeObject<VaultData>(json);

                    if (vaultData == null) return new List<PasswordRecord>();

                    if (vaultData.HardwareID != currentHardwareId)
                    {
                        throw new UnauthorizedAccessException("Hardware Mismatch! This file belongs to another USB drive.");
                    }

                    return vaultData.Records ?? new List<PasswordRecord>();
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task SaveDataAsync(string filePath, string password, IEnumerable<PasswordRecord> records, string currentHardwareId)
        {
            try
            {
                byte[] encryptedBytes = await Task.Run(() =>
                {
                    var dataToSave = new VaultData
                    {
                        HardwareID = currentHardwareId,
                        Records = new List<PasswordRecord>(records)
                    };

                    string json = JsonConvert.SerializeObject(dataToSave);
                    return CryptoService.Encrypt(json, password);
                });

                using (FileStream sourceStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                {
                    await sourceStream.WriteAsync(encryptedBytes, 0, encryptedBytes.Length);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}