using PasswordVaultUSB.Models;
namespace PasswordVaultUSB.Services {
    public static class AppSettings {
        public static int AutoLockTimeout { get; set; } = 15;
        public static int UsbCheckInterval { get; set; } = 3;
        public static bool AutoClearClipboard { get; set; } = true;
        public static bool ShowPasswordOnCopy { get; set; } = false;
        public static bool ConfirmDeletions { get; set; } = true;
        public static void ApplySettings(UserSettings settings) {
            if (settings == null) return;

            AutoLockTimeout = settings.AutoLockTimeout;
            UsbCheckInterval = settings.UsbCheckInterval;
            AutoClearClipboard = settings.AutoClearClipboard;
            ShowPasswordOnCopy = settings.ShowPasswordOnCopy;
            ConfirmDeletions = settings.ConfirmDeletions;
        }
        public static UserSettings GetCurrentSettings() {
            return new UserSettings {
                AutoLockTimeout = AutoLockTimeout,
                UsbCheckInterval = UsbCheckInterval,
                AutoClearClipboard = AutoClearClipboard,
                ShowPasswordOnCopy = ShowPasswordOnCopy,
                ConfirmDeletions = ConfirmDeletions
            };
        }
    }
}