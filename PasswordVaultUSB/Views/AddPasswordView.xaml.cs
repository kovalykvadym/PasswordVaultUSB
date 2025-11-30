using PasswordVaultUSB.Models;
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
    public partial class AddPasswordView : Window
    {
        public string Service { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }
        public string Url { get; private set; }
        public string Notes { get; private set; }

        private bool _isPasswordVisible = false;
        public AddPasswordView()
        {
            InitializeComponent();
        }

        public AddPasswordView(PasswordRecord recordToEdit) : this()
        {
            ServiceInput.Text = recordToEdit.Service;
            LoginInput.Text = recordToEdit.Login;
            PasswordInput.Password = recordToEdit.Password;
            UrlInput.Text = recordToEdit.Url;
            NotesInput.Text = recordToEdit.Notes;

            Title = "Edit information"; // МОЖЛИВО ЗМІНИТИ НА ЩОСЬ ПО МЕНШЕ

            SavePasswordButton.Content = "Update";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ResetErrors();

            bool hasError = false;

            if(string.IsNullOrWhiteSpace(ServiceInput.Text))
            {
                ServiceErrorText.Visibility = Visibility.Visible;
                hasError = true; 
            }

            if (string.IsNullOrWhiteSpace(LoginInput.Text))
            {
                LoginErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            string currentPassword = _isPasswordVisible ? PasswordVisible.Text : PasswordInput.Password;

            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                PasswordErrorText.Visibility = Visibility.Visible;
                hasError = true;
            }

            if(hasError)
            {
                return;
            }

            Service = ServiceInput.Text.Trim();
            Login = LoginInput.Text.Trim();
            Password = currentPassword.Trim();
            Url = UrlInput.Text.Trim();
            Notes = NotesInput.Text.Trim();

            DialogResult = true;
        }

        private void ResetErrors()
        {
            ServiceErrorText.Visibility = Visibility.Collapsed;
            LoginErrorText.Visibility = Visibility.Collapsed;
            PasswordErrorText.Visibility = Visibility.Collapsed;
        }

        private void Input_TextChanged(object sender, RoutedEventArgs e)
        {
            if(sender == ServiceInput)
            {
                ServiceErrorText.Visibility = Visibility.Collapsed;
            }

            if (sender == LoginInput)
            {
                LoginErrorText.Visibility = Visibility.Collapsed;
            }

            if (sender == PasswordInput || sender == PasswordVisible)
            {
                PasswordErrorText.Visibility = Visibility.Collapsed;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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
            if(!_isPasswordVisible)
            {
                if(PasswordInput.Password.Length > 0)
                {
                    PasswordErrorText.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void PasswordVisible_TextCanged(object sender, RoutedEventArgs e)
        {
            if (_isPasswordVisible)
            {
                if (PasswordVisible.Text.Length > 0)
                {
                    PasswordErrorText.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void UrlInput_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void NotesInput_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
