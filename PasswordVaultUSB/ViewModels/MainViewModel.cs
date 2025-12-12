using Newtonsoft.Json;
using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using PasswordVaultUSB.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace PasswordVaultUSB.ViewModels
{
    public class MainViewModel : BaseViewModel
    {


        private string _searchText;
        private bool _isChangePasswordVisible;
        private bool _isPasswordsVisible = true;
        private bool _showFavoritesOnly;
        private DateTime _lastActivityTime;
        private DispatcherTimer _usbCheckTimer;
        private DispatcherTimer _autoLockTimer;

        // Колекції
        public ObservableCollection<PasswordRecord> Passwords { get; set; }
        public ObservableCollection<string> ActivityLog { get; set; }

        // Для фільтрації (пошуку)
        public ICollectionView PasswordsView { get; private set; }

        // --- Властивості ---

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    PasswordsView.Refresh(); // Оновлюємо фільтр при зміні тексту
                }
            }
        }

        public bool IsChangePasswordVisible
        {
            get => _isChangePasswordVisible;
            set
            {
                if (SetProperty(ref _isChangePasswordVisible, value))
                {
                    IsPasswordsVisible = !value;
                }
            }
        }

        public bool IsPasswordsVisible
        {
            get => _isPasswordsVisible;
            set => SetProperty(ref _isPasswordsVisible, value);
        }

        // --- Команди ---
        public ICommand AddPasswordCommand { get; }
        public ICommand CopyPasswordCommand { get; }
        public ICommand EditPasswordCommand { get; }
        public ICommand DeletePasswordCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }
        public ICommand ToggleShowPasswordCommand { get; }
        public ICommand ChangeMenuCommand { get; } // Для перемикання вкладок
        public ICommand OpenSettingsCommand { get; }
        public ICommand LockVaultCommand { get; }
        public ICommand UpdateMasterPasswordCommand { get; } // Логіка зміни пароля

        // --- Поля для зміни пароля (можна винести в окрему VM, але поки залишимо тут) ---
        public string CurrentPasswordInput { get; set; }
        public string NewPasswordInput { get; set; }
        public string ConfirmPasswordInput { get; set; }

        public MainViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

            Passwords = new ObservableCollection<PasswordRecord>();
            ActivityLog = new ObservableCollection<string>();

            // Налаштування фільтрації
            PasswordsView = CollectionViewSource.GetDefaultView(Passwords);
            PasswordsView.Filter = FilterPasswords;

            // Ініціалізація команд
            AddPasswordCommand = new RelayCommand(ExecuteAddPassword);
            CopyPasswordCommand = new RelayCommand(ExecuteCopyPassword);
            EditPasswordCommand = new RelayCommand(ExecuteEditPassword);
            DeletePasswordCommand = new RelayCommand(ExecuteDeletePassword);
            ToggleFavoriteCommand = new RelayCommand(ExecuteToggleFavorite);
            ToggleShowPasswordCommand = new RelayCommand(ExecuteToggleShowPassword);
            ChangeMenuCommand = new RelayCommand(ExecuteChangeMenu);
            OpenSettingsCommand = new RelayCommand(ExecuteOpenSettings);
            LockVaultCommand = new RelayCommand(ExecuteLockVault);
            UpdateMasterPasswordCommand = new RelayCommand(ExecuteUpdateMasterPassword);

            LoadData();
            StartUsbMonitoring();
            StartAutoLockMonitoring();

            LogAction("Vault unlocked successfully");
        }

        // --- Логіка таймерів ---

        public void UpdateLastActivity()
        {
            _lastActivityTime = DateTime.Now;
        }

        private void StartUsbMonitoring()
        {
            _usbCheckTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(AppSettings.UsbCheckInterval) };
            _usbCheckTimer.Tick += (s, e) =>
            {
                if (string.IsNullOrEmpty(AppState.CurrentUserFilePath) || !File.Exists(AppState.CurrentUserFilePath))
                {
                    _usbCheckTimer.Stop();
                    MessageBox.Show("USB Key removed! Vault locking...", "Security Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ExecuteLockVault(null);
                }
            };
            _usbCheckTimer.Start();
        }

        public void StartAutoLockMonitoring()
        {
            if (_autoLockTimer != null) _autoLockTimer.Stop();

            if (AppSettings.AutoLockTimeout > 0)
            {
                _lastActivityTime = DateTime.Now;
                _autoLockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
                _autoLockTimer.Tick += (s, e) =>
                {
                    if (AppSettings.AutoLockTimeout > 0)
                    {
                        var inactiveTime = DateTime.Now - _lastActivityTime;
                        if (inactiveTime >= TimeSpan.FromMinutes(AppSettings.AutoLockTimeout))
                        {
                            _autoLockTimer.Stop();
                            MessageBox.Show($"Vault locked due to {AppSettings.AutoLockTimeout} minutes of inactivity.", "Auto-Lock", MessageBoxButton.OK, MessageBoxImage.Information);
                            ExecuteLockVault(null);
                        }
                    }
                };
                _autoLockTimer.Start();
            }
        }

        // --- Логіка даних ---

        public void LoadData()
        {
            try
            {
                Passwords.Clear();
                if (File.Exists(AppState.CurrentUserFilePath))
                {
                    string encrypted = File.ReadAllText(AppState.CurrentUserFilePath);
                    string json = CryptoService.Decrypt(encrypted, AppState.CurrentMasterPassword);
                    var records = JsonConvert.DeserializeObject<List<PasswordRecord>>(json);

                    if (records != null)
                        foreach (var record in records) Passwords.Add(record);
                }
            }
            catch (Exception ex)
            {
                LogAction($"Error loading data: {ex.Message}");
            }
        }

        private void SaveData()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Passwords);
                string encrypted = CryptoService.Encrypt(json, AppState.CurrentMasterPassword);
                File.WriteAllText(AppState.CurrentUserFilePath, encrypted);
            }
            catch (Exception ex)
            {
                LogAction($"Error saving data: {ex.Message}");
                MessageBox.Show($"Error saving data: {ex.Message}");
            }
        }

        // --- Методи команд ---

        private void ExecuteAddPassword(object obj)
        {
            var addWindow = new AddPasswordView();
            if (addWindow.ShowDialog() == true)
            {
                var newEntry = new PasswordRecord
                {
                    Service = addWindow.Service,
                    Login = addWindow.Login,
                    Password = addWindow.Password,
                    Url = addWindow.Url,
                    Notes = addWindow.Notes
                };
                Passwords.Add(newEntry);
                SaveData();
                LogAction($"Added password for '{newEntry.Service}'");
            }
        }

        private void ExecuteEditPassword(object obj)
        {
            if (obj is PasswordRecord entry)
            {
                var editWindow = new AddPasswordView(entry);
                if (editWindow.ShowDialog() == true)
                {
                    entry.Service = editWindow.Service;
                    entry.Login = editWindow.Login;
                    entry.Password = editWindow.Password;
                    entry.Url = editWindow.Url;
                    entry.Notes = editWindow.Notes;

                    // Оновлення UI
                    var index = Passwords.IndexOf(entry);
                    if (index != -1) Passwords[index] = entry;

                    SaveData();
                    LogAction($"Updated password for '{entry.Service}'");
                }
            }
        }

        private void ExecuteDeletePassword(object obj)
        {
            if (obj is PasswordRecord entry)
            {
                if (AppSettings.ConfirmDeletions)
                {
                    var result = MessageBox.Show($"Delete password for '{entry.Service}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No) return;
                }

                Passwords.Remove(entry);
                SaveData();
                LogAction($"Deleted password for '{entry.Service}'");
            }
        }

        private void ExecuteCopyPassword(object obj)
        {
            if (obj is PasswordRecord entry)
            {
                Clipboard.SetText(entry.Password);
                LogAction($"Copied password for '{entry.Service}'");
                MessageBox.Show("Password copied!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                if (AppSettings.AutoClearClipboard)
                {
                    // Простий таймер для очищення буфера
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
                    timer.Tick += (s, e) => {
                        try { if (Clipboard.GetText() == entry.Password) Clipboard.Clear(); } catch { }
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
        }

        private void ExecuteToggleFavorite(object obj)
        {
            if (obj is PasswordRecord entry)
            {
                entry.IsFavorite = !entry.IsFavorite;
                SaveData();
                // Оновлюємо вигляд (через заміну об'єкта або просто refresh, якщо DataGrid розумний)
                PasswordsView.Refresh();
            }
        }

        private void ExecuteToggleShowPassword(object obj)
        {
            if (obj is PasswordRecord entry)
            {
                entry.IsPasswordVisible = !entry.IsPasswordVisible;
                // Примушуємо UI оновити рядок
                PasswordsView.Refresh();
            }
        }

        private void ExecuteChangeMenu(object parameter)
        {
            string menu = parameter?.ToString();

            // За замовчуванням показуємо паролі
            IsChangePasswordVisible = false;
            _showFavoritesOnly = false;

            if (menu == "Favorites")
            {
                _showFavoritesOnly = true;
            }
            else if (menu == "Usb")
            {
                IsChangePasswordVisible = true; // Показуємо панель зміни майстер-пароля
            }

            PasswordsView.Refresh();
        }

        private void ExecuteOpenSettings(object obj)
        {
            var settingsWindow = new SettingsView();
            settingsWindow.DataUpdated = LoadData; // Прив'язка оновлення

            if (settingsWindow.ShowDialog() == true || settingsWindow.SettingsChanged)
            {
                // Оновити таймери
                StartAutoLockMonitoring();
                if (_usbCheckTimer != null)
                    _usbCheckTimer.Interval = TimeSpan.FromSeconds(AppSettings.UsbCheckInterval);
            }
        }

        private void ExecuteLockVault(object obj)
        {
            if (_usbCheckTimer != null) _usbCheckTimer.Stop();
            if (_autoLockTimer != null) _autoLockTimer.Stop();

            AppState.CurrentMasterPassword = null;
            AppState.CurrentUserFilePath = null;

            var loginView = new LoginView();
            loginView.Show();

            // Закриття вікна (це трохи "брудно" для VM, але дієво)
            Application.Current.Windows.OfType<MainView>().FirstOrDefault()?.Close();
        }

        private void ExecuteUpdateMasterPassword(object parameter)
        {
            // Отримуємо доступ до полів пароля через параметр (StackPanel)
            if (parameter is FrameworkElement panel)
            {
                var currentBox = panel.FindName("CurrentPasswordInput") as PasswordBox;
                var newBox = panel.FindName("NewPasswordInput") as PasswordBox;
                var confirmBox = panel.FindName("ConfirmPasswordInput") as PasswordBox;

                string current = currentBox?.Password;
                string next = newBox?.Password;
                string confirm = confirmBox?.Password;

                // Валідація
                if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(next) || string.IsNullOrWhiteSpace(confirm))
                {
                    MessageBox.Show("Please fill all password fields.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (current != AppState.CurrentMasterPassword)
                {
                    MessageBox.Show("Current password is incorrect.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (next != confirm)
                {
                    MessageBox.Show("Passwords do not match.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Зміна пароля
                try
                {
                    string json = JsonConvert.SerializeObject(Passwords);
                    // Шифруємо новим паролем
                    string encrypted = CryptoService.Encrypt(json, next);
                    File.WriteAllText(AppState.CurrentUserFilePath, encrypted);

                    // Оновлюємо поточний стан
                    AppState.CurrentMasterPassword = next;

                    LogAction("Master password updated successfully");
                    MessageBox.Show("Master password updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Очищаємо поля
                    currentBox.Password = "";
                    newBox.Password = "";
                    confirmBox.Password = "";

                    // Повертаємося до списку паролів
                    ExecuteChangeMenu("MyPasswords");
                }
                catch (Exception ex)
                {
                    LogAction($"Failed to update master password: {ex.Message}");
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // --- Допоміжні методи ---

        private bool FilterPasswords(object item)
        {
            if (item is PasswordRecord entry)
            {
                if (_showFavoritesOnly && !entry.IsFavorite) return false;

                if (string.IsNullOrWhiteSpace(SearchText)) return true;

                string search = SearchText.ToLower();
                return (entry.Service?.ToLower().Contains(search) ?? false)
                    || (entry.Login?.ToLower().Contains(search) ?? false);
            }
            return false;
        }

        private void LogAction(string message)
        {
            ActivityLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
            if (ActivityLog.Count > 200) ActivityLog.RemoveAt(ActivityLog.Count - 1);
        }
    }
}