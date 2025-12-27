using PasswordVaultUSB.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
namespace PasswordVaultUSB.ViewModels {
    public partial class RegisterViewModel : BaseViewModel {
        public RegisterViewModel() {
            RegisterCommand = new Helpers.RelayCommand(ExecuteRegister);
            NavigateToLoginCommand = new Helpers.RelayCommand(ExecuteNavigateToLogin);
            RefreshDrivesCommand = new Helpers.RelayCommand(obj => RefreshDrives());
            RefreshDrives();
        }
        #region Fields & Properties
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
        public string Username {
            get => _username;
            set { if (SetProperty(ref _username, value)) IsUsernameErrorVisible = false; }
        }
        public string Password {
            get => _password;
            set { if (SetProperty(ref _password, value)) IsPasswordErrorVisible = false; }
        }
        public string ConfirmPassword {
            get => _confirmPassword;
            set { if (SetProperty(ref _confirmPassword, value)) IsConfPasswordErrorVisible = false; }
        }
        public UsbDriveInfo SelectedUsbDrive {
            get => _selectedUsbDrive;
            set => SetProperty(ref _selectedUsbDrive, value);
        }
        public Action CloseAction { get; set; }
        #endregion
        #region Validation Properties
        public string UsernameError { get => _usernameError; set => SetProperty(ref _usernameError, value); }
        public bool IsUsernameErrorVisible { get => _isUsernameErrorVisible; set => SetProperty(ref _isUsernameErrorVisible, value); }
        public string PasswordError { get => _passwordError; set => SetProperty(ref _passwordError, value); }
        public bool IsPasswordErrorVisible { get => _isPasswordErrorVisible; set => SetProperty(ref _isPasswordErrorVisible, value); }
        public string ConfPasswordError { get => _confPasswordError; set => SetProperty(ref _confPasswordError, value); }
        public bool IsConfPasswordErrorVisible { get => _isConfPasswordErrorVisible; set => SetProperty(ref _isConfPasswordErrorVisible, value); }
        #endregion
        #region Commands
        public ICommand RegisterCommand { get; private set; }
        public ICommand NavigateToLoginCommand { get; private set; }
        public ICommand RefreshDrivesCommand { get; private set; }
        #endregion
        #region UI Logic (Validation & Navigation)
        private bool ValidateInput() {
            bool isValid = true;
            if (string.IsNullOrWhiteSpace(Username)) {
                UsernameError = "Username is required";
                IsUsernameErrorVisible = true;
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(Password)) {
                PasswordError = "Password is required";
                IsPasswordErrorVisible = true;
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(ConfirmPassword)) {
                ConfPasswordError = "Please repeat your password";
                IsConfPasswordErrorVisible = true;
                isValid = false;
            } else if (Password != ConfirmPassword) {
                ConfPasswordError = "Passwords do not match";
                IsConfPasswordErrorVisible = true;
                isValid = false;
            }
            return isValid;
        }
        private void ResetErrors() {
            IsUsernameErrorVisible = false;
            IsPasswordErrorVisible = false;
            IsConfPasswordErrorVisible = false;
        }

        private void ExecuteNavigateToLogin(object obj) {
            var loginView = new Views.LoginView();
            loginView.Show();
            CloseAction?.Invoke();
        }
        private void OpenMainView() {
            var mainView = new Views.MainView();
            System.Windows.Application.Current.MainWindow = mainView;
            mainView.Show();
            CloseAction?.Invoke();
        }
        #endregion
    }
}