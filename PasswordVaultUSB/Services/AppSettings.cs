using PasswordVaultUSB.Models;

namespace PasswordVaultUSB.Services
{
    // Статичний клас, що тримає актуальні налаштування в пам'яті під час роботи програми
    public static class AppSettings
    {
        // Значення за замовчуванням
        public static int AutoLockTimeout { get; set; } = 15;
        public static int UsbCheckInterval { get; set; } = 3;

        public static bool AutoClearClipboard { get; set; } = true;
        public static bool ShowPasswordOnCopy { get; set; } = false;
        public static bool ConfirmDeletions { get; set; } = true;

        // Застосовує налаштування, завантажені з файлу
        public static void ApplySettings(UserSettings settings)
        {
            if (settings == null) return;

            AutoLockTimeout = settings.AutoLockTimeout;
            UsbCheckInterval = settings.UsbCheckInterval;
            AutoClearClipboard = settings.AutoClearClipboard;
            ShowPasswordOnCopy = settings.ShowPasswordOnCopy;
            ConfirmDeletions = settings.ConfirmDeletions;
        }

        // Збирає поточні налаштування в об'єкт для збереження
        public static UserSettings GetCurrentSettings()
        {
            return new UserSettings
            {
                AutoLockTimeout = AutoLockTimeout,
                UsbCheckInterval = UsbCheckInterval,
                AutoClearClipboard = AutoClearClipboard,
                ShowPasswordOnCopy = ShowPasswordOnCopy,
                ConfirmDeletions = ConfirmDeletions
            };
        }
    }
}