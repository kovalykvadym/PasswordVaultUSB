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
            var password = PasswordInput.Password;
            if(password != "1234")
            {
                MessageBox.Show("Wrong password");
                return;
            }
            var mainView = new MainView();
            mainView.Show();
            Application.Current.MainWindow = mainView;
            this.Close();

        }

        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            if (_isPasswordVisible)
            {
                PasswordVisible.Text = PasswordInput.Password;
                PasswordInput.Visibility = Visibility.Collapsed;
                PasswordVisible.Visibility = Visibility.Visible;

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
            }
        }

        private void PasswordVisible_TextChanged(object sender, RoutedEventArgs e)
        {
            if (_isPasswordVisible)
            {
                PasswordInput.Password = PasswordVisible.Text;
            }
        }

        
    }
}
