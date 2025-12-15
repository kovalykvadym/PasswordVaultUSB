using PasswordVaultUSB.Models;
using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Services;
using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Input;

namespace PasswordVaultUSB.ViewModels
{
    public partial class LoginViewModel
    {
        #region Main Logic
        private void RefreshDrives()
        {
            UsbDrives.Clear();
            var drives = UsbDriveService.GetAvailableDrives();

            foreach (var drive in drives)
                UsbDrives.Add(drive);

            if (UsbDrives.Any())
                SelectedUsbDrive = UsbDrives.First();
            else
                SelectedUsbDrive = null;
        }

        private async void ExecuteLogin(object parameter)
        {
            ResetErrors();

            if (SelectedUsbDrive == null)
            {
                ShowError("Please select a USB drive!");
                RefreshDrives();
                return;
            }

            if (!ValidateInput()) return;

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                // 1. Отримуємо параметри USB
                string usbPath = SelectedUsbDrive.RootDirectory;
                string currentHardwareId = UsbDriveService.GetDriveSerialNumber(usbPath);
                string vaultPath = UsbDriveService.CreateVaultFolder(usbPath);

                string inputFileName = $"{Username.Trim()}.dat";
                string userFilePath = Path.Combine(vaultPath, inputFileName);

                // 2. Перевірка існування файлу
                if (!File.Exists(userFilePath))
                {
                    UsernameError = "User not found on this USB drive!";
                    IsUsernameErrorVisible = true;
                    return;
                }

                // 3. Строга перевірка регістру (Case Sensitivity)
                // Щоб 'User' і 'user' були різними користувачами
                string actualFileName = Directory.GetFiles(vaultPath, inputFileName)
                                 .Select(Path.GetFileName)
                                 .FirstOrDefault();

                if (actualFileName == null || !string.Equals(actualFileName, inputFileName, StringComparison.Ordinal))
                {
                    UsernameError = "User not found on this USB drive!";
                    IsUsernameErrorVisible = true;
                    return;
                }

                // 4. Спроба дешифрування
                try
                {
                    var storage = new StorageService();
                    await storage.LoadDataAsync(userFilePath, Password, currentHardwareId);

                    _failedAttempts = 0;
                    AppState.CurrentUserFilePath = userFilePath;
                    AppState.CurrentMasterPassword = SecureStringHelper.ToSecureString(Password);
                    AppState.CurrentHardwareID = currentHardwareId;

                    OpenMainView();
                }
                catch (UnauthorizedAccessException ex)
                {
                    ShowError($"Security Alert: {ex.Message}");
                }
                catch
                {
                    // Пароль невірний
                    HandleFailedAttempt(usbPath);
                    PasswordError = "Invalid password!";
                    IsPasswordErrorVisible = true;
                }
            }
            catch (Exception ex)
            {
                PasswordError = $"Error: {ex.Message}";
                IsPasswordErrorVisible = true;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        #endregion

        #region Security & Helpers
        private void HandleFailedAttempt(string usbRootPath)
        {
            _failedAttempts++;

            // Якщо 3 помилки поспіль - робимо фото
            if (_failedAttempts >= 3)
            {
                string vaultPath = UsbDriveService.CreateVaultFolder(usbRootPath);
                WebcamService.CaptureIntruder(vaultPath);
            }
        }

        private SecureString ConvertToSecureString(string password)
        {
            if (password == null) return null;
            var secure = new SecureString();
            foreach (char c in password) secure.AppendChar(c);
            secure.MakeReadOnly();
            return secure;
        }
        #endregion

        #region Navigation & Validation
        private void ExecuteNavigateToRegister(object obj)
        {
            var registerView = new Views.RegisterView();
            registerView.Show();
            CloseAction?.Invoke();
        }

        private void OpenMainView()
        {
            var mainView = new Views.MainView();
            Application.Current.MainWindow = mainView;
            mainView.Show();
            CloseAction?.Invoke();
        }

        private bool ValidateInput()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameError = "Please enter your username!";
                IsUsernameErrorVisible = true;
                isValid = false;
            }

            if (string.IsNullOrEmpty(Password))
            {
                PasswordError = "Please enter your password!";
                IsPasswordErrorVisible = true;
                isValid = false;
            }

            return isValid;
        }

        private void ResetErrors()
        {
            IsUsernameErrorVisible = false;
            IsPasswordErrorVisible = false;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion
    }
}