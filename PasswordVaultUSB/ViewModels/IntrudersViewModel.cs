using PasswordVaultUSB.Helpers;
using PasswordVaultUSB.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PasswordVaultUSB.ViewModels
{
    // Проста модель для відображення елемента списку (фото + дата)
    public class IntruderItem
    {
        public string FilePath { get; set; }
        public string TimestampDisplay { get; set; }
        public BitmapImage ImageSource { get; set; }
    }

    public class IntrudersViewModel : BaseViewModel
    {
        // --- Fields ---
        private IntruderItem _selectedItem;
        private readonly string _vaultRootPath;

        // --- Properties ---
        public ObservableCollection<IntruderItem> IntruderImages { get; set; }

        public IntruderItem SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public Action CloseAction { get; set; }

        // --- Commands ---
        public ICommand DeleteCommand { get; }
        public ICommand CloseCommand { get; }

        // --- Constructor ---
        public IntrudersViewModel(string vaultRootPath)
        {
            _vaultRootPath = vaultRootPath;
            IntruderImages = new ObservableCollection<IntruderItem>();

            DeleteCommand = new RelayCommand(ExecuteDelete);
            CloseCommand = new RelayCommand(obj => CloseAction?.Invoke());

            LoadImages();
        }

        // --- Methods ---
        private void LoadImages()
        {
            IntruderImages.Clear();

            // Отримуємо список файлів через сервіс
            var files = WebcamService.GetIntruderImages(_vaultRootPath);

            foreach (var file in files)
            {
                try
                {
                    var info = new FileInfo(file);

                    IntruderImages.Add(new IntruderItem
                    {
                        FilePath = file,
                        TimestampDisplay = info.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        ImageSource = LoadImageNoLock(file) // Завантажуємо без блокування файлу
                    });
                }
                catch
                {
                }
            }
        }

        private void ExecuteDelete(object obj)
        {
            if (SelectedItem == null) return;

            try
            {
                if (File.Exists(SelectedItem.FilePath))
                {
                    File.Delete(SelectedItem.FilePath);
                }

                IntruderImages.Remove(SelectedItem);
                SelectedItem = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot delete file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private BitmapImage LoadImageNoLock(string path)
        {
            try
            {
                var bitmap = new BitmapImage();

                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }

                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}