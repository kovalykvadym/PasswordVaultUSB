using PasswordVaultUSB.ViewModels;
using System;
using System.Windows;

namespace PasswordVaultUSB.Views
{
    public partial class SettingsView : Window
    {
        public Action DataUpdated { get; set; }

        public SettingsView()
        {
            InitializeComponent();

            var viewModel = new SettingsViewModel();
            this.DataContext = viewModel;

            viewModel.RequestClose = () =>
            {
                this.DialogResult = true;
                this.Close();
            };

            viewModel.DataUpdated = () =>
            {
                DataUpdated?.Invoke();
            };
        }
    }
}