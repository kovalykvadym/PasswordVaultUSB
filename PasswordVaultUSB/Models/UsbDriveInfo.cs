namespace PasswordVaultUSB.Models
{
    // Зберігає інформацію про підключену флешку
    public class UsbDriveInfo
    {
        public string Name { get; set; }
        public string RootDirectory { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}