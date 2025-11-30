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
// 
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

        private void ToggleRegPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isRegPasswordVisible = !_isRegPasswordVisible;
            ToggleVisibility(RegPasswordInput, RegPasswordVisible, RegEyeIcon, _isRegPasswordVisible);
        }

        private void RegPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isRegPasswordVisible)
            {
                RegPasswordVisible.Text = RegPasswordInput.Password;
            }
        }

        private void RegPasswordVisible_TextChanged(object sender, RoutedEventArgs e)
        {
            if (_isRegPasswordVisible)
            {
                RegPasswordInput.Password = RegPasswordVisible.Text;
            }
        }

        private void ToggleConfPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isConfPasswordVisible = !_isConfPasswordVisible;
            ToggleVisibility(ConfPasswordInput, ConfPasswordVisible, ConfEyeIcon, _isConfPasswordVisible);
        }

        private void ConfPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isConfPasswordVisible)
            {
                ConfPasswordVisible.Text = ConfPasswordInput.Password;
            }
        }

        private void ConfPasswordVisible_TextChanged(object sender, RoutedEventArgs e)
        {
            if (_isConfPasswordVisible)
            {
                ConfPasswordInput.Password = ConfPasswordVisible.Text;
            }
        }
        private void ToggleVisibility(PasswordBox passbox, TextBox textBox, Image icon, bool isVisible)
        {
            if (isVisible)
            {
                textBox.Text = passbox.Password;
                textBox.Visibility = Visibility.Visible;
                passbox.Visibility = Visibility.Collapsed;
                icon.Source = new BitmapImage(new Uri("/Resources/visibility_off.png", UriKind.Relative));
            }
            else
            {
                passbox.Password = textBox.Text;
                passbox.Visibility = Visibility.Visible;
                textBox.Visibility = Visibility.Collapsed;
                icon.Source = new BitmapImage(new Uri("/Resources/visibility.png", UriKind.Relative));
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
                UsernameErrorText.Text = "Username is requred";
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
                ConfPasswordErrorText.Text = "Password do not match";
                ConfPasswordErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            if (hasError) { 
                return; 
            }

            MessageBox.Show($"User {username} created successfully!", "Success");

            var mainView = new MainView();
            mainView.Show();
            this.Close();

        }

        private void ResetErrors()
        {
            UsernameErrorText.Visibility = Visibility.Collapsed;
            PasswordErrorText.Visibility = Visibility.Collapsed;
            ConfPasswordErrorText.Visibility = Visibility.Collapsed;
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginView = new LoginView();
            loginView.Show();
            this.Close();
        }
    }
}
