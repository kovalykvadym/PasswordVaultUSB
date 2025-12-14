using PasswordVaultUSB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;

namespace PasswordVaultUSB.Services
{
    public static class UsbDriveService
    {
        public static List<UsbDriveInfo> GetAvailableDrives()
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
                .Select(d => new UsbDriveInfo
                {
                    Name = $"{d.VolumeLabel} ({d.Name})",
                    RootDirectory = d.RootDirectory.FullName
                })
                .ToList();

            return drives;
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

        public static string GetDriveSerialNumber(string drivePath)
        {
            try
            {
                string driveLetter = Path.GetPathRoot(drivePath).TrimEnd('\\');

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    $"SELECT VolumeSerialNumber FROM Win32_LogicalDisk WHERE Name = '{driveLetter}'");

                foreach (var disk in searcher.Get())
                {
                    var serial = disk["VolumeSerialNumber"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(serial))
                    {
                        return serial.Trim();
                    }
                }
            }
            catch (Exception)
            {
            }

            return "UNKNOWN_ID";
        }
    }
}