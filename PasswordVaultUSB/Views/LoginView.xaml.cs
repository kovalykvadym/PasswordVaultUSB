using PasswordVaultUSB.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PasswordVaultUSB.Views
{
    public partial class LoginView : Window
    {
        private bool _isPasswordVisible = false;

        public LoginView()
        {
            InitializeComponent();

            // Прив'язуємо ViewModel
            var viewModel = new LoginViewModel();
            this.DataContext = viewModel;

            // Дозволяємо ViewModel закрити це вікно
            if (viewModel.CloseAction == null)
            {
                viewModel.CloseAction = new Action(this.Close);
            }
        }

        // --- UI Logic Only (Visual Toggle) ---
        // Ця логіка залишається тут, бо стосується виключно візуального перемикання контролів

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
                // Скидання помилки ми тепер робимо у ViewModel, але тут візуальна синхронізація
            }
        }

        private void PasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isPasswordVisible)
            {
                PasswordInput.Password = PasswordVisible.Text;
            }
        }
    }
}