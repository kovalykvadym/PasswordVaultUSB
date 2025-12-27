using System.Security;
namespace PasswordVaultUSB {
    public static class AppState {
        public static string CurrentUserFilePath { get; set; }
        public static SecureString CurrentMasterPassword { get; set; }
        public static string CurrentHardwareID { get; set; }
    }
}