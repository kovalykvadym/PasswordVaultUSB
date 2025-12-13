using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Models;
using System;
using System.Windows.Input;

namespace PasswordVaultUSB.ViewModels
{
    public class AddPasswordViewModel : BaseViewModel
    {
        // --- Private Fields ---
        private string _service;
        private string _login;
        private string _password;
        private string _url;
        private string _notes;

        private bool _isServiceErrorVisible;
        private bool _isLoginErrorVisible;
        private bool _isPasswordErrorVisible;

        // --- Properties ---
        public string WindowTitle { get; private set; }
        public string ButtonText { get; private set; }

        public string Service
        {
            get => _service;
            set { if (SetProperty(ref _service, value)) IsServiceErrorVisible = false; }
        }

        public string Login
        {
            get => _login;
            set { if (SetProperty(ref _login, value)) IsLoginErrorVisible = false; }
        }

        public string Password
        {
            get => _password;
            set { if (SetProperty(ref _password, value)) IsPasswordErrorVisible = false; }
        }

        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        // Visibility
        public bool IsServiceErrorVisible
        {
            get => _isServiceErrorVisible;
            set => SetProperty(ref _isServiceErrorVisible, value);
        }

        public bool IsLoginErrorVisible
        {
            get => _isLoginErrorVisible;
            set => SetProperty(ref _isLoginErrorVisible, value);
        }

        public bool IsPasswordErrorVisible
        {
            get => _isPasswordErrorVisible;
            set => SetProperty(ref _isPasswordErrorVisible, value);
        }

        public Action<bool> CloseAction { get; set; }

        // --- Commands ---
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // --- Constructor ---
        public AddPasswordViewModel(PasswordRecord recordToEdit = null)
        {
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);

            if (recordToEdit != null)
            {
                WindowTitle = "Edit information";
                ButtonText = "Update";

                Service = recordToEdit.Service;
                Login = recordToEdit.Login;
                Password = recordToEdit.Password;
                Url = recordToEdit.Url;
                Notes = recordToEdit.Notes;
            }
            else
            {
                WindowTitle = "Add new entry";
                ButtonText = "Save";
            }
        }

        // --- Methods ---
        private void ExecuteSave(object obj)
        {
            if (!ValidateInput()) return;
            CloseAction?.Invoke(true);
        }

        private bool ValidateInput()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(Service))
            {
                IsServiceErrorVisible = true;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Login))
            {
                IsLoginErrorVisible = true;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                IsPasswordErrorVisible = true;
                isValid = false;
            }

            return isValid;
        }

        private void ExecuteCancel(object obj)
        {
            CloseAction?.Invoke(false);
        }
    }
}