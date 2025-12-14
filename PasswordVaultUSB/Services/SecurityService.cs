using System;
using System.IO;
using System.Windows.Threading;

namespace PasswordVaultUSB.Services
{
    public class SecurityService
    {
        private DispatcherTimer _usbCheckTimer;
        private DispatcherTimer _autoLockTimer;
        private DateTime _lastActivityTime;

        // Events
        public event Action<string> OnLockRequested;
        public event Action<string> OnLogAction;

        public SecurityService()
        {
            _lastActivityTime = DateTime.Now;
        }

        public void StartMonitoring()
        {
            // USB Monitoring
            _usbCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(AppSettings.UsbCheckInterval)
            };
            _usbCheckTimer.Tick += UsbCheckTimer_Tick;
            _usbCheckTimer.Start();

            OnLogAction?.Invoke($"USB monitoring started ({AppSettings.UsbCheckInterval}s)");

            // Auto-lock Monitoring
            if (AppSettings.AutoLockTimeout > 0)
            {
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

        public void ResetAutoLockTimer()
        {
            _lastActivityTime = DateTime.Now;
        }

        public void UpdateSettings()
        {
            StopMonitoring();
            StartMonitoring();
        }

        private void UsbCheckTimer_Tick(object sender, EventArgs e)
        {
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