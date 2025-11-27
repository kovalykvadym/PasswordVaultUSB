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
    /// <summary>
    /// Interaction logic for AddPasswordView.xaml
    /// </summary>
    public partial class AddPasswordView : Window
    {
        public string Service { get; private set; }
        public string Login { get; private set; }
        public string Password { get; private set; }
        public AddPasswordView()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(ServiceInput.Text) || string.IsNullOrEmpty(LoginInput.Text) || string.IsNullOrEmpty(PasswordInput.Password))
            {
                MessageBox.Show("Fill in all fiels.");
                return;
            }

            Service = ServiceInput.Text;
            Login = LoginInput.Text;
            Password = PasswordInput.Password;

            DialogResult = true;
        }
    }
}
