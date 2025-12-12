using Newtonsoft.Json;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PasswordVaultUSB.Views
{
    public partial class MainView : Window
    {
        public ObservableCollection<PasswordRecord> Passwords { get; set; }
        public ObservableCollection<string> ActivityLog { get; set; }
        private DispatcherTimer _usbCheckTimer;
        private DispatcherTimer _autoLockTimer;
        private DateTime _lastActivityTime;
        private List<Button> _menuButtons;
        private readonly Brush _activeBackground = (Brush)new BrushConverter().ConvertFromString("#3E3E42");
        private readonly Brush _inactiveBackground = Brushes.Transparent;
        private readonly Brush _activeForeground = Brushes.White;
        private readonly Brush _inactiveForeground = (Brush)new BrushConverter().ConvertFromString("#A0A0A0");

        public MainView()
        {
            InitializeComponent();

            Passwords = new ObservableCollection<PasswordRecord>();
            ActivityLog = new ObservableCollection<string>();

            // Встановлюємо DataContext для прив'язки ActivityLog
            this.DataContext = this;

            PasswordsGrid.ItemsSource = Passwords;
            ICollectionView view = CollectionViewSource.GetDefaultView(Passwords);
            view.Filter = FilterPasswords;
            SetupMenuButtons();
            LoadData();
            StartUsbMonitoring();
            StartAutoLockMonitoring();

            LogAction("Main window opened - Vault unlocked successfully");
        }

        private void SetupMenuButtons()
        {
            _menuButtons = new List<Button>
            {
                MyPasswordsButton,
                FavoritesButton,
                UsbButton,
                SettingsButton
            };

            SetActiveMenuButton(MyPasswordsButton);
            LogAction("Menu buttons initialized");
        }

        private void StartUsbMonitoring()
        {
            _usbCheckTimer = new DispatcherTimer();
            _usbCheckTimer.Interval = TimeSpan.FromSeconds(AppSettings.UsbCheckInterval);
            _usbCheckTimer.Tick += UsbCheckTimer_Tick;
            _usbCheckTimer.Start();
            LogAction($"USB monitoring started (check interval: {AppSettings.UsbCheckInterval} seconds)");
        }

        private void StartAutoLockMonitoring()
        {
            if (AppSettings.AutoLockTimeout > 0)
            {
                _lastActivityTime = DateTime.Now;
                _autoLockTimer = new DispatcherTimer();
                _autoLockTimer.Interval = TimeSpan.FromSeconds(10); // Перевіряємо кожні 10 секунд
                _autoLockTimer.Tick += AutoLockTimer_Tick;
                _autoLockTimer.Start();
                LogAction($"Auto-lock monitoring started (timeout: {AppSettings.AutoLockTimeout} minutes)");
            }
            else
            {
                LogAction("Auto-lock disabled");
            }
        }

        private void AutoLockTimer_Tick(object sender, EventArgs e)
        {
            if (AppSettings.AutoLockTimeout > 0)
            {
                var inactiveTime = DateTime.Now - _lastActivityTime;
                var timeoutMinutes = TimeSpan.FromMinutes(AppSettings.AutoLockTimeout);

                if (inactiveTime >= timeoutMinutes)
                {
                    _autoLockTimer?.Stop();
                    LogAction($"Auto-lock triggered after {AppSettings.AutoLockTimeout} minutes of inactivity");
                    MessageBox.Show($"Vault locked due to {AppSettings.AutoLockTimeout} minutes of inactivity.",
                        "Auto-Lock", MessageBoxButton.OK, MessageBoxImage.Information);
                    LockVault_Click(this, null);
                }
            }
        }

        private void ApplySettings()
        {
            // Оновлюємо інтервал перевірки USB
            if (_usbCheckTimer != null)
            {
                _usbCheckTimer.Stop();
                _usbCheckTimer.Interval = TimeSpan.FromSeconds(AppSettings.UsbCheckInterval);
                _usbCheckTimer.Start();
                LogAction($"USB check interval updated to {AppSettings.UsbCheckInterval} seconds");
            }

            // Оновлюємо auto-lock
            if (_autoLockTimer != null)
            {
                _autoLockTimer.Stop();
            }

            if (AppSettings.AutoLockTimeout > 0)
            {
                _lastActivityTime = DateTime.Now;
                if (_autoLockTimer == null)
                {
                    _autoLockTimer = new DispatcherTimer();
                    _autoLockTimer.Interval = TimeSpan.FromSeconds(10);
                    _autoLockTimer.Tick += AutoLockTimer_Tick;
                }
                _autoLockTimer.Start();
                LogAction($"Auto-lock updated (timeout: {AppSettings.AutoLockTimeout} minutes)");
            }
            else
            {
                LogAction("Auto-lock disabled");
            }
        }

        private void UsbCheckTimer_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AppState.CurrentUserFilePath) || !File.Exists(AppState.CurrentUserFilePath))
            {
                _usbCheckTimer.Stop();
                LogAction("USB Key removed - Auto-locking vault");
                MessageBox.Show("USB Key removed! Vault locking...", "Security Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                LockVault_Click(this, null);
            }
        }

        private void LoadData()
        {
            try
            {
                Passwords.Clear();

                LogAction("Loading encrypted data from USB drive");

                if (File.Exists(AppState.CurrentUserFilePath))
                {
                    string encrypted = File.ReadAllText(AppState.CurrentUserFilePath);
                    string json = CryptoService.Decrypt(encrypted, AppState.CurrentMasterPassword);
                    var records = JsonConvert.DeserializeObject<List<PasswordRecord>>(json);

                    if (records != null)
                    {
                        foreach (var record in records)
                        {
                            Passwords.Add(record);
                        }
                        LogAction($"Successfully loaded {records.Count} password record(s)");
                    }
                    else
                    {
                        LogAction("No password records found - Starting with empty vault");
                    }
                }
                else
                {
                    LogAction("No existing vault file found - Creating new vault");
                }
            }
            catch (Exception ex)
            {
                LogAction($"ERROR: Failed to load data - {ex.Message}");
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        private void SaveData()
        {
            if (string.IsNullOrEmpty(AppState.CurrentUserFilePath) || string.IsNullOrEmpty(AppState.CurrentMasterPassword))
            {
                LogAction("WARNING: Cannot save - MasterPassword or file path is null");
                MessageBox.Show("Warning: MasterPassword is null. Data not saved.");
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(Passwords);
                string encrypted = CryptoService.Encrypt(json, AppState.CurrentMasterPassword);
                File.WriteAllText(AppState.CurrentUserFilePath, encrypted);
                LogAction($"Data saved successfully - {Passwords.Count} record(s) encrypted and written to USB");
            }
            catch (Exception ex)
            {
                LogAction($"ERROR: Failed to save data - {ex.Message}");
                MessageBox.Show($"Error saving data: {ex.Message}");
            }
        }

        private bool _showFavoritesOnly;
        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                // Якщо натиснули Settings
                if (clickedButton == SettingsButton)
                {
                    LogAction("Opening Settings window");
                    var settingsWindow = new SettingsView();
                    settingsWindow.DataUpdated = LoadData;
                    if (settingsWindow.ShowDialog() == true)
                    {
                        // Застосувати зміни
                        ApplySettings();
                        LogAction("Settings updated and applied");
                    }
                    return;
                }

                SetActiveMenuButton(clickedButton);

                _showFavoritesOnly = clickedButton == FavoritesButton;
                bool showChangePassword = clickedButton == UsbButton;

                CollectionViewSource.GetDefaultView(Passwords)?.Refresh();
                ToggleChangePasswordPanel(showChangePassword);

                if (!showChangePassword)
                {
                    LogAction($"Switched to menu: {clickedButton.Content}");
                }
            }
        }

        private void SetActiveMenuButton(Button activeButton)
        {
            foreach (var btn in _menuButtons.Where(b => b != null))
            {
                btn.Background = _inactiveBackground;
                btn.Foreground = _inactiveForeground;
            }

            if (activeButton != null)
            {
                activeButton.Background = _activeBackground;
                activeButton.Foreground = _activeForeground;
            }
        }

        private void ToggleChangePasswordPanel(bool show)
        {
            ChangePasswordPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            PasswordsGrid.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            if (HeaderGrid != null)
            {
                HeaderGrid.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            }
            if (!show)
            {
                ClearChangePasswordInputs();
            }
            else
            {
                LogAction("Opened master password change panel");
            }
        }

        private void ApplyChangePassword_Click(object sender, RoutedEventArgs e)
        {
            LogAction("Attempting to change master password");

            var current = CurrentPasswordInput.Password;
            var next = NewPasswordInput.Password;
            var confirm = ConfirmPasswordInput.Password;

            if (string.IsNullOrEmpty(AppState.CurrentMasterPassword) || string.IsNullOrEmpty(AppState.CurrentUserFilePath))
            {
                LogAction("ERROR: Vault not loaded - Cannot change password");
                MessageBox.Show("Vault is not loaded. Please log in first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(AppState.CurrentUserFilePath))
            {
                LogAction("ERROR: Vault file missing from USB drive");
                MessageBox.Show("Vault file is missing on the USB drive.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(next) || string.IsNullOrWhiteSpace(confirm))
            {
                LogAction("VALIDATION FAILED: Empty password fields");
                MessageBox.Show("Please fill all password fields.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (current != AppState.CurrentMasterPassword)
            {
                LogAction("VALIDATION FAILED: Incorrect current password");
                MessageBox.Show("Current password is incorrect.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (next != confirm)
            {
                LogAction("VALIDATION FAILED: Password confirmation mismatch");
                MessageBox.Show("Passwords do not match.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(Passwords);
                string encrypted = CryptoService.Encrypt(json, next);
                File.WriteAllText(AppState.CurrentUserFilePath, encrypted);
                AppState.CurrentMasterPassword = next;

                LogAction("SUCCESS: Master password updated and all data re-encrypted on USB");
                MessageBox.Show("Master password updated and data re-encrypted on USB.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                ToggleChangePasswordPanel(false);
                SetActiveMenuButton(MyPasswordsButton);
                _showFavoritesOnly = false;
                CollectionViewSource.GetDefaultView(Passwords)?.Refresh();
            }
            catch (Exception ex)
            {
                LogAction($"ERROR: Failed to update master password - {ex.Message}");
                MessageBox.Show($"Failed to update password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearChangePasswordInputs()
        {
            CurrentPasswordInput.Password = string.Empty;
            NewPasswordInput.Password = string.Empty;
            ConfirmPasswordInput.Password = string.Empty;
            LogAction("Password change form cleared");
        }

        private void LogAction(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            ActivityLog.Insert(0, $"[{timestamp}] {message}");

            // Optional: cap log size
            if (ActivityLog.Count > 200)
            {
                ActivityLog.RemoveAt(ActivityLog.Count - 1);
            }
        }

        private bool FilterPasswords(object item)
        {
            if (item is PasswordRecord entry)
            {
                if (_showFavoritesOnly && !entry.IsFavorite) return false;
                var search = SearchBox.Text?.ToLower();
                if (string.IsNullOrWhiteSpace(search)) return true;
                return (entry.Service?.ToLower().Contains(search) ?? false)
                    || (entry.Login?.ToLower().Contains(search) ?? false)
                    || (entry.Url?.ToLower().Contains(search) ?? false);
            }
            return false;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text;
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                LogAction($"Searching for: '{searchText}'");
            }
            CollectionViewSource.GetDefaultView(Passwords).Refresh();
        }

        private void AddPassword_Click(object sender, RoutedEventArgs e)
        {
            LogAction("Opening 'Add Password' dialog");
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
                LogAction($"NEW ENTRY: Added password for '{newEntry.Service}' (Login: {newEntry.Login})");
            }
            else
            {
                LogAction("'Add Password' dialog cancelled by user");
            }
        }

        private void CopyPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var entry = button.DataContext as PasswordRecord;

            if (entry != null)
            {
                Clipboard.SetText(entry.Password);
                LogAction($"CLIPBOARD: Copied password for '{entry.Service}' to clipboard");

                // Показуємо пароль на короткий час якщо налаштовано
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

                MessageBox.Show($"Password for {entry.Service} copied to clipboard", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);

                // Автоматично очищаємо clipboard якщо налаштовано
                if (AppSettings.AutoClearClipboard)
                {
                    var clearTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
                    clearTimer.Tick += (s, args) =>
                    {
                        try
                        {
                            if (Clipboard.GetText() == entry.Password)
                            {
                                Clipboard.Clear();
                                LogAction($"Clipboard automatically cleared after 30 seconds");
                            }
                        }
                        catch { }
                        clearTimer.Stop();
                    };
                    clearTimer.Start();
                }
            }
        }

        private void EditPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var entry = button.DataContext as PasswordRecord;

            if (entry != null)
            {
                LogAction($"Opening edit dialog for '{entry.Service}'");
                var editWindow = new AddPasswordView(entry);

                if (editWindow.ShowDialog() == true)
                {
                    var oldService = entry.Service;
                    entry.Service = editWindow.Service;
                    entry.Login = editWindow.Login;
                    entry.Password = editWindow.Password;
                    entry.Url = editWindow.Url;
                    entry.Notes = editWindow.Notes;

                    PasswordsGrid.Items.Refresh();
                    SaveData();
                    LogAction($"UPDATED: Edited password entry for '{oldService}' → '{entry.Service}'");
                }
                else
                {
                    LogAction($"Edit cancelled for '{entry.Service}'");
                }
            }
        }

        private void DeletePassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var entry = button.DataContext as PasswordRecord;

            if (entry != null)
            {
                LogAction($"Delete confirmation requested for '{entry.Service}'");
                var result = MessageBox.Show($"Are you sure you want to delete password for '{entry.Service}'?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Passwords.Remove(entry);
                    SaveData();
                    LogAction($"DELETED: Removed password entry for '{entry.Service}' (Login: {entry.Login})");
                }
                else
                {
                    LogAction($"Delete cancelled for '{entry.Service}'");
                }
            }
        }

        private void ToggleShowPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var entry = button.DataContext as PasswordRecord;

            if (entry != null)
            {
                entry.IsPasswordVisible = !entry.IsPasswordVisible;
                string visibility = entry.IsPasswordVisible ? "VISIBLE" : "HIDDEN";
                LogAction($"Password visibility toggled to {visibility} for '{entry.Service}'");
            }
        }

        private void LockVault_Click(object sender, RoutedEventArgs e)
        {
            LogAction("Locking vault - Clearing session data");

            if (_usbCheckTimer != null && _usbCheckTimer.IsEnabled)
            {
                _usbCheckTimer.Stop();
                LogAction("USB monitoring stopped");
            }

            AppState.CurrentMasterPassword = null;
            AppState.CurrentUserFilePath = null;

            LogAction("Session cleared - Returning to login screen");

            var loginView = new LoginView();
            loginView.Show();
            this.Close();
        }

        private void ToggleFavorite_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PasswordRecord entry)
            {
                entry.IsFavorite = !entry.IsFavorite;
                PasswordsGrid.Items.Refresh();
                SaveData();

                string action = entry.IsFavorite ? "ADDED TO" : "REMOVED FROM";
                LogAction($"FAVORITE: {action} favorites - '{entry.Service}'");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            LogAction("Main window closing");
            if (_usbCheckTimer != null && _usbCheckTimer.IsEnabled)
            {
                _usbCheckTimer.Stop();
            }
            base.OnClosed(e);
        }

        // Додайте цей метод у клас MainView
        private void ResetAutoLockTimer(object sender, InputEventArgs e)
        {
            // Оновлюємо час останньої активності на "зараз"
            _lastActivityTime = DateTime.Now;
        }
    }
}