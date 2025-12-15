using System.Collections.Generic;

namespace PasswordVaultUSB.Models
{
    // Головний клас структури даних, що зберігається у зашифрованому файлі
    public class VaultData
    {
        // Прив'язка до "заліза" (серійний номер флешки для захисту від копіювання)
        public string HardwareID { get; set; }

        // Збережені налаштування користувача
        public UserSettings Settings { get; set; } = new UserSettings();

        // Список усіх паролів
        public List<PasswordRecord> Records { get; set; } = new List<PasswordRecord>();
    }
}