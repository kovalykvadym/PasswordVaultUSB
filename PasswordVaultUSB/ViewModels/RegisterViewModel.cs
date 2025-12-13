using Newtonsoft.Json;
using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using PasswordVaultUSB.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

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

        public Action CloseAction { get; set; }

        // --- Commands ---
        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        // --- Constructor ---
        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(ExecuteRegister);
            NavigateToLoginCommand = new RelayCommand(ExecuteNavigateToLogin);
        }

        // --- Methods ---
        private void ExecuteRegister(object parameter)
        {
            ResetErrors();

            if (!ValidateInput()) return;

            try
            {
                string usbPath = UsbDriveService.GetUsbPath();
                if (string.IsNullOrEmpty(usbPath))
                {
                    MessageBox.Show("USB drive not found! Please insert your flash drive.", "USB Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string vaultPath = UsbDriveService.CreateVaultFolder(usbPath);
                string userFilePath = Path.Combine(vaultPath, $"{Username.Trim()}.dat");

                if (File.Exists(userFilePath))
                {
                    UsernameError = "User already exists on this USB drive!";
                    IsUsernameErrorVisible = true;
                    return;
                }

                CreateUserFile(userFilePath);

                AppState.CurrentUserFilePath = userFilePath;
                AppState.CurrentMasterPassword = Password;

                MessageBox.Show($"User {Username} registered successfully!", "Success");
                OpenMainView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateUserFile(string path)
        {
            List<PasswordRecord> initialData = new List<PasswordRecord>();
            string json = JsonConvert.SerializeObject(initialData);
            string encryptedContent = CryptoService.Encrypt(json, Password);
            File.WriteAllText(path, encryptedContent);
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