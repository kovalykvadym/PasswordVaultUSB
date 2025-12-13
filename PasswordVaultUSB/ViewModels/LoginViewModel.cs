using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using PasswordVaultUSB.Views;
using System;
using System.IO;
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

        // --- Properties ---
        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value)) IsUsernameErrorVisible = false;
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value)) IsPasswordErrorVisible = false;
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

        public Action CloseAction { get; set; }

        // --- Commands ---
        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        // --- Constructor ---
        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            NavigateToRegisterCommand = new RelayCommand(ExecuteNavigateToRegister);
        }

        // --- Methods ---
        private void ExecuteLogin(object parameter)
        {
            ResetErrors();

            if (!ValidateInput()) return;

            try
            {
                string usbPath = UsbDriveService.GetUsbPath();
                if (string.IsNullOrEmpty(usbPath))
                {
                    ShowError("USB drive not found! Please insert your flash drive.");
                    return;
                }

                string vaultPath = UsbDriveService.CreateVaultFolder(usbPath);
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
                    string json = CryptoService.Decrypt(encryptedContent, Password);

                    AppState.CurrentUserFilePath = userFilePath;
                    AppState.CurrentMasterPassword = Password;

                    OpenMainView();
                }
                catch
                {
                    PasswordError = "Invalid password!";
                    IsPasswordErrorVisible = true;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Critical error: {ex.Message}");
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