using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordVaultUSB.Services
{
    public static class BackupService
    {
        private const string BackupFolderName = "PasswordVaultBackups";
        private const int MaxBackupsToKeep = 5; // Зберігати лише 5 останніх копій

        public static async Task PerformBackupAsync(string sourceFilePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath))
                        return;

                    // 1. Визначаємо шлях до папки "Мої документи" на комп'ютері
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string backupDir = Path.Combine(documentsPath, BackupFolderName);

                    // 2. Створюємо папку, якщо її немає
                    if (!Directory.Exists(backupDir))
                    {
                        Directory.CreateDirectory(backupDir);
                    }

                    // 3. Формуємо ім'я файлу з датою: "User.dat" -> "User_20231215_123000.bak"
                    string fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string backupFileName = $"{fileName}_{timestamp}.bak";
                    string destPath = Path.Combine(backupDir, backupFileName);

                    // 4. Копіюємо файл
                    File.Copy(sourceFilePath, destPath, true);

                    // 5. Видаляємо старі бекапи (Rotation)
                    CleanupOldBackups(backupDir, fileName);
                }
                catch
                {
                    // Бекап - це допоміжна функція. Якщо не вдалося (немає прав/місця) - просто мовчимо,
                    // щоб не лякати користувача помилками при старті.
                }
            });
        }

        private static void CleanupOldBackups(string backupDir, string baseFileName)
        {
            try
            {
                var directory = new DirectoryInfo(backupDir);

                // Знаходимо всі файли бекапів для ЦЬОГО користувача
                var myBackups = directory.GetFiles($"{baseFileName}_*.bak")
                                         .OrderByDescending(f => f.CreationTime) // Найновіші зверху
                                         .ToList();

                // Якщо файлів більше ліміту - видаляємо зайві
                if (myBackups.Count > MaxBackupsToKeep)
                {
                    var filesToDelete = myBackups.Skip(MaxBackupsToKeep);
                    foreach (var file in filesToDelete)
                    {
                        file.Delete();
                    }
                }
            }
            catch { }
        }
    }
}