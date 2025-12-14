namespace PasswordVaultUSB.Services
{
    public static class AppSettings
    {
        // Тут зберігаються налаштування АКТИВНОГО користувача
        // За замовчуванням встановлюємо дефолтні значення
        public static int AutoLockTimeout { get; set; } = 15;
        public static int UsbCheckInterval { get; set; } = 3;
        public static bool AutoClearClipboard { get; set; } = true;
        public static bool ShowPasswordOnCopy { get; set; } = false;
        public static bool ConfirmDeletions { get; set; } = true;

        // Метод для оновлення поточного стану з завантажених даних
        public static void ApplySettings(Models.UserSettings settings)
        {
            if (settings == null) return;

            AutoLockTimeout = settings.AutoLockTimeout;
            UsbCheckInterval = settings.UsbCheckInterval;
            AutoClearClipboard = settings.AutoClearClipboard;
            ShowPasswordOnCopy = settings.ShowPasswordOnCopy;
            ConfirmDeletions = settings.ConfirmDeletions;
        }

        // Метод для отримання поточного стану для збереження
        public static Models.UserSettings GetCurrentSettings()
        {
            return new Models.UserSettings
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