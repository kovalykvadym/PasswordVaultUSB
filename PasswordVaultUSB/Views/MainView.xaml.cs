using PasswordVaultUSB.Models;
using PasswordVaultUSB.ViewModels;
using System;
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

            _viewModel.RequestLockView += () =>
            {
                var loginView = new LoginView();
                loginView.Show();

                var windowsToClose = new List<Window>();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window != loginView)
                    {
                        windowsToClose.Add(window);
                    }
                }

                foreach (var window in windowsToClose)
                {
                    window.Close();
                }
            };

            PasswordsGrid.ItemsSource = _viewModel.Passwords;
            SetupMenuButtons();
        }

        private void SetupMenuButtons()
        {
            _menuButtons = new List<Button> { MyPasswordsButton, FavoritesButton, UsbButton, GeneratorButton, SettingsButton };
            SetActiveMenuButton(MyPasswordsButton);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                SetActiveMenuButton(clickedButton);

                _viewModel.IsFavoritesOnly = (clickedButton == FavoritesButton);

                bool showUsbPanel = (clickedButton == UsbButton);
                bool showSettingsPanel = (clickedButton == SettingsButton);
                bool showGeneratorPanel = (clickedButton == GeneratorButton);

                if (showSettingsPanel)
                {
                    _viewModel.LoadSettingsToProperties();
                }

                TogglePanels(showUsbPanel, showSettingsPanel, showGeneratorPanel);
            }
        }

        private void TogglePanels(bool showUsb, bool showSettings, bool showGenerator)
        {
            ChangePasswordPanel.Visibility = showUsb ? Visibility.Visible : Visibility.Collapsed;
            SettingsPanel.Visibility = showSettings ? Visibility.Visible : Visibility.Collapsed;

            if (GeneratorPanel != null)
            {
                GeneratorPanel.Visibility = showGenerator ? Visibility.Visible : Visibility.Collapsed;
            }

            bool showMainContent = !showUsb && !showSettings && !showGenerator;

            PasswordsGrid.Visibility = showMainContent ? Visibility.Visible : Visibility.Collapsed;
            if (HeaderGrid != null)
            {
                HeaderGrid.Visibility = showMainContent ? Visibility.Visible : Visibility.Collapsed;
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

        private void LogAction(string message)
        {
            _viewModel.LogAction(message);
        }


        private void AddPassword_Click(object sender, RoutedEventArgs e)
        {
            LogAction("Opening 'Add Password' dialog");
            var addWindow = new AddPasswordView();

            if (addWindow.ShowDialog() == true)
            {
                var newEntry = new PasswordRecord
                {
                    Service = addWindow.Service,
                    Login = addWindow.Login,
                    Password = addWindow.Password,
                    Url = addWindow.Url,
                    Notes = addWindow.Notes
                };

                _viewModel.AddNewRecord(newEntry);
            }
            else
            {
                LogAction("'Add Password' dialog cancelled");
            }
        }

        private void EditPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is PasswordRecord entry)
            {
                LogAction($"Opening edit dialog for '{entry.Service}'");
                var editWindow = new AddPasswordView(entry);

                if (editWindow.ShowDialog() == true)
                {
                    var updatedEntry = new PasswordRecord
                    {
                        Service = editWindow.Service,
                        Login = editWindow.Login,
                        Password = editWindow.Password,
                        Url = editWindow.Url,
                        Notes = editWindow.Notes,

                        IsFavorite = entry.IsFavorite,
                        IsPasswordVisible = false
                    };

                    _viewModel.UpdateRecord(entry, updatedEntry);
                }
                else
                {
                    LogAction($"Edit cancelled for '{entry.Service}'");
                }
            }
        }

        private void ToggleShowPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button.DataContext is PasswordRecord entry)
            {
                entry.IsPasswordVisible = !entry.IsPasswordVisible;
                string visibility = entry.IsPasswordVisible ? "VISIBLE" : "HIDDEN";
                LogAction($"Password visibility toggled to {visibility} for '{entry.Service}'");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            LogAction("Main window closing");
            base.OnClosed(e);
        }

        private void ResetAutoLockTimer(object sender, InputEventArgs e)
        {
            _viewModel.NotifyUserActivity();
        }
    }
}