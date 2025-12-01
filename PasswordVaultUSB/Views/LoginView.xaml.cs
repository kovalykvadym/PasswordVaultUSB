using PasswordVaultUSB.ViewModels;
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

namespace PasswordVaultUSB.Views
{
    public partial class LoginView : Window
    {
        private LoginViewModel ViewModel => DataContext as LoginViewModel;
        public LoginView()
        {
            InitializeComponent();
            PasswordInput.PasswordChanged += PasswordInput_PasswordChanged;
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null && !ViewModel.IsPasswordVisible)
            {
                ViewModel.Password = PasswordInput.Password;
            }
        }
    }
}
