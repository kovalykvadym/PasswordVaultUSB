using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using PasswordVaultUSB.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PasswordVaultUSB.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private string _username;
        private string _usernameError;
        private string _passwordError;
        private string _confPasswordError;
        private bool _isUsernameErrorVisible;
        private bool _isPasswordErrorVisible;
        private bool _isConfPasswordErrorVisible;

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

        // --- Команди ---
        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public Action CloseAction { get; set; }

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(ExecuteRegister);
            NavigateToLoginCommand = new RelayCommand(ExecuteNavigateToLogin);
        }

        private void ExecuteRegister(object parameter)
        {
            // Скидаємо помилки
            IsUsernameErrorVisible = false;
            IsPasswordErrorVisible = false;
            IsConfPasswordErrorVisible = false;

            // Отримуємо доступ до PasswordBox через параметр (передаємо вікно)
            string password = "";
            string confirm = "";

            if (parameter is Window window)
            {
                var passBox = window.FindName("RegPasswordInput") as PasswordBox;
                var confBox = window.FindName("ConfPasswordInput") as PasswordBox;

                // Також враховуємо, якщо користувач ввів пароль у видиме текстове поле (якщо ви перемикали видимість)
                // Для спрощення беремо з PasswordBox, оскільки Code-Behind синхронізує їх
                password = passBox?.Password;
                confirm = confBox?.Password;
            }

            bool hasError = false;

            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameError = "Username is required";
                IsUsernameErrorVisible = true;
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                PasswordError = "Password is required";
                IsPasswordErrorVisible = true;
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(confirm))
            {
                ConfPasswordError = "Please repeat your password";
                IsConfPasswordErrorVisible = true;
                hasError = true;
            }
            else if (password != confirm)
            {
                ConfPasswordError = "Passwords do not match";
                IsConfPasswordErrorVisible = true;
                hasError = true;
            }

            if (hasError) return;

            try
            {
                string usbPath = UsbDriveService.GetUsbPath();

                if (string.IsNullOrEmpty(usbPath))
                {
                    MessageBox.Show("USB drive not found! Please insert your flash drive to create a key.", "USB Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                // Створення нового користувача
                List<PasswordRecord> initialData = new List<PasswordRecord>();
                string json = JsonConvert.SerializeObject(initialData);
                string encryptedContent = CryptoService.Encrypt(json, password);

                File.WriteAllText(userFilePath, encryptedContent);

                // Вхід в систему
                AppState.CurrentUserFilePath = userFilePath;
                AppState.CurrentMasterPassword = password;

                MessageBox.Show($"User {Username} registered successfully on drive {usbPath}!", "Success");

                var mainView = new MainView();
                Application.Current.MainWindow = mainView;
                mainView.Show();

                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteNavigateToLogin(object obj)
        {
            var loginView = new LoginView();
            loginView.Show();
            CloseAction?.Invoke();
        }
    }
}