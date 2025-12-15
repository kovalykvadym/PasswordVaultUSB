using PasswordVaultUSB.Services;
using System;
using System.Windows;
using PasswordVaultUSB.Helpers;

namespace PasswordVaultUSB.ViewModels
{
    public partial class MainViewModel
    {
        #region Settings Fields & Properties
        private string _currentPasswordInput;
        private string _newPasswordInput;
        private string _confirmPasswordInput;

        private int _autoLockTimeout;
        private int _usbCheckInterval;
        private bool _autoClearClipboard;
        private bool _showPasswordOnCopy;
        private bool _confirmDeletions;

        private int _originalAutoLockTimeout;
        private int _originalUsbCheckInterval;

        public string CurrentPasswordInput { get => _currentPasswordInput; set => SetProperty(ref _currentPasswordInput, value); }
        public string NewPasswordInput { get => _newPasswordInput; set => SetProperty(ref _newPasswordInput, value); }
        public string ConfirmPasswordInput { get => _confirmPasswordInput; set => SetProperty(ref _confirmPasswordInput, value); }

        public int AutoLockTimeout { get => _autoLockTimeout; set => SetProperty(ref _autoLockTimeout, value); }
        public int UsbCheckInterval { get => _usbCheckInterval; set => SetProperty(ref _usbCheckInterval, value); }
        public bool AutoClearClipboard { get => _autoClearClipboard; set => SetProperty(ref _autoClearClipboard, value); }
        public bool ShowPasswordOnCopy { get => _showPasswordOnCopy; set => SetProperty(ref _showPasswordOnCopy, value); }
        public bool ConfirmDeletions { get => _confirmDeletions; set => SetProperty(ref _confirmDeletions, value); }
        #endregion

        public void LoadSettingsToProperties()
        {
            AutoLockTimeout = AppSettings.AutoLockTimeout;
            UsbCheckInterval = AppSettings.UsbCheckInterval;
            AutoClearClipboard = AppSettings.AutoClearClipboard;
            ShowPasswordOnCopy = AppSettings.ShowPasswordOnCopy;
            ConfirmDeletions = AppSettings.ConfirmDeletions;

            _originalAutoLockTimeout = AutoLockTimeout;
            _originalUsbCheckInterval = UsbCheckInterval;
        }

        private void ExecuteSaveSettings(object obj)
        {
            AppSettings.AutoLockTimeout = AutoLockTimeout;
            AppSettings.UsbCheckInterval = UsbCheckInterval;
            AppSettings.AutoClearClipboard = AutoClearClipboard;
            AppSettings.ShowPasswordOnCopy = ShowPasswordOnCopy;
            AppSettings.ConfirmDeletions = ConfirmDeletions;

            if (AutoLockTimeout != _originalAutoLockTimeout || UsbCheckInterval != _originalUsbCheckInterval)
            {
                _securityService.UpdateSettings();
                LogAction("Security monitors restarted due to settings change");
                _originalAutoLockTimeout = AutoLockTimeout;
                _originalUsbCheckInterval = UsbCheckInterval;
            }

            SaveData();
            LogAction("Settings saved to encrypted vault");
            MessageBox.Show("Settings updated and saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ExecuteChangeMasterPassword(object obj)
        {
            if (string.IsNullOrWhiteSpace(CurrentPasswordInput) ||
                string.IsNullOrWhiteSpace(NewPasswordInput) ||
                string.IsNullOrWhiteSpace(ConfirmPasswordInput))
            {
                MessageBox.Show("Please fill all fields.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Використовуємо Helper для отримання поточного пароля
            var currentMasterString = SecureStringHelper.ToUnsecureString(AppState.CurrentMasterPassword);

            if (CurrentPasswordInput != currentMasterString)
            {
                MessageBox.Show("Current password incorrect.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (NewPasswordInput != ConfirmPasswordInput)
            {
                MessageBox.Show("New passwords do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _storageService.SaveDataAsync(AppState.CurrentUserFilePath, NewPasswordInput, Passwords, AppState.CurrentHardwareID);

                // Використовуємо Helper для створення SecureString
                AppState.CurrentMasterPassword = SecureStringHelper.ToSecureString(NewPasswordInput);

                LogAction("Master password updated.");
                MessageBox.Show("Master password updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CurrentPasswordInput = NewPasswordInput = ConfirmPasswordInput = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to change password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}