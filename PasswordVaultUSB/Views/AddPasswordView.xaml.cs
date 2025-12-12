using PasswordVaultUSB.Models;
using PasswordVaultUSB.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PasswordVaultUSB.Views
{
    public partial class AddPasswordView : Window
    {
        private AddPasswordViewModel _viewModel;
        private bool _isPasswordVisible = false;

        // --- Властивості-проксі для зворотної сумісності з MainView ---
        public string Service => _viewModel.Service;
        public string Login => _viewModel.Login;
        public string Password => _viewModel.Password;
        public string Url => _viewModel.Url;
        public string Notes => _viewModel.Notes;

        public AddPasswordView()
        {
            InitializeComponent();
            InitializeViewModel(null);
        }

        public AddPasswordView(PasswordRecord recordToEdit)
        {
            InitializeComponent();
            InitializeViewModel(recordToEdit);
        }

        private void InitializeViewModel(PasswordRecord record)
        {
            _viewModel = new AddPasswordViewModel(record);
            this.DataContext = _viewModel;

            // Налаштовуємо закриття вікна
            _viewModel.CloseAction = (result) =>
            {
                this.DialogResult = result;
                this.Close();
            };

            // Якщо це редагування, треба заповнити PasswordBox вручну,
            // бо він не підтримує прямий Binding на запис
            if (record != null && !string.IsNullOrEmpty(record.Password))
            {
                PasswordInput.Password = record.Password;
            }
        }

        // --- UI Logic Only (Visual Toggle & Password Sync) ---

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

        // Синхронізація PasswordBox -> ViewModel
        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isPasswordVisible)
            {
                // Оновлюємо властивість у ViewModel, бо Binding не працює для PasswordBox
                _viewModel.Password = PasswordInput.Password;
            }
        }

        // Синхронізація TextBox -> PasswordBox (для перемикання "очка")
        private void PasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isPasswordVisible)
            {
                PasswordInput.Password = PasswordVisible.Text;
                // ViewModel оновлюється автоматично через Binding на TextBox
            }
        }
    }
}