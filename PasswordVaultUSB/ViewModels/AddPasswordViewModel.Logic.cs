using PasswordVaultUSB.Services;
using System;
using System.Windows;
namespace PasswordVaultUSB.ViewModels {
    public partial class AddPasswordViewModel {
        private void ExecuteSave(object obj) {
            if (!ValidateInput()) return;

            CloseAction?.Invoke(true);
        }
        private void ExecuteCancel(object obj) {
            CloseAction?.Invoke(false);
        }
        private void ExecuteGeneratePassword(object obj) {
            try {
                Password = PasswordGeneratorService.GeneratePassword(16, true, true, true, true);
            } catch (Exception ex) {
                MessageBox.Show($"Password generation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool ValidateInput() {
            bool isValid = true;
            if (string.IsNullOrWhiteSpace(Service)) {
                IsServiceErrorVisible = true;
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(Login)) {
                IsLoginErrorVisible = true;
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(Password)) {
                IsPasswordErrorVisible = true;
                isValid = false;
            }
            return isValid;
        }
    }
}