using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordVaultUSB.Models
{
    public static class AppState
    {
        public static string CurrentUserFilePath { get; set; }
        public static string CurrentMasterPassword { get; set; }
    }
}
