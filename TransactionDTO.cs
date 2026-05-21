using System;
using PersonalFinanceApp.Models;

namespace PersonalFinanceApp.DTO
{
    public class TransactionDTO : BaseEntity
    {
        private decimal _amount;
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                if (value < 0) throw new ArgumentException("Số tiền giao dịch không được âm.");
                _amount = value;
            }
        }
        public string Note { get; set; }
        public string CategoryName { get; set; }
        public bool IsIncome { get; set; }

        public override string GetSummary()
        {
            string type = IsIncome ? "Thu" : "Chi";
            return $"[{Id}] {CreatedDate:dd/MM/yyyy} - {type} | {CategoryName}: {Amount:#,##0} VND" +
                   (string.IsNullOrEmpty(Note) ? "" : $" (Ghi chú: {Note})");
        }
    }
}
