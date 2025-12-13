using Newtonsoft.Json;
using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Data;

namespace PasswordVaultUSB.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // Колекції
        private readonly SecurityService _securityService;

        public Action RequestLockView { get; set; }

        // Команда для ручного блокування
        public ICommand LockVaultCommand { get; }

        public ObservableCollection<PasswordRecord> Passwords { get; set; }
        public ObservableCollection<string> ActivityLog { get; set; }

        // Команди
        public ICommand DeleteCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }
        public ICommand ChangeMasterPasswordCommand { get; }
        public ICommand CopyPasswordCommand { get; }

        // Сервіси
        private readonly StorageService _storageService;
        private readonly ClipboardService _clipboardService;

        // --- Властивості для зміни пароля ---
        private string _currentPasswordInput;
        private string _newPasswordInput;
        private string _confirmPasswordInput;

        private ICollectionView _passwordsView;

        private string _searchText;
        private bool _isFavoritesOnly;

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
                {
                    _passwordsView.Refresh();
                }
            }
        }

        public bool IsFavoritesOnly
        {
            get => _isFavoritesOnly;
            set
            {
                if (SetProperty(ref _isFavoritesOnly, value))
                {
                    _passwordsView.Refresh();
                }
            }
        }

        public MainViewModel()
        {
            Passwords = new ObservableCollection<PasswordRecord>();
            ActivityLog = new ObservableCollection<string>();

            _storageService = new StorageService();
            _clipboardService = new ClipboardService();
            _securityService = new SecurityService();
            _passwordsView = CollectionViewSource.GetDefaultView(Passwords);
            _passwordsView.Filter = FilterRecords;

            _securityService.OnLogAction += LogAction;
            _securityService.OnLockRequested += (reason) =>
            {
                MessageBox.Show(reason, "Security Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                ExecuteLockVault(null);
            };

            // Ініціалізація команд
            DeleteCommand = new RelayCommand(DeletePassword);
            ToggleFavoriteCommand = new RelayCommand(ToggleFavorite);
            ChangeMasterPasswordCommand = new RelayCommand(ExecuteChangeMasterPassword);
            CopyPasswordCommand = new RelayCommand(ExecuteCopyPassword);
            LockVaultCommand = new RelayCommand(ExecuteLockVault);

            LogAction("Application started (ViewModel initialized)");
            LoadData();
            _securityService.StartMonitoring();
        }

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
                else LogAction("No data found or vault is empty.");

                _passwordsView = CollectionViewSource.GetDefaultView(Passwords);
                _passwordsView.Filter = FilterRecords;
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
        // ... [Кінець LoadData/SaveData] ...


        // --- МЕТОДИ КОМАНД ---

        private void ExecuteCopyPassword(object parameter)
        {
            if (parameter is PasswordRecord entry)
            {
                // 1. Копіюємо через сервіс
                _clipboardService.CopyToClipboard(entry.Password, AppSettings.AutoClearClipboard);
                LogAction($"CLIPBOARD: Copied password for '{entry.Service}'");

                // 2. Логіка короткочасного показу пароля (візуальний ефект)
                if (AppSettings.ShowPasswordOnCopy && !entry.IsPasswordVisible)
                {
                    entry.IsPasswordVisible = true;

                    // Таймер для приховування
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                    timer.Tick += (s, args) =>
                    {
                        entry.IsPasswordVisible = false;
                        timer.Stop();
                    };
                    timer.Start();
                }

                MessageBox.Show($"Password for {entry.Service} copied to clipboard", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeletePassword(object parameter)
        {
            if (parameter is PasswordRecord entry)
            {
                // Перевірка налаштування "Confirm Deletions"
                if (AppSettings.ConfirmDeletions)
                {
                    var result = MessageBox.Show($"Are you sure you want to delete '{entry.Service}'?",
                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes) return;
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

        // ... [LogAction та ExecuteChangeMasterPassword залишаються без змін] ...

        public void LogAction(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            Application.Current.Dispatcher.Invoke(() =>
            {
                ActivityLog.Insert(0, $"[{timestamp}] {message}");
                if (ActivityLog.Count > 200) ActivityLog.RemoveAt(ActivityLog.Count - 1);
            });
        }

        private void ExecuteChangeMasterPassword(object obj)
        {
            LogAction("Attempting to change master password");

            if (string.IsNullOrWhiteSpace(CurrentPasswordInput) ||
                string.IsNullOrWhiteSpace(NewPasswordInput) ||
                string.IsNullOrWhiteSpace(ConfirmPasswordInput))
            {
                MessageBox.Show("Please fill all password fields.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CurrentPasswordInput != AppState.CurrentMasterPassword)
            {
                MessageBox.Show("Current password is incorrect.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                LogAction("SUCCESS: Master password updated and all data re-encrypted");
                MessageBox.Show("Master password updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CurrentPasswordInput = string.Empty;
                NewPasswordInput = string.Empty;
                ConfirmPasswordInput = string.Empty;
            }
            catch (Exception ex)
            {
                LogAction($"ERROR: Failed to update master password - {ex.Message}");
                MessageBox.Show($"Failed to update password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для додавання нового запису (викликається з View)
        public void AddNewRecord(PasswordRecord newRecord)
        {
            Passwords.Add(newRecord);
            SaveData();
            LogAction($"NEW ENTRY: Added password for '{newRecord.Service}'");
        }

        // Метод для збереження змін після редагування (викликається з View)
        public void UpdateRecord(PasswordRecord oldRecord, PasswordRecord newRecord)
        {
            // Знаходимо індекс старого запису
            int index = Passwords.IndexOf(oldRecord);
            if (index != -1)
            {
                // Оновлюємо дані
                Passwords[index] = newRecord;
                SaveData();
                LogAction($"UPDATED: Edited password entry for '{newRecord.Service}'");
            }
        }

        private bool FilterRecords(object item)
        {
            if (item is PasswordRecord entry)
            {
                // 1. Перевірка на "Обране"
                if (IsFavoritesOnly && !entry.IsFavorite) return false;

                // 2. Перевірка на пошук
                if (string.IsNullOrWhiteSpace(SearchText)) return true;

                var search = SearchText.ToLower();
                return (entry.Service?.ToLower().Contains(search) ?? false)
                    || (entry.Login?.ToLower().Contains(search) ?? false)
                    || (entry.Url?.ToLower().Contains(search) ?? false);
            }
            return false;
        }

        // Метод ручного блокування
        private void ExecuteLockVault(object obj)
        {
            _securityService.StopMonitoring();
            AppState.CurrentMasterPassword = null;
            AppState.CurrentUserFilePath = null;
            LogAction("Vault locked manually.");

            // Викликаємо подію, щоб View закрилося
            RequestLockView?.Invoke();
        }

        // Метод, який викликає View при русі миші/клавіатурі
        public void NotifyUserActivity()
        {
            _securityService.ResetAutoLockTimer();
        }

        // Оновлення налаштувань (викликається після закриття SettingsView)
        public void RefreshSettings()
        {
            LoadData(); // Перезавантажуємо дані (якщо треба)
            _securityService.UpdateSettings(); // Перезапускаємо таймери
        }
    }
}