using System;
using System.Runtime.InteropServices;
using System.Security;

namespace PasswordVaultUSB.Helpers
{
    public static class SecureStringHelper
    {
        // Перетворення звичайного string у SecureString
        public static SecureString ToSecureString(string plainText)
        {
            if (plainText == null) return null;

            var secure = new SecureString();
            foreach (char c in plainText)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        // Безпечне перетворення SecureString у звичайний string
        public static string ToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null) return null;

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}