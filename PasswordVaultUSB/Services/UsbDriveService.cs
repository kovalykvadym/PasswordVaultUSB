using PasswordVaultUSB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
namespace PasswordVaultUSB.Services {
    public static class UsbDriveService {
        public static List<UsbDriveInfo> GetAvailableDrives() {
            return DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
                .Select(d => new UsbDriveInfo{
                    Name = string.IsNullOrEmpty(d.VolumeLabel) 
                           ? $"{d.Name} (Removable Disk)" 
                           : $"{d.VolumeLabel} ({d.Name})",
                    RootDirectory = d.RootDirectory.FullName
                }).ToList();
        }
        public static string CreateVaultFolder(string usbRootPath) {
            string folderPath = Path.Combine(usbRootPath, ".PasswordVaultData");
            DirectoryInfo di;
            if (!Directory.Exists(folderPath)) {
                di = Directory.CreateDirectory(folderPath);
            } else {
                di = new DirectoryInfo(folderPath);}
            try { di.Attributes = FileAttributes.Directory |
                    FileAttributes.Hidden | FileAttributes.System;
            } catch { }
            return folderPath;
        }
        public static string GetDriveSerialNumber(string drivePath) {
            try {
                string driveLetter = Path.GetPathRoot(drivePath).TrimEnd('\\');
                using (var searcher = new ManagementObjectSearcher(
                    $"SELECT VolumeSerialNumber " +
                    $"FROM Win32_LogicalDisk WHERE Name = '{driveLetter}'")) {
                    foreach (var disk in searcher.Get()) {
                        var serial = disk["VolumeSerialNumber"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(serial)) {
                            return serial.Trim();}}}
            } catch { }
            return "UNKNOWN_ID";
        }
    }
}