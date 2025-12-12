using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using PasswordVaultUSB.Views;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PasswordVaultUSB.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _usernameError;
        private string _passwordError;
        private bool _isUsernameErrorVisible;
        private bool _isPasswordErrorVisible;

        // --- Властивості ---

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    IsUsernameErrorVisible = false;
                }
            }
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

        // --- Команди ---
        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public Action CloseAction { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            NavigateToRegisterCommand = new RelayCommand(ExecuteNavigateToRegister);
        }

        private void ExecuteLogin(object parameter)
        {
            // Скидаємо помилки
            IsUsernameErrorVisible = false;
            IsPasswordErrorVisible = false;

            var passwordBox = parameter as PasswordBox;
            string password = passwordBox?.Password;

            bool hasError = false;

            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameError = "Please enter your username!";
                IsUsernameErrorVisible = true;
                hasError = true;
            }

            if (string.IsNullOrEmpty(password))
            {
                PasswordError = "Please enter your password!";
                IsPasswordErrorVisible = true;
                hasError = true;
            }

            if (hasError) return;

            try
            {
                string usbPath = UsbDriveService.GetUsbPath();

                if (string.IsNullOrEmpty(usbPath))
                {
                    MessageBox.Show("USB drive not found! Please insert your flash drive.", "USB Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string vaultPath = UsbDriveService.CreateVaultFolder(usbPath);
                // Переконайтеся, що тут немає квадратних дужок [ ]
                string userFilePath = Path.Combine(vaultPath, $"{Username.Trim()}.dat");

                if (!File.Exists(userFilePath))
                {
                    UsernameError = "User not found on this USB drive!";
                    IsUsernameErrorVisible = true;
                    return;
                }

                string encryptedContent = File.ReadAllText(userFilePath);

                try
                {
                    string json = CryptoService.Decrypt(encryptedContent, password);

                    AppState.CurrentUserFilePath = userFilePath;
                    AppState.CurrentMasterPassword = password;

                    var mainView = new MainView();
                    Application.Current.MainWindow = mainView;
                    mainView.Show();

                    CloseAction?.Invoke();
                }
                catch
                {
                    PasswordError = "Invalid password!";
                    IsPasswordErrorVisible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteNavigateToRegister(object obj)
        {
            var registerView = new RegisterView();
            registerView.Show();
            CloseAction?.Invoke();
        }
    }
}