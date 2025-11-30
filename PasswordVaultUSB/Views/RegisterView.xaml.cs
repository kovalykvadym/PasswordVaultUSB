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
    public partial class RegisterView : Window
    {
        private bool _isRegPasswordVisible = false;
        private bool _isConfPasswordVisible = false;
            
        public RegisterView()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RegisterButton_Click(sender, e);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ResetErrors();

            string username = RegUsernameInput.Text;
            string pass = _isRegPasswordVisible ? RegPasswordVisible.Text : RegPasswordInput.Password;
            string confirm = _isConfPasswordVisible ? ConfPasswordVisible.Text : ConfPasswordInput.Password;

            bool hasError = false;

            if (string.IsNullOrWhiteSpace(username))
            {
                UsernameErrorText.Text = "Username is required";
                UsernameErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(pass))
            {
                PasswordErrorText.Text = "Password is required";
                PasswordErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(confirm))
            {
                ConfPasswordErrorText.Text = "Please repeat your password";
                ConfPasswordErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }
            else if (pass != confirm) 
            {
                ConfPasswordErrorText.Text = "Passwords do not match";
                ConfPasswordErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (hasError)
            {
                return;
            }

            MessageBox.Show($"User {username} created successfully", "Success");

            var mainView = new MainView();
            Application.Current.MainWindow = mainView;
            mainView.Show();
            this.Close();
        }

        private void ResetErrors()
        {
            UsernameErrorText.Visibility = Visibility.Collapsed;
            PasswordErrorText.Visibility = Visibility.Collapsed;
            ConfPasswordErrorText.Visibility = Visibility.Collapsed;
        }
        
        private void RegUsernameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UsernameErrorText.Visibility == Visibility.Visible)
            {
                UsernameErrorText.Visibility = Visibility.Collapsed;
            }
        }

        private void RegPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isRegPasswordVisible)
            {
                RegPasswordVisible.Text = RegPasswordInput.Password;
                if (PasswordErrorText.Visibility == Visibility.Visible)
                {
                    PasswordErrorText.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void RegPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isRegPasswordVisible) 
            {
                RegPasswordInput.Password = RegPasswordVisible.Text;
                if (PasswordErrorText.Visibility == Visibility.Visible)
                {
                    PasswordErrorText.Visibility = Visibility.Collapsed;
                }
            } 
        }

        private void ToggleRegPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isRegPasswordVisible = !_isRegPasswordVisible;
            ToggleVisibility(RegPasswordInput, RegPasswordVisible, RegEyeIcon, _isRegPasswordVisible);
        }

        private void ConfPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isConfPasswordVisible)
            {
                ConfPasswordVisible.Text = ConfPasswordInput.Password;
                if (ConfPasswordErrorText.Visibility == Visibility.Visible)
                {
                    ConfPasswordErrorText.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ConfPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isConfPasswordVisible)
            {
                ConfPasswordInput.Password = ConfPasswordVisible.Text;
                if (ConfPasswordErrorText.Visibility == Visibility.Visible)
                {
                    ConfPasswordErrorText.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ToggleConfPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isConfPasswordVisible = !_isConfPasswordVisible;
            ToggleVisibility(ConfPasswordInput, ConfPasswordVisible, ConfEyeIcon, _isConfPasswordVisible);
        }

        private void ToggleVisibility(PasswordBox passBox, TextBox textBox, Image icon, bool isVisible) 
        {
            if (isVisible)
            {
                textBox.Text = passBox.Password;
                textBox.Visibility = Visibility.Visible;
                passBox.Visibility = Visibility.Collapsed;
                icon.Source = new BitmapImage(new Uri("/Resources/visibility_off.png", UriKind.Relative));
            }
            else
            {
                passBox.Password = textBox.Text;
                passBox.Visibility = Visibility.Visible;
                textBox.Visibility = Visibility.Collapsed;
                icon.Source = new BitmapImage(new Uri("/Resources/visibility.png", UriKind.Relative));
            }
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginView = new LoginView();
            loginView.Show();
            this.Close();
        }
    }
}
