using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PasswordVaultUSB.ViewModels
{
    internal class LoginViewModel : BaseViewModel
    {

        private string _username;
        private string _password;
        private bool _isPasswordVisible;
        private string _usernameError;
        private string _passwordError;
        private bool _hasUsernameError;
        private bool _hasPasswordError;

        public string Username
        {
            get => _username; 
            set
            {
                if (SetProperty(ref _username, value))
                {
                    if (HasUsernameError)
                    {
                        HasUsernameError = false;
                        UsernameError = string.Empty;
                    }
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    if (HasPasswordError)
                    {
                        HasPasswordError = false;
                        PasswordError = string.Empty;
                    }
                }
            }
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        public string UsernameError
        {
            get => _usernameError;
            set => SetProperty(ref _usernameError, value);
        }

        public string PasswordError
        {
            get => _passwordError;
            set => SetProperty(ref _passwordError, value);
        }

        public bool HasUsernameError
        {
            get => _hasUsernameError; 
            set => SetProperty(ref _hasUsernameError, value);
        }

        public bool HasPasswordError
        {
            get => _hasPasswordError;
            set => SetProperty(ref _hasPasswordError, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            TogglePasswordVisibilityCommand = new RelayCommand(_ =>  TogglePasswordVisibility());
            NavigateToRegisterCommand = new RelayCommand(ExecuteNavigateToRegister);
        }

        private void ExecuteLogin(object parameter)
        {
            ResetErrors();

            bool hasError = false;

            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameError = "Please enter your username!";
                HasUsernameError = true;
                hasError = true;
            }

            if (string.IsNullOrEmpty(Password))
            {
                PasswordError = "Please enter your password!";
                HasPasswordError = true;
                hasError = true;
            }
            else if (Password != "1234")
            {
                PasswordError = "Incorrect password. Please try again.";
                HasPasswordError = true;
                hasError = true;
            }

            if (hasError)
            {
                return;
            }

            var mainView = new MainView();
            Application.Current.MainWindow = mainView;
            mainView.Show();
            CloseWindow(parameter);
        }

        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private void ExecuteNavigateToRegister(object parameter)
        {
            var registerView = new RegisterView();
            registerView.Show();
            CloseWindow(parameter);
        }

        private void ResetErrors()
        {
            HasUsernameError = false;
            HasPasswordError = false;
            UsernameError = string.Empty;
            PasswordError = string.Empty;
        }

        private void CloseWindow(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }
    }
}
