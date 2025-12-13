using PasswordVaultUSB.ViewModels;
using System;
using System.Windows;

namespace PasswordVaultUSB.Views
{
    public partial class RegisterView : Window
    {
        public RegisterView()
        {
            InitializeComponent();

            var viewModel = new RegisterViewModel();
            this.DataContext = viewModel;

            if (viewModel.CloseAction == null)
            {
                viewModel.CloseAction = new Action(this.Close);
            }
        }
    }
}