using PasswordVaultUSB.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class MainView : Window
    {
        public ObservableCollection<PasswordRecord> Passwords { get; set; }
        public MainView()
        {
            InitializeComponent();

            Passwords = new ObservableCollection<PasswordRecord>();

            Passwords.Add(new PasswordRecord {
                Service = "Google",
                Login = "my.email@gmail.com",
                Password = "secretPassword123",
                Url = "google.com"
            });

            Passwords.Add(new PasswordRecord
            {
                Service = "Facebook",
                Login = "mark.zuck",
                Password = "qwerty",
                Url = "fb.com"
            });

            PasswordsGrid.ItemsSource = Passwords;

        }

        private void AddPassword_Click(object sender, RoutedEventArgs e)
        {
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

                Passwords.Add(newEntry);
            }
        }

        private void CopyPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var entry = button.DataContext as PasswordRecord;

            if (entry != null)
            {
                Clipboard.SetText(entry.Password);
                MessageBox.Show($"Password for {entry.Service} copied to clipboard", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EditPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var entry = button.DataContext as PasswordRecord;

            if (entry != null)
            {
                var editWindow = new AddPasswordView(entry);

                if(editWindow.ShowDialog() == true)
                {
                    entry.Service = editWindow.Service;
                    entry.Login = editWindow.Login;
                    entry.Password = editWindow.Password;
                    entry.Url = editWindow.Url;
                    entry.Notes = editWindow.Notes;

                    PasswordsGrid.Items.Refresh();

                    MessageBox.Show("Record updated successflly!", "Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void DeletePassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var entry = button.DataContext as PasswordRecord;

            if (entry != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete password for '{entry.Service}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes) 
                {
                    Passwords.Remove(entry);
                }
            }

        }


        private void ToggleShowPassword_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var entry = button.DataContext as PasswordRecord;

            if (entry != null)
            {
                entry.IsPasswordVisible = !entry.IsPasswordVisible;
            }
        }


        private void LockVault_Click(object sender, RoutedEventArgs e)
        {
            var loginView = new LoginView();
            loginView.Show();
            this.Close();
        }
    }
}