using System.Collections.Generic;

namespace PasswordVaultUSB.Models
{
    public class VaultData
    {
        public string HardwareID { get; set; }
        public UserSettings Settings { get; set; } = new UserSettings();
        public List<PasswordRecord> Records { get; set; } = new List<PasswordRecord>();
    }
}