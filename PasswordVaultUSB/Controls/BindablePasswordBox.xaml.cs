using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PasswordVaultUSB.Controls
{
    public partial class BindablePasswordBox : UserControl
    {
        private bool _isPasswordVisible = false;
        private bool _isInternalChange = false;

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(BindablePasswordBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordPropertyChanged));

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public BindablePasswordBox()
        {
            InitializeComponent();
        }

        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = (BindablePasswordBox)d;
            if (box._isInternalChange) return;

            var newPassword = (string)e.NewValue;

            box._isInternalChange = true;
            if (box.PasswordBoxInput.Password != newPassword)
            {
                box.PasswordBoxInput.Password = newPassword ?? string.Empty;
            }
            if (box.TextBoxInput.Text != newPassword)
            {
                box.TextBoxInput.Text = newPassword ?? string.Empty;
            }
            box._isInternalChange = false;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_isInternalChange) return;

            _isInternalChange = true;
            Password = PasswordBoxInput.Password;
            if (_isPasswordVisible)
            {
                TextBoxInput.Text = PasswordBoxInput.Password;
            }
            _isInternalChange = false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInternalChange) return;

            _isInternalChange = true;
            Password = TextBoxInput.Text;
            if (!_isPasswordVisible)
            {
                PasswordBoxInput.Password = TextBoxInput.Text;
            }
            _isInternalChange = false;
        }

        private void ToggleVisibility_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                TextBoxInput.Text = PasswordBoxInput.Password;
                TextBoxInput.Visibility = Visibility.Visible;
                PasswordBoxInput.Visibility = Visibility.Collapsed;

                EyeIcon.Source = new BitmapImage(new Uri("/Resources/visibility_off.png", UriKind.Relative));

                TextBoxInput.Focus();
                TextBoxInput.CaretIndex = TextBoxInput.Text.Length;
            }
            else
            {
                PasswordBoxInput.Password = TextBoxInput.Text;
                PasswordBoxInput.Visibility = Visibility.Visible;
                TextBoxInput.Visibility = Visibility.Collapsed;

                EyeIcon.Source = new BitmapImage(new Uri("/Resources/visibility.png", UriKind.Relative));

                PasswordBoxInput.Focus();
            }
        }
    }
}