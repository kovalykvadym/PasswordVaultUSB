using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordVaultUSB.Services
{
    public static class AppSettings
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PVault",
            "settings.json"
        );

        // Налаштування безпеки
        public static int AutoLockTimeout { get; set; } = 15; // хвилини (0 = ніколи)
        public static int UsbCheckInterval { get; set; } = 3; // секунди
        public static bool AutoClearClipboard { get; set; } = true;

        // Налаштування відображення
        public static bool ShowPasswordOnCopy { get; set; } = false;
        public static bool ConfirmDeletions { get; set; } = true;

        static AppSettings()
        {
            LoadSettings();
        }

        public static void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonConvert.DeserializeObject<SettingsData>(json);

                    if (settings != null)
                    {
                        AutoLockTimeout = settings.AutoLockTimeout;
                        UsbCheckInterval = settings.UsbCheckInterval;
                        AutoClearClipboard = settings.AutoClearClipboard;
                        ShowPasswordOnCopy = settings.ShowPasswordOnCopy;
                        ConfirmDeletions = settings.ConfirmDeletions;
                    }
                }
            }
            catch (Exception ex)
            {
                // Якщо не вдалося завантажити, використовуємо значення за замовчуванням
                Console.WriteLine($"Failed to load settings: {ex.Message}");
            }
        }

        public static void SaveSettings()
        {
            try
            {
                var settings = new SettingsData
                {
                    AutoLockTimeout = AutoLockTimeout,
                    UsbCheckInterval = UsbCheckInterval,
                    AutoClearClipboard = AutoClearClipboard,
                    ShowPasswordOnCopy = ShowPasswordOnCopy,
                    ConfirmDeletions = ConfirmDeletions
                };

                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save settings: {ex.Message}");
            }
        }

        private class SettingsData
        {
            public int AutoLockTimeout { get; set; }
            public int UsbCheckInterval { get; set; }
            public bool AutoClearClipboard { get; set; }
            public bool ShowPasswordOnCopy { get; set; }
            public bool ConfirmDeletions { get; set; }
        }
    }
}