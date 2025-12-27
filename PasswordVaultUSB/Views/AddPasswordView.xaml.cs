using PasswordVaultUSB.Models;
using PasswordVaultUSB.ViewModels;
using System.Windows;
namespace PasswordVaultUSB.Views {
    public partial class AddPasswordView : Window {
        private AddPasswordViewModel _viewModel;
        public string Service => _viewModel.Service;
        public string Login => _viewModel.Login;
        public string Password => _viewModel.Password;
        public string Url => _viewModel.Url;
        public string Notes => _viewModel.Notes;
        public AddPasswordView() {
            InitializeComponent();
            InitializeViewModel(null);
        }
        public AddPasswordView(PasswordRecord recordToEdit) {
            InitializeComponent();
            InitializeViewModel(recordToEdit);
        }
        private void InitializeViewModel(PasswordRecord record) {
            _viewModel = new AddPasswordViewModel(record);
            this.DataContext = _viewModel;
            _viewModel.CloseAction = (result) => {
                this.DialogResult = result;
                this.Close();
            };
        }
    }
}