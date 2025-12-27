using System.Collections.Generic;
namespace PasswordVaultUSB.Models {
    public class AuditResult {
        public int TotalCount { get; set; }
        public int DuplicateCount { get; set; }
        public int WeakCount { get; set; }
        public int OldCount { get; set; }
        public List<string> DuplicateServices { get; set; } = new List<string>();
        public string SafetyGrade {
            get {
                if (DuplicateCount > 0) return "F";
                if (WeakCount > 0) return "C";
                if (OldCount > 0) return "B";
                return "A";
            }
        }
    }
}