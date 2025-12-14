using Microsoft.Win32;
using Newtonsoft.Json;
using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        // --- NEW: SETTINGS FIELDS ---
        private int _autoLockTimeout;
        private int _usbCheckInterval;
        private bool _autoClearClipboard;
        private bool _showPasswordOnCopy;
        private bool _confirmDeletions;

        // Для зберігання попередніх значень (щоб не перезапускати сервіс дарма)
        private int _originalAutoLockTimeout;
        private int _originalUsbCheckInterval;

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

        // --- NEW: SETTINGS PROPERTIES ---

        private int _genLength = 16;
        private bool _genUseUpper = true;
        private bool _genUseLower = true;
        private bool _genUseDigits = true;
        private bool _genUseSymbols = true;
        private string _generatedPassword;

        public int GenLength
        {
            get => _genLength;
            set
            {
                if (value < 4) value = 4;
                if (value > 64) value = 64;
                SetProperty(ref _genLength, value);
            }
        }

        public bool GenUseUpper { get => _genUseUpper; set => SetProperty(ref _genUseUpper, value); }
        public bool GenUseLower { get => _genUseLower; set => SetProperty(ref _genUseLower, value); }
        public bool GenUseDigits { get => _genUseDigits; set => SetProperty(ref _genUseDigits, value); }
        public bool GenUseSymbols { get => _genUseSymbols; set => SetProperty(ref _genUseSymbols, value); }

        public string GeneratedPassword
        {
            get => _generatedPassword;
            set => SetProperty(ref _generatedPassword, value);
        }
        public int AutoLockTimeout
        {
            get => _autoLockTimeout;
            set => SetProperty(ref _autoLockTimeout, value);
        }
        public int UsbCheckInterval
        {
            get => _usbCheckInterval;
            set => SetProperty(ref _usbCheckInterval, value);
        }
        public bool AutoClearClipboard
        {
            get => _autoClearClipboard;
            set => SetProperty(ref _autoClearClipboard, value);
        }
        public bool ShowPasswordOnCopy
        {
            get => _showPasswordOnCopy;
            set => SetProperty(ref _showPasswordOnCopy, value);
        }
        public bool ConfirmDeletions
        {
            get => _confirmDeletions;
            set => SetProperty(ref _confirmDeletions, value);
        }

        // --- Commands ---
        public ICommand DeleteCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }
        public ICommand ChangeMasterPasswordCommand { get; }
        public ICommand CopyPasswordCommand { get; }
        public ICommand LockVaultCommand { get; }

        // --- NEW: SETTINGS COMMANDS ---
        public ICommand SaveSettingsCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ClearDataCommand { get; }
        public ICommand GenerateStandaloneCommand { get; }
        public ICommand CopyGeneratedCommand { get; }

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
                ExecuteLockVault(null);
                MessageBox.Show(reason, "Security Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
            };

            // Init Commands
            DeleteCommand = new RelayCommand(DeletePassword);
            ToggleFavoriteCommand = new RelayCommand(ToggleFavorite);
            ChangeMasterPasswordCommand = new RelayCommand(ExecuteChangeMasterPassword);
            CopyPasswordCommand = new RelayCommand(ExecuteCopyPassword);
            LockVaultCommand = new RelayCommand(ExecuteLockVault);

            // Init Settings Commands
            SaveSettingsCommand = new RelayCommand(ExecuteSaveSettings);
            ExportCommand = new RelayCommand(ExecuteExport);
            ImportCommand = new RelayCommand(ExecuteImport);
            ClearDataCommand = new RelayCommand(ExecuteClearData);
            GenerateStandaloneCommand = new RelayCommand(ExecuteGenerateStandalone);
            CopyGeneratedCommand = new RelayCommand(ExecuteCopyGenerated);

            LogAction("Application started");
            LoadData();

            LoadSettingsToProperties();

            _securityService.StartMonitoring();
        }

        // --- Data Methods ---
        public async void LoadData()
        {
            try
            {
                Passwords.Clear();
                var records = await _storageService.LoadDataAsync(AppState.CurrentUserFilePath, AppState.CurrentMasterPassword, AppState.CurrentHardwareID);

                if (records.Count > 0)
                {
                    foreach (var record in records) Passwords.Add(record);
                }

                _passwordsView.Refresh();
            }
            catch (Exception ex)
            {
                LogAction($"ERROR loading data: {ex.Message}");
            }
        }

        public async void SaveData()
        {
            if (string.IsNullOrEmpty(AppState.CurrentUserFilePath)) return;

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                await _storageService.SaveDataAsync(AppState.CurrentUserFilePath, AppState.CurrentMasterPassword, Passwords, AppState.CurrentHardwareID);
            }
            catch (Exception ex)
            {
                LogAction($"ERROR saving data: {ex.Message}");
                MessageBox.Show($"Error saving data: {ex.Message}");
            }
            finally
            {
                Mouse.OverrideCursor = null;
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

        // --- NEW: SETTINGS METHODS ---

        public void LoadSettingsToProperties()
        {
            AutoLockTimeout = AppSettings.AutoLockTimeout;
            UsbCheckInterval = AppSettings.UsbCheckInterval;
            AutoClearClipboard = AppSettings.AutoClearClipboard;
            ShowPasswordOnCopy = AppSettings.ShowPasswordOnCopy;
            ConfirmDeletions = AppSettings.ConfirmDeletions;

            _originalAutoLockTimeout = AutoLockTimeout;
            _originalUsbCheckInterval = UsbCheckInterval;
        }

        private void ExecuteSaveSettings(object obj)
        {
            AppSettings.AutoLockTimeout = AutoLockTimeout;
            AppSettings.UsbCheckInterval = UsbCheckInterval;
            AppSettings.AutoClearClipboard = AutoClearClipboard;
            AppSettings.ShowPasswordOnCopy = ShowPasswordOnCopy;
            AppSettings.ConfirmDeletions = ConfirmDeletions;

            if (AutoLockTimeout != _originalAutoLockTimeout || UsbCheckInterval != _originalUsbCheckInterval)
            {
                _securityService.UpdateSettings();
                LogAction("Security monitors restarted due to settings change");

                _originalAutoLockTimeout = AutoLockTimeout;
                _originalUsbCheckInterval = UsbCheckInterval;
            }

            SaveData();

            LogAction("Settings saved to encrypted vault");
            MessageBox.Show("Settings updated and saved to your secure file!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ExecuteExport(object obj)
        {
            try
            {
                if (!File.Exists(AppState.CurrentUserFilePath))
                {
                    MessageBox.Show("No vault data found to export.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Export Vault Data",
                    FileName = $"PVault_Export_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var records = await _storageService.LoadDataAsync(AppState.CurrentUserFilePath, AppState.CurrentMasterPassword, AppState.CurrentHardwareID);

                    string json = JsonConvert.SerializeObject(records, Formatting.Indented);
                    File.WriteAllText(saveDialog.FileName, json);

                    MessageBox.Show("Vault data exported successfully!\nWARNING: File is NOT encrypted.",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    LogAction("Data exported to JSON");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}");
            }
        }

        private async void ExecuteImport(object obj)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Import Vault Data"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(openDialog.FileName);
                    var importedRecords = JsonConvert.DeserializeObject<List<PasswordRecord>>(json);

                    if (importedRecords == null || importedRecords.Count == 0)
                    {
                        MessageBox.Show("File is empty or invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var currentRecords = await _storageService.LoadDataAsync(AppState.CurrentUserFilePath, AppState.CurrentMasterPassword, AppState.CurrentHardwareID);

                    int addedCount = 0;
                    int skippedCount = 0;

                    foreach (var importRecord in importedRecords)
                    {
                        bool exists = currentRecords.Any(r =>
                            string.Equals(r.Service, importRecord.Service, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(r.Login, importRecord.Login, StringComparison.OrdinalIgnoreCase));

                        if (!exists)
                        {
                            currentRecords.Add(importRecord);
                            addedCount++;
                        }
                        else
                        {
                            skippedCount++;
                        }
                    }

                    if (addedCount > 0)
                    {
                        await _storageService.SaveDataAsync(AppState.CurrentUserFilePath, AppState.CurrentMasterPassword, currentRecords, AppState.CurrentHardwareID);

                        LoadData();

                        MessageBox.Show($"Imported: {addedCount}\nSkipped: {skippedCount}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LogAction($"Imported {addedCount} records");
                    }
                    else
                    {
                        MessageBox.Show("No new records found.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Import failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ExecuteClearData(object obj)
        {
            if (MessageBox.Show("Permanently delete ALL passwords?", "Confirm Clear", MessageBoxButton.YesNo, MessageBoxImage.Stop) == MessageBoxResult.Yes)
            {
                try
                {
                    Passwords.Clear();

                    await _storageService.SaveDataAsync(AppState.CurrentUserFilePath, AppState.CurrentMasterPassword, new List<PasswordRecord>(), AppState.CurrentHardwareID);

                    MessageBox.Show("All data cleared.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    LogAction("All data cleared by user");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        // --- Existing Commands ---
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
                    timer.Tick += (s, args) => { entry.IsPasswordVisible = false; timer.Stop(); };
                    timer.Start();
                }
            }
        }

        private void DeletePassword(object parameter)
        {
            if (parameter is PasswordRecord entry)
            {
                if (AppSettings.ConfirmDeletions)
                {
                    if (MessageBox.Show($"Delete '{entry.Service}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
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
            }
        }

        private async void ExecuteChangeMasterPassword(object obj)
        {
            if (string.IsNullOrWhiteSpace(CurrentPasswordInput) || string.IsNullOrWhiteSpace(NewPasswordInput) || string.IsNullOrWhiteSpace(ConfirmPasswordInput))
            {
                MessageBox.Show("Please fill all fields."); return;
            }
            if (CurrentPasswordInput != AppState.CurrentMasterPassword)
            {
                MessageBox.Show("Current password incorrect."); return;
            }
            if (NewPasswordInput != ConfirmPasswordInput)
            {
                MessageBox.Show("New passwords do not match."); return;
            }

            try
            {
                // Асинхронне збереження з новим паролем
                await _storageService.SaveDataAsync(AppState.CurrentUserFilePath, NewPasswordInput, Passwords, AppState.CurrentHardwareID);

                AppState.CurrentMasterPassword = NewPasswordInput;
                LogAction("Master password updated.");
                MessageBox.Show("Master password updated!");
                CurrentPasswordInput = NewPasswordInput = ConfirmPasswordInput = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed: {ex.Message}");
            }
        }

        private void ExecuteLockVault(object obj)
        {
            _securityService.StopMonitoring();
            AppState.CurrentMasterPassword = null;
            AppState.CurrentUserFilePath = null;
            LogAction("Vault locked.");
            RequestLockView?.Invoke();
        }

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
                return (entry.Service?.ToLower().Contains(search) ?? false) ||
                       (entry.Login?.ToLower().Contains(search) ?? false) ||
                       (entry.Url?.ToLower().Contains(search) ?? false);
            }
            return false;
        }

        public void NotifyUserActivity() => _securityService.ResetAutoLockTimer();

        private void ExecuteGenerateStandalone(object obj)
        {
            GeneratedPassword = PasswordGeneratorService.GeneratePassword(
                GenLength, GenUseLower, GenUseUpper, GenUseDigits, GenUseSymbols);
            LogAction("Generated new secure password");
        }

        private void ExecuteCopyGenerated(object obj)
        {
            if (!string.IsNullOrEmpty(GeneratedPassword))
            {
                _clipboardService.CopyToClipboard(GeneratedPassword, AppSettings.AutoClearClipboard);
                LogAction("Generated password copied to clipboard");
            }
        }
    }
}