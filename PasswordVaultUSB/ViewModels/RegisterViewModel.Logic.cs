using Newtonsoft.Json;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
namespace PasswordVaultUSB.ViewModels {
    public partial class RegisterViewModel {
        private void RefreshDrives() {
            UsbDrives.Clear();
            var drives = UsbDriveService.GetAvailableDrives();
            foreach (var drive in drives)
                UsbDrives.Add(drive);

            if (UsbDrives.Any())
                SelectedUsbDrive = UsbDrives.First();
            else
                SelectedUsbDrive = null;
        }
        private async void ExecuteRegister(object parameter) {
            ResetErrors();
            if (SelectedUsbDrive == null) {
                MessageBox.Show("Please select a USB drive to store your vault!", "USB Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                RefreshDrives();
                return;
            }
            if (!ValidateInput()) return;

            Mouse.OverrideCursor = Cursors.Wait;
            try {
                string usbPath = SelectedUsbDrive.RootDirectory;
                string currentHardwareId = UsbDriveService.GetDriveSerialNumber(usbPath);
                if (currentHardwareId == "UNKNOWN_ID") {
                    var result = MessageBox.Show("Warning: Could not read USB Serial Number. Security binding will be weak. Continue?",
                        "Security Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No) return;
                }
                string vaultPath = UsbDriveService.CreateVaultFolder(usbPath);
                string userFilePath = Path.Combine(vaultPath, $"{Username.Trim()}.dat");
                if (File.Exists(userFilePath)) {
                    UsernameError = "User already exists on this USB drive!";
                    IsUsernameErrorVisible = true;
                    return;
                }
                await CreateUserFileAsync(userFilePath, currentHardwareId);
                AppState.CurrentUserFilePath = userFilePath;
                AppState.CurrentMasterPassword = SecureStringHelper.ToSecureString(Password);
                AppState.CurrentHardwareID = currentHardwareId;
                MessageBox.Show($"User {Username} registered successfully!", "Success");
                OpenMainView();
            } catch (Exception ex) {
                MessageBox.Show($"Critical error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            } finally { Mouse.OverrideCursor = null; }
        }
        private async Task CreateUserFileAsync(string path, string hardwareId) {
            var initialData = new VaultData {
                HardwareID = hardwareId,
                Records = new List<PasswordRecord>(),
                Settings = new UserSettings()
            };
            byte[] encryptedContent = await Task.Run(() => {
                string json = JsonConvert.SerializeObject(initialData);
                return CryptoService.Encrypt(json, Password);
            });
            using (FileStream sourceStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true)) {
                await sourceStream.WriteAsync(encryptedContent, 0, encryptedContent.Length);
            }
        }
    }
}