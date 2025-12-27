using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace PasswordVaultUSB.Services {
    public static class BackupService {
        private const string BackupFolderName = "PasswordVaultBackups";
        private const int MaxBackupsToKeep = 5;
        public static async Task PerformBackupAsync(string sourceFilePath) {
            await Task.Run(() => {
                try {
                    if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath))
                        return;

                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string backupDir = Path.Combine(documentsPath, BackupFolderName);
                    if (!Directory.Exists(backupDir)) {
                        Directory.CreateDirectory(backupDir);
                    }
                    string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string backupFileName = $"{fileName}_{timestamp}.bak";
                    string destPath = Path.Combine(backupDir, backupFileName);
                    File.Copy(sourceFilePath, destPath, true);
                    CleanupOldBackups(backupDir, fileName);
                } catch { }
            });
        }
        private static void CleanupOldBackups(string backupDir, string baseFileName) {
            try {
                var directory = new DirectoryInfo(backupDir);
                var myBackups = directory.GetFiles($"{baseFileName}_*.bak")
                                         .OrderByDescending(f => f.CreationTime)
                                         .ToList();
                if (myBackups.Count > MaxBackupsToKeep) {
                    var filesToDelete = myBackups.Skip(MaxBackupsToKeep);
                    foreach (var file in filesToDelete) {
                        file.Delete();
                    }
                }
            } catch { }
        }
    }
}