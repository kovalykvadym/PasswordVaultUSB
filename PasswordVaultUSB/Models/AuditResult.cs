using System.Collections.Generic;

namespace PasswordVaultUSB.Models
{
    public class AuditResult
    {
        public int TotalCount { get; set; }
        public int DuplicateCount { get; set; }
        public int WeakCount { get; set; }
        public int OldCount { get; set; }

        // Список сервісів, де є проблеми (для відображення деталей)
        public List<string> DuplicateServices { get; set; } = new List<string>();

        // Логіка оцінки безпеки (A, B, C, F)
        public string SafetyGrade
        {
            get
            {
                if (DuplicateCount > 0) return "F"; // Критично
                if (WeakCount > 0) return "C";      // Погано
                if (OldCount > 0) return "B";       // Нормально
                return "A";                         // Відмінно
            }
        }
    }
}