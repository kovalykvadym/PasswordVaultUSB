using System.IO;
using System.Linq;

namespace PasswordVaultUSB.Services
{
    public static class UsbDriveService
    {
        public static string GetUsbPath()
        {
            var drives = DriveInfo.GetDrives();
            var usbDrive = drives.FirstOrDefault(d => d.DriveType == DriveType.Removable && d.IsReady);

            return usbDrive?.RootDirectory.FullName;
        }

        public static string CreateVaultFolder(string usbRootPath)
        {
            string folderPath = Path.Combine(usbRootPath, ".PasswordVaultData");

            if (!Directory.Exists(folderPath))
            {
                DirectoryInfo di = Directory.CreateDirectory(folderPath);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            return folderPath;
        }
    }
}