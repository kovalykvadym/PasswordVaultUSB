using Microsoft.Win32;
using Newtonsoft.Json;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using System;
using System.Collections.Generic;
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
using System.IO;

namespace PasswordVaultUSB.Views
{
    public partial class SettingsView : Window
    {
        public bool SettingsChanged { get; private set; }
        public Action DataUpdated { get; set; }

        public SettingsView()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Завантажуємо збережені налаштування
            AutoLockComboBox.SelectedIndex = GetComboBoxIndexByTag(AutoLockComboBox, AppSettings.AutoLockTimeout.ToString());
            UsbCheckComboBox.SelectedIndex = GetComboBoxIndexByTag(UsbCheckComboBox, AppSettings.UsbCheckInterval.ToString());
            ClearClipboardCheckBox.IsChecked = AppSettings.AutoClearClipboard;
            ShowOnCopyCheckBox.IsChecked = AppSettings.ShowPasswordOnCopy;
            ConfirmDeletionsCheckBox.IsChecked = AppSettings.ConfirmDeletions;
        }

        private int GetComboBoxIndexByTag(ComboBox comboBox, string tag)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i] is ComboBoxItem item && item.Tag.ToString() == tag)
                {
                    return i;
                }
            }
            return 0;
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Зберігаємо налаштування
                var autoLockItem = AutoLockComboBox.SelectedItem as ComboBoxItem;
                AppSettings.AutoLockTimeout = int.Parse(autoLockItem.Tag.ToString());

                var usbCheckItem = UsbCheckComboBox.SelectedItem as ComboBoxItem;
                AppSettings.UsbCheckInterval = int.Parse(usbCheckItem.Tag.ToString());

                AppSettings.AutoClearClipboard = ClearClipboardCheckBox.IsChecked ?? true;
                AppSettings.ShowPasswordOnCopy = ShowOnCopyCheckBox.IsChecked ?? false;
                AppSettings.ConfirmDeletions = ConfirmDeletionsCheckBox.IsChecked ?? true;

                // Зберігаємо в файл
                AppSettings.SaveSettings();

                SettingsChanged = true;
                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ExportData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Export Vault Data",
                    FileName = $"PVault_Export_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    if (File.Exists(AppState.CurrentUserFilePath))
                    {
                        string encrypted = File.ReadAllText(AppState.CurrentUserFilePath);
                        string json = CryptoService.Decrypt(encrypted, AppState.CurrentMasterPassword);

                        // Зберігаємо розшифровані дані
                        File.WriteAllText(saveDialog.FileName, json);

                        MessageBox.Show("Vault data exported successfully!\n\nWARNING: The exported file is NOT encrypted. Keep it secure!",
                            "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("No vault data found to export.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "WARNING: Importing data will REPLACE all current passwords in your vault!\n\nDo you want to continue?",
                    "Confirm Import",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                    return;

                var openDialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Import Vault Data"
                };

                if (openDialog.ShowDialog() == true)
                {
                    string json = File.ReadAllText(openDialog.FileName);

                    // Перевіряємо чи це валідний JSON з паролями
                    var records = JsonConvert.DeserializeObject<List<PasswordRecord>>(json);

                    if (records != null)
                    {
                        // Шифруємо та зберігаємо
                        string encrypted = CryptoService.Encrypt(json, AppState.CurrentMasterPassword);
                        File.WriteAllText(AppState.CurrentUserFilePath, encrypted);

                        MessageBox.Show($"Successfully imported {records.Count} password record(s)!",
                            "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                        SettingsChanged = true;

                        DataUpdated?.Invoke();
                    }
                    else
                    {
                        MessageBox.Show("Invalid data format. Please select a valid PVault export file.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Import failed: {ex.Message}\n\nMake sure you selected a valid PVault export file.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearAllData_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "WARNING: This will permanently delete ALL passwords in your vault!\n\nThis action CANNOT be undone!\n\nAre you absolutely sure?",
                "Confirm Clear All Data",
                MessageBoxButton.YesNo,
                MessageBoxImage.Stop);

            if (result == MessageBoxResult.Yes)
            {
                var confirmResult = MessageBox.Show(
                    "Last chance! Are you REALLY sure you want to delete everything?",
                    "Final Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Створюємо порожній vault
                        var emptyList = new List<PasswordRecord>();
                        string json = JsonConvert.SerializeObject(emptyList);
                        string encrypted = CryptoService.Encrypt(json, AppState.CurrentMasterPassword);
                        File.WriteAllText(AppState.CurrentUserFilePath, encrypted);

                        MessageBox.Show("All vault data has been cleared.", "Data Cleared", MessageBoxButton.OK, MessageBoxImage.Information);

                        SettingsChanged = true;
                        DataUpdated?.Invoke();
                        this.DialogResult = true;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to clear data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}