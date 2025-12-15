namespace PasswordVaultUSB.Models
{
    // Налаштування користувача, що зберігаються всередині зашифрованого файлу
    public class UserSettings
    {
        public int AutoLockTimeout { get; set; } = 15;
        public int UsbCheckInterval { get; set; } = 3;

        public bool AutoClearClipboard { get; set; } = true;
        public bool ShowPasswordOnCopy { get; set; } = false;
        public bool ConfirmDeletions { get; set; } = true;
    }
}