using PasswordVaultUSB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
namespace PasswordVaultUSB.Services {
    public class PasswordAuditService {
        public AuditResult PerformAudit(IEnumerable<PasswordRecord> records) {
            var result = new AuditResult {
                TotalCount = records.Count()
            };
            if (result.TotalCount == 0) return result;
            var duplicates = records.GroupBy(r => r.Password)
                                    .Where(g => g.Count() > 1)
                                    .ToList();
            result.DuplicateCount = duplicates.Sum(g => g.Count());
            foreach (var group in duplicates) {
                foreach (var record in group) {
                    result.DuplicateServices.Add(record.Service);
                }
            }
            foreach (var record in records) {
                if (IsWeak(record.Password)) result.WeakCount++;
                if (IsOld(record.CreatedDate)) result.OldCount++;
            }
            return result;
        }
        private bool IsWeak(string password) {
            if (string.IsNullOrEmpty(password)) return true;
            if (password.Length < 8) return true;
            int categories = 0;
            if (password.Any(char.IsUpper)) categories++;
            if (password.Any(char.IsLower)) categories++;
            if (password.Any(char.IsDigit)) categories++;
            if (password.Any(ch => !char.IsLetterOrDigit(ch))) categories++;
            return categories < 3;
        }
        private bool IsOld(DateTime createdDate) {
            return (DateTime.Now - createdDate).TotalDays > 365;
        }
    }
}