using Microsoft.Win32;
using Newtonsoft.Json;
using PasswordVaultUSB.Models;
using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
namespace PasswordVaultUSB.ViewModels {
    public partial class MainViewModel {
        #region Import / Export
        private async void ExecuteExport(object obj) {
            try {
                if (!File.Exists(AppState.CurrentUserFilePath)) {
                    MessageBox.Show("No vault data found to export.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var saveDialog = new SaveFileDialog {
                    Filter = "Encrypted Backup (*.dat)|*.dat",
                    Title = "Export Encrypted Vault Data",
                    FileName = $"PVault_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.dat"
                };
                if (saveDialog.ShowDialog() == true) {
                    Mouse.OverrideCursor = Cursors.Wait;
                    var masterPassword = SecureStringHelper.ToUnsecureString(AppState.CurrentMasterPassword);
                    var records = await _storageService.LoadDataAsync(AppState.CurrentUserFilePath, masterPassword, AppState.CurrentHardwareID);
                    string json = JsonConvert.SerializeObject(records, Formatting.Indented);
                    byte[] encryptedBytes = await Task.Run(() => CryptoService.Encrypt(json, masterPassword));
                    File.WriteAllBytes(saveDialog.FileName, encryptedBytes);
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Vault data exported successfully!", "Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    LogAction("Data exported to encrypted backup");
                }
            } catch (Exception ex) {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"Export failed: {ex.Message}");
            }
        }
        private async void ExecuteImport(object obj) {
            var openDialog = new OpenFileDialog { Filter = "Encrypted Backup (*.dat)|*.dat|JSON (*.json)|*.json" };
            if (openDialog.ShowDialog() == true) {
                Mouse.OverrideCursor = Cursors.Wait;
                try {
                    List<PasswordRecord> importedRecords;
                    string extension = Path.GetExtension(openDialog.FileName).ToLower();
                    string masterPassword = SecureStringHelper.ToUnsecureString(AppState.CurrentMasterPassword);
                    if (extension == ".json") {
                        string json = File.ReadAllText(openDialog.FileName);
                        importedRecords = JsonConvert.DeserializeObject<List<PasswordRecord>>(json);
                    } else {
                        byte[] encryptedBytes = File.ReadAllBytes(openDialog.FileName);
                        string json = await Task.Run(() => CryptoService.Decrypt(encryptedBytes, masterPassword));
                        importedRecords = JsonConvert.DeserializeObject<List<PasswordRecord>>(json);
                    }
                    if (importedRecords == null || importedRecords.Count == 0) return;

                    var currentRecords = await _storageService.LoadDataAsync(AppState.CurrentUserFilePath, masterPassword, AppState.CurrentHardwareID);
                    int added = 0;
                    foreach (var rec in importedRecords) {
                        if (!currentRecords.Any(r => r.Service == rec.Service && r.Login == rec.Login)) {
                            currentRecords.Add(rec);
                            added++;
                        }
                    }
                    if (added > 0) {
                        await _storageService.SaveDataAsync(AppState.CurrentUserFilePath, masterPassword, currentRecords, AppState.CurrentHardwareID);
                        LoadData();
                        MessageBox.Show($"Imported: {added} records", "Success");
                        LogAction($"Imported {added} records");
                    }
                } catch (Exception ex) {
                    MessageBox.Show($"Import failed: {ex.Message}", "Error");
                } finally { Mouse.OverrideCursor = null; }
            }
        }
        private async void ExecuteClearData(object obj) {
            if (MessageBox.Show("Permanently delete ALL passwords?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                try {
                    Passwords.Clear();
                    var masterPassword = SecureStringHelper.ToUnsecureString(AppState.CurrentMasterPassword);
                    await _storageService.SaveDataAsync(AppState.CurrentUserFilePath, masterPassword, new List<PasswordRecord>(), AppState.CurrentHardwareID);
                    LogAction("All data cleared by user");
                } catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
            }
        }
        #endregion
        #region Generator & Security
        private int _genLength = 16;
        private bool _genUseUpper = true, _genUseLower = true, _genUseDigits = true, _genUseSymbols = true;
        private string _generatedPassword;
        public int GenLength { get => _genLength; set => SetProperty(ref _genLength, Math.Max(4, Math.Min(64, value))); }
        public bool GenUseUpper { get => _genUseUpper; set => SetProperty(ref _genUseUpper, value); }
        public bool GenUseLower { get => _genUseLower; set => SetProperty(ref _genUseLower, value); }
        public bool GenUseDigits { get => _genUseDigits; set => SetProperty(ref _genUseDigits, value); }
        public bool GenUseSymbols { get => _genUseSymbols; set => SetProperty(ref _genUseSymbols, value); }
        public string GeneratedPassword { get => _generatedPassword; set => SetProperty(ref _generatedPassword, value); }
        private void ExecuteGenerateStandalone(object obj) {
            GeneratedPassword = PasswordGeneratorService.GeneratePassword(GenLength, GenUseLower, GenUseUpper, GenUseDigits, GenUseSymbols);
            LogAction("Generated new secure password");
        }

        private void ExecuteCopyGenerated(object obj) {
            if (!string.IsNullOrEmpty(GeneratedPassword))
                _clipboardService.CopyToClipboard(GeneratedPassword, AppSettings.AutoClearClipboard);
        }
        private void InitializeSecurityEvents() {
            _securityService.OnLogAction += LogAction;
            _securityService.OnLockRequested += (reason) => {
                ExecuteLockVault(null);
                MessageBox.Show(reason, "Security Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
            };
        }
        public void LogAction(string message) {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            Application.Current.Dispatcher.Invoke(() => {
                ActivityLog.Insert(0, $"[{timestamp}] {message}");
                if (ActivityLog.Count > 200) ActivityLog.RemoveAt(ActivityLog.Count - 1);
            });
        }
        private void ExecuteCopyPassword(object parameter) {
            if (parameter is PasswordRecord entry) {
                _clipboardService.CopyToClipboard(entry.Password, AppSettings.AutoClearClipboard);
                LogAction($"CLIPBOARD: Copied password for '{entry.Service}'");
                if (AppSettings.ShowPasswordOnCopy && !entry.IsPasswordVisible) {
                    entry.IsPasswordVisible = true;
                    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                    timer.Tick += (s, args) => { entry.IsPasswordVisible = false; timer.Stop(); };
                    timer.Start();
                }
            }
        }

        private void ExecuteLockVault(object obj) {
            _securityService.StopMonitoring();
            AppState.CurrentMasterPassword = null;
            AppState.CurrentUserFilePath = null;
            LogAction("Vault locked.");
            RequestLockView?.Invoke();
        }

        private void ExecuteViewIntruders(object obj) {
            if (string.IsNullOrEmpty(AppState.CurrentUserFilePath)) return;

            string vaultRoot = Directory.GetParent(AppState.CurrentUserFilePath).FullName;
            var vm = new IntrudersViewModel(vaultRoot);
            var window = new Views.IntrudersWindow();
            window.DataContext = vm;
            vm.CloseAction = window.Close;
            window.ShowDialog();
        }

        public void NotifyUserActivity() => _securityService.ResetAutoLockTimer();

        private bool FilterRecords(object item) {
            if (item is PasswordRecord entry) {
                if (IsFavoritesOnly && !entry.IsFavorite) return false;
                if (string.IsNullOrWhiteSpace(SearchText)) return true;

                var search = SearchText.ToLower();
                return (entry.Service?.ToLower().Contains(search) ?? false) ||
                       (entry.Login?.ToLower().Contains(search) ?? false) ||
                       (entry.Url?.ToLower().Contains(search) ?? false);
            }
            return false;
        }
        #endregion
        #region Audit
        private void ExecuteAudit(object obj) {
            if (Passwords == null || Passwords.Count == 0) {
                MessageBox.Show("Vault is empty.", "Audit");
                return;
            }
            try {
                var auditService = new PasswordAuditService();
                AuditResult result = auditService.PerformAudit(Passwords);
                var reportWindow = new Views.AuditReportWindow(result);
                reportWindow.Owner = Application.Current.MainWindow;
                reportWindow.ShowDialog();
                LogAction("Security audit performed");
            } catch (Exception ex) {
                LogAction($"Audit failed: {ex.Message}");
            }
        }
        #endregion
    }
}