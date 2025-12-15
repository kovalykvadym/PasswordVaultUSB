using System;
using System.Windows;
using System.Windows.Threading;

namespace PasswordVaultUSB.Services
{
    public class ClipboardService
    {
        public void CopyToClipboard(string text, bool autoClear)
        {
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                Clipboard.SetText(text);

                if (autoClear)
                {
                    StartClearTimer(text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy to clipboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartClearTimer(string textToClear)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };

            timer.Tick += (sender, args) =>
            {
                try
                {
                    // Перевіряємо, чи в буфері все ще наш пароль.
                    // Якщо користувач вже встиг скопіювати щось інше - не чіпаємо.
                    if (Clipboard.ContainsText() && Clipboard.GetText() == textToClear)
                    {
                        Clipboard.Clear();
                    }
                }
                catch
                {
                }
                finally
                {
                    timer.Stop();
                }
            };

            timer.Start();
        }
    }
}