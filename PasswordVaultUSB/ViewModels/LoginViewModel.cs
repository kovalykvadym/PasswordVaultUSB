using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using PasswordVaultUSB.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PasswordVaultUSB.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        // --- Private Fields ---
        private string _username;
        private string _password;
        private string _usernameError;
        private string _passwordError;
        private bool _isUsernameErrorVisible;
        private bool _isPasswordErrorVisible;
        private UsbDriveInfo _selectedUsbDrive;
        private int _failedAttempts = 0;

        // --- Properties ---
        public string Username
        {
            get => _username;
            set { if (SetProperty(ref _username, value)) IsUsernameErrorVisible = false; }
        }

        public string Password
        {
            get => _password;
            set { if (SetProperty(ref _password, value)) IsPasswordErrorVisible = false; }
        }

        public string UsernameError { get => _usernameError; set => SetProperty(ref _usernameError, value); }
        public bool IsUsernameErrorVisible { get => _isUsernameErrorVisible; set => SetProperty(ref _isUsernameErrorVisible, value); }
        public string PasswordError { get => _passwordError; set => SetProperty(ref _passwordError, value); }
        public bool IsPasswordErrorVisible { get => _isPasswordErrorVisible; set => SetProperty(ref _isPasswordErrorVisible, value); }

        public Action CloseAction { get; set; }
        public ObservableCollection<UsbDriveInfo> UsbDrives { get; set; } = new ObservableCollection<UsbDriveInfo>();

        public UsbDriveInfo SelectedUsbDrive
        {
            get => _selectedUsbDrive;
            set => SetProperty(ref _selectedUsbDrive, value);
        }

        // --- Commands ---
        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }
        public ICommand RefreshDrivesCommand { get; }

        // --- Constructor ---
        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            NavigateToRegisterCommand = new RelayCommand(ExecuteNavigateToRegister);
            RefreshDrivesCommand = new RelayCommand(obj => RefreshDrives());
            RefreshDrives();
        }

        private void RefreshDrives()
        {
            UsbDrives.Clear();
            var drives = UsbDriveService.GetAvailableDrives();
            foreach (var drive in drives) UsbDrives.Add(drive);
            if (UsbDrives.Any()) SelectedUsbDrive = UsbDrives.First();
            else SelectedUsbDrive = null;
        }

        // --- Methods ---
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
                string usbPath = SelectedUsbDrive.RootDirectory;
                string currentHardwareId = UsbDriveService.GetDriveSerialNumber(usbPath);
                string vaultPath = UsbDriveService.CreateVaultFolder(usbPath);
                string userFilePath = Path.Combine(vaultPath, $"{Username.Trim()}.dat");

                if (!File.Exists(userFilePath))
                {
                    UsernameError = "User not found on this USB drive!";
                    IsUsernameErrorVisible = true;
                    return;
                }

                try
                {
                    var storage = new StorageService();
                    await storage.LoadDataAsync(userFilePath, Password, currentHardwareId);

                    _failedAttempts = 0;
                    AppState.CurrentUserFilePath = userFilePath;
                    AppState.CurrentMasterPassword = Password;
                    AppState.CurrentHardwareID = currentHardwareId;

                    OpenMainView();
                }
                catch (UnauthorizedAccessException ex)
                {
                    ShowError($"Security Alert: {ex.Message}");
                }
                catch
                {
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

        private void HandleFailedAttempt(string usbRootPath)
        {
            _failedAttempts++;

            if (_failedAttempts >= 3)
            {
                string vaultPath = UsbDriveService.CreateVaultFolder(usbRootPath);

                WebcamService.CaptureIntruder(vaultPath);
            }
        }

        private void ExecuteNavigateToRegister(object obj)
        {
            var registerView = new RegisterView();
            registerView.Show();
            CloseAction?.Invoke();
        }

        private void ResetErrors()
        {
            IsUsernameErrorVisible = false;
            IsPasswordErrorVisible = false;
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

        private void OpenMainView()
        {
            var mainView = new MainView();
            Application.Current.MainWindow = mainView;
            mainView.Show();
            CloseAction?.Invoke();
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}