using System.Security;

namespace PasswordVaultUSB
{
    // Зберігає дані, які потрібні у всіх частинах програми
    public static class AppState
    {
        public static string CurrentUserFilePath { get; set; }
        public static SecureString CurrentMasterPassword { get; set; }
        public static string CurrentHardwareID { get; set; }
    }
}