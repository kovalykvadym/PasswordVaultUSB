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
            PasswordsGrid.ItemsSource = Passwords;
            ICollectionView view = CollectionViewSource.GetDefaultView(Passwords);
            view.Filter = FilterPasswords;
            SetupMenuButtons();
            LoadData();
            StartUsbMonitoring();
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
        }

        private void StartUsbMonitoring()
        {
            _usbCheckTimer = new DispatcherTimer();
            _usbCheckTimer.Interval = TimeSpan.FromSeconds(3);
            _usbCheckTimer.Tick += UsbCheckTimer_Tick;
            _usbCheckTimer.Start();
        }

        private void UsbCheckTimer_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AppState.CurrentUserFilePath) || !File.Exists(AppState.CurrentUserFilePath))
            {
                _usbCheckTimer.Stop();
                MessageBox.Show("USB Key removed! Vault locking...", "Security Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                LockVault_Click(this, null);
            }
        }

        private void LoadData()
        {
            try
            {
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
                    }
                }
            } catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        private void SaveData()
        {
            if (string.IsNullOrEmpty(AppState.CurrentUserFilePath) || string.IsNullOrEmpty(AppState.CurrentMasterPassword))
            {
                MessageBox.Show("Warning: MasterPassword is null. Data not saved.");
                return;
            }

            try 
            {
                string json = JsonConvert.SerializeObject(Passwords);
                string encrypted = CryptoService.Encrypt(json, AppState.CurrentMasterPassword);
                File.WriteAllText(AppState.CurrentUserFilePath, encrypted);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }

        }

        private bool _showFavoritesOnly;
        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
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
            var current = CurrentPasswordInput.Password;
            var next = NewPasswordInput.Password;
            var confirm = ConfirmPasswordInput.Password;

            if (string.IsNullOrEmpty(AppState.CurrentMasterPassword) || string.IsNullOrEmpty(AppState.CurrentUserFilePath))
            {
                MessageBox.Show("Vault is not loaded. Please log in first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(AppState.CurrentUserFilePath))
            {
                MessageBox.Show("Vault file is missing on the USB drive.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

            try
            {
                string json = JsonConvert.SerializeObject(Passwords);
                string encrypted = CryptoService.Encrypt(json, next);
                File.WriteAllText(AppState.CurrentUserFilePath, encrypted);
                AppState.CurrentMasterPassword = next;
                MessageBox.Show("Master password updated and data re-encrypted on USB.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LogAction("Master password updated and data re-encrypted on USB");
                ToggleChangePasswordPanel(false);
                SetActiveMenuButton(MyPasswordsButton);
                _showFavoritesOnly = false;
                CollectionViewSource.GetDefaultView(Passwords)?.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LogAction($"Failed to update master password: {ex.Message}");
            }
        }

        private void ClearChangePasswordInputs()
        {
            CurrentPasswordInput.Password = string.Empty;
            NewPasswordInput.Password = string.Empty;
            ConfirmPasswordInput.Password = string.Empty;
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
            CollectionViewSource.GetDefaultView(Passwords).Refresh();
        }

        private void AddPassword_Click(object sender, RoutedEventArgs e)
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
                LogAction($"Added password for service '{newEntry.Service}'");
            }
        }

        private void CopyPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var entry = button.DataContext as PasswordRecord;

            if (entry != null)
            {
                Clipboard.SetText(entry.Password);
                MessageBox.Show($"Password for {entry.Service} copied to clipboard", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
                LogAction($"Copied password for '{entry.Service}' to clipboard");
            }
        }

        private void EditPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var entry = button.DataContext as PasswordRecord;

            if (entry != null)
            {
                var editWindow = new AddPasswordView(entry);

                if(editWindow.ShowDialog() == true)
                {
                    entry.Service = editWindow.Service;
                    entry.Login = editWindow.Login;
                    entry.Password = editWindow.Password;
                    entry.Url = editWindow.Url;
                    entry.Notes = editWindow.Notes;
                    PasswordsGrid.Items.Refresh();
                    SaveData();
                    LogAction($"Edited password entry for '{entry.Service}'");
                }
            }
        }

        private void DeletePassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var entry = button.DataContext as PasswordRecord;

            if (entry != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete password for '{entry.Service}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes) 
                {
                    Passwords.Remove(entry);
                    SaveData();
                    LogAction($"Deleted password for '{entry.Service}'");
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
                LogAction($"Toggled visibility for '{entry.Service}'");
            }
        }

        private void LockVault_Click(object sender, RoutedEventArgs e)
        {
            if(_usbCheckTimer != null && _usbCheckTimer.IsEnabled)
            {
                _usbCheckTimer.Stop();
            }

            AppState.CurrentMasterPassword = null;
            AppState.CurrentUserFilePath = null;

            var loginView = new LoginView();
            loginView.Show();
            this.Close();
            LogAction("Vault locked and returned to login");
        }

        private void ToggleFavorite_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PasswordRecord entry)
            {
                entry.IsFavorite = !entry.IsFavorite;
                PasswordsGrid.Items.Refresh();
                SaveData();
                LogAction($"{(entry.IsFavorite ? "Added to" : "Removed from")} favorites: '{entry.Service}'");
            }
        }
    }
}