using Newtonsoft.Json;
using PasswordVaultUSB.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace PasswordVaultUSB.Services
{
    public class StorageService
    {
        public List<PasswordRecord> LoadData(string filePath, string password)
        {
            if (!File.Exists(filePath))
            {
                return new List<PasswordRecord>();
            }

            try
            {
                string encryptedJson = File.ReadAllText(filePath);

                string json = CryptoService.Decrypt(encryptedJson, password);

                var records = JsonConvert.DeserializeObject<List<PasswordRecord>>(json);
                return records ?? new List<PasswordRecord>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveData(string filePath, string password, IEnumerable<PasswordRecord> records)
        {
            try
            {
                string json = JsonConvert.SerializeObject(records);
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