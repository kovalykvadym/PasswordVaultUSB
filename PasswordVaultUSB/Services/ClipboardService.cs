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
            timer.Tick += (s, e) =>
            {
                try
                {
                    // Only clear if the clipboard still contains our text
                    if (Clipboard.ContainsText() && Clipboard.GetText() == textToClear)
                    {
                        Clipboard.Clear();
                    }
                }
                catch { /* Ignore clipboard access errors */ }

                timer.Stop();
            };
            timer.Start();
        }
    }
}