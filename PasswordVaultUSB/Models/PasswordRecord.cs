using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace PasswordVaultUSB.Models
{
    public class PasswordRecord : INotifyPropertyChanged
    {
        public string Service {  get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        private bool _isPasswordVisible;

        public bool IsPasswordVisible
        {
            get { return _isPasswordVisible; }
            set
            {
                if (_isPasswordVisible != value)
                {
                    _isPasswordVisible = value;
                    OnPropertyChanged(nameof(DisplayPassword));
                    OnPropertyChanged(nameof(EyeIconOpacity));
                }
            }
        }
        public string DisplayPassword => IsPasswordVisible ? Password : "*********";
        public double EyeIconOpacity => IsPasswordVisible ? 1.0 : 0.4;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string properyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(properyName));
        }
    }
}
