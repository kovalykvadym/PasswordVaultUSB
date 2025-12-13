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

        // --- Properties ---
        public int AutoLockTimeout { get; set; }
        public int UsbCheckInterval { get; set; }
        public bool AutoClearClipboard { get; set; }
        public bool ShowPasswordOnCopy { get; set; }
        public bool ConfirmDeletions { get; set; }

        public Action RequestClose { get; set; }
        public Action DataUpdated { get; set; }

        // --- Commands ---
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ClearDataCommand { get; }

        // --- Constructor ---
        public SettingsViewModel()
        {
            _storageService = new StorageService();

            // Load settings
            AutoLockTimeout = AppSettings.AutoLockTimeout;
            UsbCheckInterval = AppSettings.UsbCheckInterval;
            AutoClearClipboard = AppSettings.AutoClearClipboard;
            ShowPasswordOnCopy = AppSettings.ShowPasswordOnCopy;
            ConfirmDeletions = AppSettings.ConfirmDeletions;

            // Init commands
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(obj => RequestClose?.Invoke());
            ExportCommand = new RelayCommand(ExecuteExport);
            ImportCommand = new RelayCommand(ExecuteImport);
            ClearDataCommand = new RelayCommand(ExecuteClearData);
        }

        // --- Methods ---
        private void ExecuteSave(object obj)
        {
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
                    var records = _storageService.LoadData(AppState.CurrentUserFilePath, AppState.CurrentMasterPassword);
                    string json = JsonConvert.SerializeObject(records, Formatting.Indented);
                    File.WriteAllText(saveDialog.FileName, json);

                    MessageBox.Show("Vault data exported successfully!\n\nWARNING: The exported file is NOT encrypted.",
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
            if (MessageBox.Show("WARNING: Importing data will REPLACE all current passwords!\nContinue?",
                   "Confirm Import", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

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
                        _storageService.SaveData(AppState.CurrentUserFilePath, AppState.CurrentMasterPassword, records);
                        MessageBox.Show($"Successfully imported {records.Count} records!", "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                        DataUpdated?.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Import failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteClearData(object obj)
        {
            if (MessageBox.Show("Permanently delete ALL passwords?", "Confirm Clear", MessageBoxButton.YesNo, MessageBoxImage.Stop) == MessageBoxResult.Yes)
            {
                try
                {
                    _storageService.SaveData(AppState.CurrentUserFilePath, AppState.CurrentMasterPassword, new List<PasswordRecord>());
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