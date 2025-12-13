using Newtonsoft.Json;
using PasswordVaultUSB.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace PasswordVaultUSB.Services
{
    public class StorageService
    {
        public List<PasswordRecord> LoadData(string filePath, string password, string currentHardwareId)
        {
            if (!File.Exists(filePath))
            {
                return new List<PasswordRecord>();
            }

            try
            {
                string encryptedJson = File.ReadAllText(filePath);
                string json = CryptoService.Decrypt(encryptedJson, password);

                var vaultData = JsonConvert.DeserializeObject<VaultData>(json);

                if (vaultData == null) return new List<PasswordRecord>();

                if (vaultData.HardwareID != currentHardwareId)
                {
                    throw new UnauthorizedAccessException("Hardware Mismatch! This file belongs to another USB drive.");
                }

                return vaultData.Records ?? new List<PasswordRecord>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveData(string filePath, string password, IEnumerable<PasswordRecord> records, string currentHardwareId)
        {
            try
            {
                var dataToSave = new VaultData
                {
                    HardwareID = currentHardwareId,
                    Records = new List<PasswordRecord>(records)
                };

                string json = JsonConvert.SerializeObject(dataToSave);
                string encryptedJson = CryptoService.Encrypt(json, password);

                File.WriteAllText(filePath, encryptedJson);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}