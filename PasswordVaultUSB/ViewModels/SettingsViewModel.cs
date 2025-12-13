using Microsoft.Win32;
using Newtonsoft.Json;
using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace PasswordVaultUSB.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly StorageService _storageService;

        // Властивості налаштувань (Binding)
        public int AutoLockTimeout { get; set; }
        public int UsbCheckInterval { get; set; }
        public bool AutoClearClipboard { get; set; }
        public bool ShowPasswordOnCopy { get; set; }
        public bool ConfirmDeletions { get; set; }

        // Подія, щоб повідомити View, що треба закритися
        public Action RequestClose { get; set; }

        // Подія, щоб повідомити MainView, що дані змінилися (після імпорту/очищення)
        public Action DataUpdated { get; set; }

        // Команди
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ClearDataCommand { get; }

        public SettingsViewModel()
        {
            _storageService = new StorageService();

            // Завантажуємо поточні значення з AppSettings
            AutoLockTimeout = AppSettings.AutoLockTimeout;
            UsbCheckInterval = AppSettings.UsbCheckInterval;
            AutoClearClipboard = AppSettings.AutoClearClipboard;
            ShowPasswordOnCopy = AppSettings.ShowPasswordOnCopy;
            ConfirmDeletions = AppSettings.ConfirmDeletions;

            // Ініціалізація команд
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(obj => RequestClose?.Invoke());
            ExportCommand = new RelayCommand(ExecuteExport);
            ImportCommand = new RelayCommand(ExecuteImport);
            ClearDataCommand = new RelayCommand(ExecuteClearData);
        }

        private void ExecuteSave(object obj)
        {
            // Зберігаємо значення назад у AppSettings
            AppSettings.AutoLockTimeout = AutoLockTimeout;
            AppSettings.UsbCheckInterval = UsbCheckInterval;
            AppSettings.AutoClearClipboard = AutoClearClipboard;
            AppSettings.ShowPasswordOnCopy = ShowPasswordOnCopy;
            AppSettings.ConfirmDeletions = ConfirmDeletions;

            AppSettings.SaveSettings();

            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            RequestClose?.Invoke();
        }

        private void ExecuteExport(object obj)
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
                    // Використовуємо StorageService для завантаження
                    var records = _storageService.LoadData(AppState.CurrentUserFilePath, AppState.CurrentMasterPassword);

                    // Серіалізуємо у "чистий" JSON
                    string json = JsonConvert.SerializeObject(records, Formatting.Indented);
                    File.WriteAllText(saveDialog.FileName, json);

                    MessageBox.Show("Vault data exported successfully!\n\nWARNING: The exported file is NOT encrypted. Keep it secure!",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteImport(object obj)
        {
            var result = MessageBox.Show(
                   "WARNING: Importing data will REPLACE all current passwords in your vault!\n\nDo you want to continue?",
                   "Confirm Import", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.No) return;

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
                    var records = JsonConvert.DeserializeObject<List<PasswordRecord>>(json);

                    if (records != null)
                    {
                        // Зберігаємо через сервіс (він сам зашифрує)
                        _storageService.SaveData(AppState.CurrentUserFilePath, AppState.CurrentMasterPassword, records);

                        MessageBox.Show($"Successfully imported {records.Count} password record(s)!",
                            "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                        DataUpdated?.Invoke(); // Оновлюємо головне вікно
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Import failed: {ex.Message}\nCheck file format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteClearData(object obj)
        {
            var result = MessageBox.Show(
                "WARNING: This will permanently delete ALL passwords!\nAre you absolutely sure?",
                "Confirm Clear", MessageBoxButton.YesNo, MessageBoxImage.Stop);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var emptyList = new List<PasswordRecord>();
                    _storageService.SaveData(AppState.CurrentUserFilePath, AppState.CurrentMasterPassword, emptyList);

                    MessageBox.Show("All vault data has been cleared.", "Data Cleared", MessageBoxButton.OK, MessageBoxImage.Information);
                    DataUpdated?.Invoke();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to clear data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}