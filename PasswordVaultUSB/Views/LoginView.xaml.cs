using PasswordVaultUSB.ViewModels;
using System;
using System.Windows;

namespace PasswordVaultUSB.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();

            var viewModel = new LoginViewModel();
            this.DataContext = viewModel;

            if (viewModel.CloseAction == null)
            {
                viewModel.CloseAction = new Action(this.Close);
            }
        }
    }
}