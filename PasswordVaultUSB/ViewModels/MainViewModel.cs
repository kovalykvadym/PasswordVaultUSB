using PasswordVaultUSB.Models;
using PasswordVaultUSB.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security;
using System.Windows.Data;
using System.Windows.Input;

namespace PasswordVaultUSB.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        #region Fields (Services & State)
        private readonly StorageService _storageService;
        private readonly ClipboardService _clipboardService;
        private readonly SecurityService _securityService;
        private ICollectionView _passwordsView;
        #endregion

        #region Properties (Data & UI)
        public ObservableCollection<PasswordRecord> Passwords { get; set; }
        public ObservableCollection<string> ActivityLog { get; set; }
        public Action RequestLockView { get; set; }
        #endregion

        #region Properties (Search & Filter)
        private string _searchText;
        private bool _isFavoritesOnly;

        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) _passwordsView.Refresh(); }
        }

        public bool IsFavoritesOnly
        {
            get => _isFavoritesOnly;
            set { if (SetProperty(ref _isFavoritesOnly, value)) _passwordsView.Refresh(); }
        }
        #endregion

        #region Commands Definitions
        public ICommand DeleteCommand { get; private set; }
        public ICommand ToggleFavoriteCommand { get; private set; }
        public ICommand ChangeMasterPasswordCommand { get; private set; }
        public ICommand CopyPasswordCommand { get; private set; }
        public ICommand LockVaultCommand { get; private set; }
        public ICommand SaveSettingsCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand ImportCommand { get; private set; }
        public ICommand ClearDataCommand { get; private set; }
        public ICommand GenerateStandaloneCommand { get; private set; }
        public ICommand CopyGeneratedCommand { get; private set; }
        public ICommand ViewIntrudersCommand { get; private set; }
        public ICommand AuditCommand { get; private set; }
        #endregion

        public MainViewModel()
        {
            // Ініціалізація колекцій та сервісів
            Passwords = new ObservableCollection<PasswordRecord>();
            ActivityLog = new ObservableCollection<string>();
            AuditCommand = new Helpers.RelayCommand(ExecuteAudit);

            _storageService = new StorageService();
            _clipboardService = new ClipboardService();
            _securityService = new SecurityService();

            // Налаштування фільтрації
            _passwordsView = CollectionViewSource.GetDefaultView(Passwords);
            _passwordsView.Filter = FilterRecords;

            // Підписка на події безпеки
            InitializeSecurityEvents();

            // Ініціалізація команд
            InitializeCommands();

            // Запуск
            LogAction("Application started");
            LoadData();
            LoadSettingsToProperties();

            _securityService.StartMonitoring();
        }

        private void InitializeCommands()
        {
            DeleteCommand = new Helpers.RelayCommand(DeletePassword);
            ToggleFavoriteCommand = new Helpers.RelayCommand(ToggleFavorite);
            ChangeMasterPasswordCommand = new Helpers.RelayCommand(ExecuteChangeMasterPassword);
            CopyPasswordCommand = new Helpers.RelayCommand(ExecuteCopyPassword);
            LockVaultCommand = new Helpers.RelayCommand(ExecuteLockVault);
            SaveSettingsCommand = new Helpers.RelayCommand(ExecuteSaveSettings);
            ExportCommand = new Helpers.RelayCommand(ExecuteExport);
            ImportCommand = new Helpers.RelayCommand(ExecuteImport);
            ClearDataCommand = new Helpers.RelayCommand(ExecuteClearData);
            GenerateStandaloneCommand = new Helpers.RelayCommand(ExecuteGenerateStandalone);
            CopyGeneratedCommand = new Helpers.RelayCommand(ExecuteCopyGenerated);
            ViewIntrudersCommand = new Helpers.RelayCommand(ExecuteViewIntruders);
        }
    }
}