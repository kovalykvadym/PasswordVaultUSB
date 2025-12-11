using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordVaultUSB.Services
{
    public static class UsbDriveService
    {
        public static string GetUsbPath()
        {
            var drives = DriveInfo.GetDrives();

            var usbDrive = drives.FirstOrDefault(d => d.DriveType == DriveType.Removable && d.IsReady);

            if (usbDrive !=  null)
            {
                return usbDrive.RootDirectory.FullName;
            }

            return null;
        }

        public static string CreateVaultFolder(string usbRootPath)
        {
            string folderPath = Path.Combine(usbRootPath, ".PasswordVauldData");

            if (!Directory.Exists(folderPath))
            {
                DirectoryInfo di = Directory.CreateDirectory(folderPath);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            return folderPath;
        }
    }
}
