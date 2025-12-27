using System;
using System.Runtime.InteropServices;
using System.Security;
namespace PasswordVaultUSB.Helpers {
    public static class SecureStringHelper {
        public static SecureString ToSecureString(string plainText) {
            if (plainText == null) return null;

            var secure = new SecureString();
            foreach (char c in plainText) {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }
        public static string ToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null) return null;

            IntPtr unmanagedString = IntPtr.Zero;
            try {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            } finally {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}