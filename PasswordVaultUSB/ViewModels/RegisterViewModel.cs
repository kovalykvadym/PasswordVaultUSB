using Newtonsoft.Json;
using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using PasswordVaultUSB.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;

namespace PasswordVaultUSB.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        // --- Private Fields ---
        private string _username;
        private string _password;
        private string _confirmPassword;

        private string _usernameError;
        private string _passwordError;
        private string _confPasswordError;

        private bool _isUsernameErrorVisible;
        private bool _isPasswordErrorVisible;
        private bool _isConfPasswordErrorVisible;

        private UsbDriveInfo _selectedUsbDrive;
        public ObservableCollection<UsbDriveInfo> UsbDrives { get; set; } = new ObservableCollection<UsbDriveInfo>();

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

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { if (SetProperty(ref _confirmPassword, value)) IsConfPasswordErrorVisible = false; }
        }

        public string UsernameError
        {
            get => _usernameError;
            set => SetProperty(ref _usernameError, value);
        }

        public bool IsUsernameErrorVisible
        {
            get => _isUsernameErrorVisible;
            set => SetProperty(ref _isUsernameErrorVisible, value);
        }

        public string PasswordError
        {
            get => _passwordError;
            set => SetProperty(ref _passwordError, value);
        }

        public bool IsPasswordErrorVisible
        {
            get => _isPasswordErrorVisible;
            set => SetProperty(ref _isPasswordErrorVisible, value);
        }

        public string ConfPasswordError
        {
            get => _confPasswordError;
            set => SetProperty(ref _confPasswordError, value);
        }

        public bool IsConfPasswordErrorVisible
        {
            get => _isConfPasswordErrorVisible;
            set => SetProperty(ref _isConfPasswordErrorVisible, value);
        }

        public UsbDriveInfo SelectedUsbDrive
        {
            get => _selectedUsbDrive;
            set => SetProperty(ref _selectedUsbDrive, value);
        }

        public Action CloseAction { get; set; }

        // --- Commands ---
        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public ICommand RefreshDrivesCommand { get; }

        // --- Constructor ---
        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(ExecuteRegister);
            NavigateToLoginCommand = new RelayCommand(ExecuteNavigateToLogin);

            RefreshDrivesCommand = new RelayCommand(obj => RefreshDrives());
            RefreshDrives();
        }

        // --- Methods ---
        private void RefreshDrives()
        {
            UsbDrives.Clear();
            var drives = UsbDriveService.GetAvailableDrives();
            foreach (var drive in drives) UsbDrives.Add(drive);

            if (UsbDrives.Any()) SelectedUsbDrive = UsbDrives.First();
            else SelectedUsbDrive = null;
        }

        private async void ExecuteRegister(object parameter)
        {
            ResetErrors();

            if (SelectedUsbDrive == null)
            {
                MessageBox.Show("Please select a USB drive to store your vault!", "USB Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                RefreshDrives();
                return;
            }

            if (!ValidateInput()) return;

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                string usbPath = SelectedUsbDrive.RootDirectory;

                string currentHardwareId = UsbDriveService.GetDriveSerialNumber(usbPath);

                if (currentHardwareId == "UNKNOWN_ID")
                {
                    var result = MessageBox.Show("Warning: Could not read USB Serial Number. Security binding will be weak. Continue?",
                        "Security Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No) return;
                }

                string vaultPath = UsbDriveService.CreateVaultFolder(usbPath);
                string userFilePath = Path.Combine(vaultPath, $"{Username.Trim()}.dat");

                if (File.Exists(userFilePath))
                {
                    UsernameError = "User already exists on this USB drive!";
                    IsUsernameErrorVisible = true;
                    return;
                }

                await CreateUserFileAsync(userFilePath, currentHardwareId);

                AppState.CurrentUserFilePath = userFilePath;
                AppState.CurrentMasterPassword = Password;
                AppState.CurrentHardwareID = currentHardwareId;

                MessageBox.Show($"User {Username} registered successfully!", "Success");
                OpenMainView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        private async Task CreateUserFileAsync(string path, string hardwareId)
        {
            var initialData = new VaultData
            {
                HardwareID = hardwareId,
                Records = new List<PasswordRecord>(),
                Settings = new UserSettings()
            };

            byte[] encryptedContent = await Task.Run(() =>
            {
                string json = JsonConvert.SerializeObject(initialData);
                return CryptoService.Encrypt(json, Password);
            });

            using (FileStream sourceStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                await sourceStream.WriteAsync(encryptedContent, 0, encryptedContent.Length);
            }
        }

        private bool ValidateInput()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameError = "Username is required";
                IsUsernameErrorVisible = true;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = "Password is required";
                IsPasswordErrorVisible = true;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ConfPasswordError = "Please repeat your password";
                IsConfPasswordErrorVisible = true;
                isValid = false;
            }
            else if (Password != ConfirmPassword)
            {
                ConfPasswordError = "Passwords do not match";
                IsConfPasswordErrorVisible = true;
                isValid = false;
            }

            return isValid;
        }

        private void ExecuteNavigateToLogin(object obj)
        {
            var loginView = new LoginView();
            loginView.Show();
            CloseAction?.Invoke();
        }

        private void OpenMainView()
        {
            var mainView = new MainView();
            Application.Current.MainWindow = mainView;
            mainView.Show();
            CloseAction?.Invoke();
        }

        private void ResetErrors()
        {
            IsUsernameErrorVisible = false;
            IsPasswordErrorVisible = false;
            IsConfPasswordErrorVisible = false;
        }
    }
}