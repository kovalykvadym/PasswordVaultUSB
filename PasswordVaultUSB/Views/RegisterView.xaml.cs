using PasswordVaultUSB.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PasswordVaultUSB.Views
{
    public partial class RegisterView : Window
    {
        private bool _isRegPasswordVisible = false;
        private bool _isConfPasswordVisible = false;

        public RegisterView()
        {
            InitializeComponent();

            var viewModel = new RegisterViewModel();
            this.DataContext = viewModel;

            // Налаштування закриття вікна
            if (viewModel.CloseAction == null)
            {
                viewModel.CloseAction = new Action(this.Close);
            }
        }

        // --- Visual Toggle Logic Only (No Business Logic) ---

        private void ToggleRegPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isRegPasswordVisible = !_isRegPasswordVisible;
            ToggleVisibility(RegPasswordInput, RegPasswordVisible, RegEyeIcon, _isRegPasswordVisible);
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

        // Синхронізація TextBox -> PasswordBox
        private void RegPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isRegPasswordVisible) RegPasswordInput.Password = RegPasswordVisible.Text;
        }

        private void ConfPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isConfPasswordVisible) ConfPasswordInput.Password = ConfPasswordVisible.Text;
        }

        // Синхронізація PasswordBox -> TextBox (щоб при перемиканні очка текст був актуальний)
        private void RegPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isRegPasswordVisible) RegPasswordVisible.Text = RegPasswordInput.Password;
        }

        private void ConfPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isConfPasswordVisible) ConfPasswordVisible.Text = ConfPasswordInput.Password;
        }
    }
}