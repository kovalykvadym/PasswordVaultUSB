using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Models;
using System;
using System.Windows.Input;

namespace PasswordVaultUSB.ViewModels
{
    public class AddPasswordViewModel : BaseViewModel
    {
        private string _service;
        private string _login;
        private string _password;
        private string _url;
        private string _notes;

        // Поля для помилок
        private bool _isServiceErrorVisible;
        private bool _isLoginErrorVisible;
        private bool _isPasswordErrorVisible;

        // Заголовки
        public string WindowTitle { get; private set; }
        public string ButtonText { get; private set; }

        // --- Властивості ---

        public string Service
        {
            get => _service;
            set
            {
                if (SetProperty(ref _service, value))
                    IsServiceErrorVisible = false;
            }
        }

        public string Login
        {
            get => _login;
            set
            {
                if (SetProperty(ref _login, value))
                    IsLoginErrorVisible = false;
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                    IsPasswordErrorVisible = false;
            }
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

        // --- Видимість помилок ---

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

        // --- Команди та Події ---

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public Action<bool> CloseAction { get; set; }

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

        private void ExecuteSave(object obj)
        {
            bool hasError = false;

            if (string.IsNullOrWhiteSpace(Service))
            {
                IsServiceErrorVisible = true;
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(Login))
            {
                IsLoginErrorVisible = true;
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                IsPasswordErrorVisible = true;
                hasError = true;
            }

            if (hasError) return;

            // Успіх -> закриваємо з результатом true
            CloseAction?.Invoke(true);
        }

        private void ExecuteCancel(object obj)
        {
            // Скасування -> закриваємо з результатом false
            CloseAction?.Invoke(false);
        }
    }
}