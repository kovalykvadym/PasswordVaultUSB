using PasswordVaultUSB.Models;
using System;
using System.Windows.Input;

namespace PasswordVaultUSB.ViewModels
{
    public partial class AddPasswordViewModel : BaseViewModel
    {
        #region Fields
        // Data Fields
        private string _service;
        private string _login;
        private string _password;
        private string _url;
        private string _notes;

        // Error Visibility Flags
        private bool _isServiceErrorVisible;
        private bool _isLoginErrorVisible;
        private bool _isPasswordErrorVisible;
        #endregion

        #region Properties
        // UI Text
        public string WindowTitle { get; private set; }
        public string ButtonText { get; private set; }

        // Callback to close window (bool = true if saved)
        public Action<bool> CloseAction { get; set; }

        // Data Properties
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
        #endregion

        #region Validation Properties
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
        #endregion

        #region Commands
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand GeneratePasswordCommand { get; private set; }
        #endregion

        public AddPasswordViewModel(PasswordRecord recordToEdit = null)
        {
            InitializeCommands();
            InitializeData(recordToEdit);
        }

        private void InitializeCommands()
        {
            SaveCommand = new Helpers.RelayCommand(ExecuteSave);
            CancelCommand = new Helpers.RelayCommand(ExecuteCancel);
            GeneratePasswordCommand = new Helpers.RelayCommand(ExecuteGeneratePassword);
        }

        private void InitializeData(PasswordRecord recordToEdit)
        {
            if (recordToEdit != null)
            {
                WindowTitle = "Edit Information";
                ButtonText = "Update";

                Service = recordToEdit.Service;
                Login = recordToEdit.Login;
                Password = recordToEdit.Password;
                Url = recordToEdit.Url;
                Notes = recordToEdit.Notes;
            }
            else
            {
                WindowTitle = "Add New Entry";
                ButtonText = "Save";
            }
        }
    }
}