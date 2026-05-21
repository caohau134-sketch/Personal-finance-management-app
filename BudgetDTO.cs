using System;
using PersonalFinanceApp.Models;

namespace PersonalFinanceApp.DTO
{
    public class BudgetDTO : BaseEntity
    {
        private decimal _limitAmount;
        public string CategoryName { get; set; }

        public decimal LimitAmount
        {
            get { return _limitAmount; }
            set
            {
                // Hạn mức có thể bằng 0 (không bắt buộc)
                if (value < 0) throw new ArgumentException("Hạn mức ngân sách không được âm.");
                _limitAmount = value;
            }
        }

        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount => LimitAmount - SpentAmount;
        public bool IsOverBudget => LimitAmount > 0 && SpentAmount > LimitAmount;

        public override string GetSummary()
        {
            string status = IsOverBudget ? "⚠ VƯỢT HẠN MỨC" : "✓ An toàn";
            return $"[{Id}] Ngân sách {CategoryName}: Tiêu {SpentAmount:#,##0} / Hạn mức {LimitAmount:#,##0} VND | {status}";
        }
    }
}
