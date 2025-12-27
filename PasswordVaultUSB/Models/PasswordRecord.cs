using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace PasswordVaultUSB.Models {
    public class PasswordRecord : INotifyPropertyChanged {
        private bool _isPasswordVisible;
        private bool _isFavorite;
        public string Service { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        private string _category = "Uncategorized";
        public string Category {
            get => _category;
            set {
                if (_category != value) {
                    _category = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsPasswordVisible {
            get => _isPasswordVisible;
            set {
                if (_isPasswordVisible != value) {
                    _isPasswordVisible = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayPassword));
                    OnPropertyChanged(nameof(EyeIconOpacity));
                }
            }
        }
        public bool IsFavorite {
            get => _isFavorite;
            set {
                if (_isFavorite != value) {
                    _isFavorite = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FavoriteIconOpacity));
                }
            }
        }
        public string DisplayPassword => IsPasswordVisible ? Password : "*********";
        public double EyeIconOpacity => IsPasswordVisible ? 1.0 : 0.6;
        public double FavoriteIconOpacity => IsFavorite ? 1.0 : 0.2;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}