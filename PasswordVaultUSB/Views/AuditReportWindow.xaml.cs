using PasswordVaultUSB.Models;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
namespace PasswordVaultUSB.Views {
    public partial class AuditReportWindow : Window {
        public AuditReportWindow(AuditResult result) {
            InitializeComponent();
            SetupUI(result);
        }
        private void SetupUI(AuditResult result) {
            TotalText.Text = result.TotalCount.ToString();
            DupText.Text = result.DuplicateCount.ToString();
            WeakText.Text = result.WeakCount.ToString();
            OldText.Text = result.OldCount.ToString();
            GradeText.Text = result.SafetyGrade;
            switch (result.SafetyGrade) {
                case "A":
                    GradeBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
                    RecText.Text = "Great job! Your vault is secure.";
                    break;
                case "B":
                    GradeBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107"));
                    RecText.Text = "Good, but consider updating old passwords.";
                    break;
                case "C":
                    GradeBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
                    RecText.Text = "Warning: You have weak passwords. Make them complex.";
                    break;
                case "F":
                    GradeBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
                    RecText.Text = "CRITICAL: Duplicate passwords found! Change them immediately.";
                    break;
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DragMove();
        }
    }
}