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
        private bool _isPasswordVisible = false;
        public LoginView()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, e);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ResetErrors();

            string username = UsernameInput.Text;
            string password = PasswordInput.Password;

            bool hasError = false;

            if (string.IsNullOrWhiteSpace(username))
            {
                UsernameErrorText.Text = "Please enter your username!";
                UsernameErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (string.IsNullOrEmpty(password))
            {
                PasswordErrorText.Text = "Please enter your password!";
                PasswordErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }
            else if(password != "1234")
            {
                PasswordErrorText.Text = "Incorrect password. Please try again.";
                PasswordErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (hasError)
            {
                return;
            }

            var mainView = new MainView();
            Application.Current.MainWindow = mainView;
            mainView.Show();
            this.Close();
        }

        private void ResetErrors()
        {
            UsernameErrorText.Visibility = Visibility.Collapsed;
            PasswordErrorText.Visibility = Visibility.Collapsed;
        }

        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            if (_isPasswordVisible)
            {
                PasswordVisible.Text = PasswordInput.Password;
                PasswordVisible.Visibility = Visibility.Visible;
                PasswordInput.Visibility = Visibility.Collapsed;

                EyeIcon.Source = new BitmapImage(new Uri("/Resources/visibility_off.png", UriKind.Relative));
            }
            else
            {
                PasswordInput.Password = PasswordVisible.Text;
                PasswordInput.Visibility = Visibility.Visible;
                PasswordVisible.Visibility = Visibility.Collapsed;

                EyeIcon.Source = new BitmapImage(new Uri("/Resources/visibility.png", UriKind.Relative));
            }
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isPasswordVisible)
            {
                PasswordVisible.Text = PasswordInput.Password;
                if (PasswordErrorText.Visibility == Visibility.Visible)
                {
                    PasswordErrorText.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void PasswordVisible_TextChanged(object sender, RoutedEventArgs e)
        {
            if (_isPasswordVisible)
            {
                PasswordInput.Password = PasswordVisible.Text;
                if (PasswordErrorText.Visibility == Visibility.Visible)
                {
                    PasswordErrorText.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void GoToRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerView = new RegisterView();
            registerView.Show();
            this.Close();
        }
    }
}
