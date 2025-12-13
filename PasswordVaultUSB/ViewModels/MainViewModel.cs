using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace PasswordVaultUSB.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // --- Private Fields & Services ---
        private readonly StorageService _storageService;
        private readonly ClipboardService _clipboardService;
        private readonly SecurityService _securityService;

        private ICollectionView _passwordsView;

        private string _currentPasswordInput;
        private string _newPasswordInput;
        private string _confirmPasswordInput;
        private string _searchText;
        private bool _isFavoritesOnly;

        // --- Properties ---
        public ObservableCollection<PasswordRecord> Passwords { get; set; }
        public ObservableCollection<string> ActivityLog { get; set; }

        public Action RequestLockView { get; set; }

        public string CurrentPasswordInput
        {
            get => _currentPasswordInput;
            set => SetProperty(ref _currentPasswordInput, value);
        }

        public string NewPasswordInput
        {
            get => _newPasswordInput;
            set => SetProperty(ref _newPasswordInput, value);
        }

        public string ConfirmPasswordInput
        {
            get => _confirmPasswordInput;
            set => SetProperty(ref _confirmPasswordInput, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    _passwordsView.Refresh();
            }
        }

        public bool IsFavoritesOnly
        {
            get => _isFavoritesOnly;
            set
            {
                if (SetProperty(ref _isFavoritesOnly, value))
                    _passwordsView.Refresh();
            }
        }

        // --- Commands ---
        public ICommand DeleteCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }
        public ICommand ChangeMasterPasswordCommand { get; }
        public ICommand CopyPasswordCommand { get; }
        public ICommand LockVaultCommand { get; }

        // --- Constructor ---
        public MainViewModel()
        {
            Passwords = new ObservableCollection<PasswordRecord>();
            ActivityLog = new ObservableCollection<string>();

            _storageService = new StorageService();
            _clipboardService = new ClipboardService();
            _securityService = new SecurityService();

            // Setup CollectionView for filtering
            _passwordsView = CollectionViewSource.GetDefaultView(Passwords);
            _passwordsView.Filter = FilterRecords;

            // Security Events
            _securityService.OnLogAction += LogAction;
            _securityService.OnLockRequested += (reason) =>
            {
                MessageBox.Show(reason, "Security Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                ExecuteLockVault(null);
            };

            // Init Commands
            DeleteCommand = new RelayCommand(DeletePassword);
            ToggleFavoriteCommand = new RelayCommand(ToggleFavorite);
            ChangeMasterPasswordCommand = new RelayCommand(ExecuteChangeMasterPassword);
            CopyPasswordCommand = new RelayCommand(ExecuteCopyPassword);
            LockVaultCommand = new RelayCommand(ExecuteLockVault);

            LogAction("Application started (ViewModel initialized)");
            LoadData();
            _securityService.StartMonitoring();
        }

        // --- Data Methods (Load/Save/Update) ---
        public void LoadData()
        {
            try
            {
                Passwords.Clear();
                LogAction("Loading encrypted data...");

                var records = _storageService.LoadData(AppState.CurrentUserFilePath, AppState.CurrentMasterPassword);

                if (records.Count > 0)
                {
                    foreach (var record in records) Passwords.Add(record);
                    LogAction($"Successfully loaded {records.Count} records");
                }
                else
                {
                    LogAction("No data found or vault is empty.");
                }

                _passwordsView.Refresh();
            }
            catch (Exception ex)
            {
                LogAction($"ERROR loading data: {ex.Message}");
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        public void SaveData()
        {
            if (string.IsNullOrEmpty(AppState.CurrentUserFilePath)) return;
            try
            {
                _storageService.SaveData(AppState.CurrentUserFilePath, AppState.CurrentMasterPassword, Passwords);
                LogAction("Data saved successfully.");
            }
            catch (Exception ex)
            {
                LogAction($"ERROR saving data: {ex.Message}");
                MessageBox.Show($"Error saving data: {ex.Message}");
            }
        }

        public void AddNewRecord(PasswordRecord newRecord)
        {
            Passwords.Add(newRecord);
            SaveData();
            LogAction($"NEW ENTRY: Added password for '{newRecord.Service}'");
        }

        public void UpdateRecord(PasswordRecord oldRecord, PasswordRecord newRecord)
        {
            int index = Passwords.IndexOf(oldRecord);
            if (index != -1)
            {
                Passwords[index] = newRecord;
                SaveData();
                LogAction($"UPDATED: Edited password entry for '{newRecord.Service}'");
            }
        }

        // --- Command Methods ---
        private void ExecuteCopyPassword(object parameter)
        {
            if (parameter is PasswordRecord entry)
            {
                _clipboardService.CopyToClipboard(entry.Password, AppSettings.AutoClearClipboard);
                LogAction($"CLIPBOARD: Copied password for '{entry.Service}'");

                if (AppSettings.ShowPasswordOnCopy && !entry.IsPasswordVisible)
                {
                    entry.IsPasswordVisible = true;
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                    timer.Tick += (s, args) =>
                    {
                        entry.IsPasswordVisible = false;
                        timer.Stop();
                    };
                    timer.Start();
                }

                MessageBox.Show($"Password for {entry.Service} copied.", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeletePassword(object parameter)
        {
            if (parameter is PasswordRecord entry)
            {
                if (AppSettings.ConfirmDeletions)
                {
                    if (MessageBox.Show($"Delete '{entry.Service}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                        return;
                }

                Passwords.Remove(entry);
                SaveData();
                LogAction($"Deleted password for '{entry.Service}'");
            }
        }

        private void ToggleFavorite(object parameter)
        {
            if (parameter is PasswordRecord entry)
            {
                entry.IsFavorite = !entry.IsFavorite;
                SaveData();
                LogAction($"Favorite toggled for '{entry.Service}'");
            }
        }

        private void ExecuteChangeMasterPassword(object obj)
        {
            if (string.IsNullOrWhiteSpace(CurrentPasswordInput) ||
                string.IsNullOrWhiteSpace(NewPasswordInput) ||
                string.IsNullOrWhiteSpace(ConfirmPasswordInput))
            {
                MessageBox.Show("Please fill all fields.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CurrentPasswordInput != AppState.CurrentMasterPassword)
            {
                MessageBox.Show("Current password incorrect.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewPasswordInput != ConfirmPasswordInput)
            {
                MessageBox.Show("New passwords do not match.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _storageService.SaveData(AppState.CurrentUserFilePath, NewPasswordInput, Passwords);
                AppState.CurrentMasterPassword = NewPasswordInput;

                LogAction("SUCCESS: Master password updated.");
                MessageBox.Show("Master password updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                CurrentPasswordInput = NewPasswordInput = ConfirmPasswordInput = string.Empty;
            }
            catch (Exception ex)
            {
                LogAction($"ERROR: Failed to update password - {ex.Message}");
                MessageBox.Show($"Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteLockVault(object obj)
        {
            _securityService.StopMonitoring();
            AppState.CurrentMasterPassword = null;
            AppState.CurrentUserFilePath = null;
            LogAction("Vault locked manually.");
            RequestLockView?.Invoke();
        }

        // --- Helpers & Filtering ---
        public void LogAction(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            Application.Current.Dispatcher.Invoke(() =>
            {
                ActivityLog.Insert(0, $"[{timestamp}] {message}");
                if (ActivityLog.Count > 200) ActivityLog.RemoveAt(ActivityLog.Count - 1);
            });
        }

        private bool FilterRecords(object item)
        {
            if (item is PasswordRecord entry)
            {
                if (IsFavoritesOnly && !entry.IsFavorite) return false;

                if (string.IsNullOrWhiteSpace(SearchText)) return true;

                var search = SearchText.ToLower();
                return (entry.Service?.ToLower().Contains(search) ?? false)
                    || (entry.Login?.ToLower().Contains(search) ?? false)
                    || (entry.Url?.ToLower().Contains(search) ?? false);
            }
            return false;
        }

        public void NotifyUserActivity() => _securityService.ResetAutoLockTimer();

        public void RefreshSettings()
        {
            LoadData();
            _securityService.UpdateSettings();
        }
    }
}