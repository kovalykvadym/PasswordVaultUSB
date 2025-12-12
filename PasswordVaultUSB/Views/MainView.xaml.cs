using PasswordVaultUSB.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PasswordVaultUSB.Views
{
    public partial class MainView : Window
    {
        private MainViewModel _viewModel;
        private List<Button> _menuButtons;
        private readonly Brush _activeBackground = (Brush)new BrushConverter().ConvertFromString("#3E3E42");
        private readonly Brush _inactiveBackground = Brushes.Transparent;
        private readonly Brush _activeForeground = Brushes.White;
        private readonly Brush _inactiveForeground = (Brush)new BrushConverter().ConvertFromString("#A0A0A0");

        public MainView()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            SetupMenuButtons();
        }

        // --- Залишаємо логіку скидання таймера (передаємо сигнал у VM) ---
        private void Window_PreviewInput(object sender, InputEventArgs e)
        {
            _viewModel.UpdateLastActivity();
        }

        // --- Візуальна логіка меню (кольори) ---
        private void SetupMenuButtons()
        {
            _menuButtons = new List<Button> { MyPasswordsButton, FavoritesButton, UsbButton };
            SetActiveMenuButton(MyPasswordsButton);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Логіка перемикання даних вже спрацювала через Command у XAML
            // Тут ми тільки міняємо колір
            if (sender is Button clickedButton)
            {
                SetActiveMenuButton(clickedButton);
            }
        }

        private void SetActiveMenuButton(Button activeButton)
        {
            foreach (var btn in _menuButtons.Where(b => b != null))
            {
                btn.Background = _inactiveBackground;
                btn.Foreground = _inactiveForeground;
            }

            if (activeButton != null)
            {
                activeButton.Background = _activeBackground;
                activeButton.Foreground = _activeForeground;
            }
        }
    }
}