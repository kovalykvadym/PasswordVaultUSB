using System;
using System.IO;
using System.Windows.Threading;

namespace PasswordVaultUSB.Services
{
    // Сервіс фонового моніторингу: стежить за наявністю USB-ключа та активністю користувача
    public class SecurityService
    {
        private DispatcherTimer _usbCheckTimer;
        private DispatcherTimer _autoLockTimer;
        private DateTime _lastActivityTime;

        // Події для сповіщення головного вікна
        public event Action<string> OnLockRequested;
        public event Action<string> OnLogAction;

        public SecurityService()
        {
            _lastActivityTime = DateTime.Now;
        }

        public void StartMonitoring()
        {
            // 1. Таймер перевірки фізичної наявності USB
            _usbCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(AppSettings.UsbCheckInterval)
            };
            _usbCheckTimer.Tick += UsbCheckTimer_Tick;
            _usbCheckTimer.Start();

            OnLogAction?.Invoke($"USB monitoring started ({AppSettings.UsbCheckInterval}s)");

            // 2. Таймер автоблокування при неактивності (якщо увімкнено)
            if (AppSettings.AutoLockTimeout > 0)
            {
                // Перевіряємо статус кожні 10 секунд, щоб не навантажувати процесор
                _autoLockTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(10)
                };
                _autoLockTimer.Tick += AutoLockTimer_Tick;
                _autoLockTimer.Start();

                OnLogAction?.Invoke($"Auto-lock monitoring started ({AppSettings.AutoLockTimeout}m)");
            }
        }

        public void StopMonitoring()
        {
            _usbCheckTimer?.Stop();
            _autoLockTimer?.Stop();
        }

        // Викликається з UI (MainView) при будь-якому русі миші або натисканні клавіш
        public void ResetAutoLockTimer()
        {
            _lastActivityTime = DateTime.Now;
        }

        // Перезапускає таймери, якщо користувач змінив налаштування часу
        public void UpdateSettings()
        {
            StopMonitoring();
            StartMonitoring();
        }

        private void UsbCheckTimer_Tick(object sender, EventArgs e)
        {
            // Якщо файл бази даних зник (флешку витягли) — негайно блокуємо
            if (string.IsNullOrEmpty(AppState.CurrentUserFilePath) || !File.Exists(AppState.CurrentUserFilePath))
            {
                StopMonitoring();
                OnLogAction?.Invoke("USB Key removed - Triggering Lock");
                OnLockRequested?.Invoke("USB Key removed!");
            }
        }

        private void AutoLockTimer_Tick(object sender, EventArgs e)
        {
            if (AppSettings.AutoLockTimeout > 0)
            {
                var inactiveTime = DateTime.Now - _lastActivityTime;

                // Якщо час бездіяльності перевищив ліміт
                if (inactiveTime.TotalMinutes >= AppSettings.AutoLockTimeout)
                {
                    StopMonitoring();
                    OnLogAction?.Invoke("Auto-lock timeout reached");
                    OnLockRequested?.Invoke($"Vault locked due to {AppSettings.AutoLockTimeout} minutes of inactivity.");
                }
            }
        }
    }
}