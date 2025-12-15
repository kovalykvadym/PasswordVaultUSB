using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using System;
using System.Windows;
using System.Windows.Input;

namespace PasswordVaultUSB.ViewModels
{
    public partial class MainViewModel
    {
        #region CRUD Operations

        public async void LoadData()
        {
            try
            {
                Passwords.Clear();

                var masterPassword = SecureStringHelper.ToUnsecureString(AppState.CurrentMasterPassword);

                // 1. Завантажуємо дані з флешки
                var records = await _storageService.LoadDataAsync(AppState.CurrentUserFilePath, masterPassword, AppState.CurrentHardwareID);

                if (records.Count > 0)
                {
                    foreach (var record in records) Passwords.Add(record);
                }

                _passwordsView.Refresh();

                _ = BackupService.PerformBackupAsync(AppState.CurrentUserFilePath);

                LogAction("Backup created successfully");
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
                // Використовуємо Helper
                var masterPassword = SecureStringHelper.ToUnsecureString(AppState.CurrentMasterPassword);

                await _storageService.SaveDataAsync(AppState.CurrentUserFilePath, masterPassword, Passwords, AppState.CurrentHardwareID);
            }
            catch (Exception ex)
            {
                LogAction($"ERROR saving data: {ex.Message}");
                MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        #endregion
    }
}