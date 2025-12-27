using PasswordVaultUSB.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
namespace PasswordVaultUSB.ViewModels {
    public partial class LoginViewModel : BaseViewModel {
        #region Fields
        private string _username;
        private string _password;
        private string _usernameError;
        private string _passwordError;
        private bool _isUsernameErrorVisible;
        private bool _isPasswordErrorVisible;
        private UsbDriveInfo _selectedUsbDrive;
        private int _failedAttempts = 0;
        #endregion
        #region Properties
        public string Username {
            get => _username;
            set { if (SetProperty(ref _username, value)) IsUsernameErrorVisible = false; }
        }
        public string Password {
            get => _password;
            set { if (SetProperty(ref _password, value)) IsPasswordErrorVisible = false; }
        }
        public UsbDriveInfo SelectedUsbDrive {
            get => _selectedUsbDrive;
            set => SetProperty(ref _selectedUsbDrive, value);
        }
        public ObservableCollection<UsbDriveInfo> UsbDrives { get; set; } = new ObservableCollection<UsbDriveInfo>();
        public Action CloseAction { get; set; }
        #endregion
        #region Validation Properties
        public string UsernameError { get => _usernameError; set => SetProperty(ref _usernameError, value); }
        public bool IsUsernameErrorVisible { get => _isUsernameErrorVisible; set => SetProperty(ref _isUsernameErrorVisible, value); }
        public string PasswordError { get => _passwordError; set => SetProperty(ref _passwordError, value); }
        public bool IsPasswordErrorVisible { get => _isPasswordErrorVisible; set => SetProperty(ref _isPasswordErrorVisible, value); }
        #endregion
        #region Commands
        public ICommand LoginCommand { get; private set; }
        public ICommand NavigateToRegisterCommand { get; private set; }
        public ICommand RefreshDrivesCommand { get; private set; }
        #endregion
        public LoginViewModel() {
            LoginCommand = new Helpers.RelayCommand(ExecuteLogin);
            NavigateToRegisterCommand = new Helpers.RelayCommand(ExecuteNavigateToRegister);
            RefreshDrivesCommand = new Helpers.RelayCommand(obj => RefreshDrives());
            RefreshDrives();
        }
    }
}