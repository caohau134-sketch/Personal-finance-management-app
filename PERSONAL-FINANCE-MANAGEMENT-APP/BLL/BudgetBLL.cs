using System;
using System.Collections.Generic;
using System.Linq;
using PersonalFinanceApp.DAL;
using PersonalFinanceApp.DTO;

namespace PersonalFinanceApp.BLL
{
    public class BudgetBLL
    {
        private readonly BudgetDAL _budgetDAL;

        public BudgetBLL()
        {
            _budgetDAL = new BudgetDAL();
        }

        public List<BudgetDTO> GetAllBudgets()
        {
            return _budgetDAL.GetAll();
        }

        public BudgetDTO GetBudgetById(int id)
        {
            return _budgetDAL.GetAll().FirstOrDefault(b => b.Id == id);
        }

        public BudgetDTO GetBudgetByCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName)) return null;
            return _budgetDAL.GetAll().FirstOrDefault(b => b.CategoryName.Equals(categoryName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public bool AddBudget(BudgetDTO budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget), "Dữ liệu ngân sách trống.");
            if (string.IsNullOrWhiteSpace(budget.CategoryName)) throw new ArgumentException("Tên danh mục không được để trống.");

            // Kiểm tra xem danh mục này đã được thiết lập ngân sách trước đó chưa
            var existing = GetBudgetByCategory(budget.CategoryName);
            if (existing != null) throw new InvalidOperationException($"Danh mục '{budget.CategoryName}' đã được thiết lập ngân sách trước đó.");

            budget.CategoryName = budget.CategoryName.Trim();
            return _budgetDAL.Insert(budget);
        }

        public bool UpdateBudget(BudgetDTO budget)
        {
            if (budget == null) throw new ArgumentNullException(nameof(budget));
            if (string.IsNullOrWhiteSpace(budget.CategoryName)) throw new ArgumentException("Tên danh mục không được để trống.");

            return _budgetDAL.Update(budget);
        }

        public bool DeleteBudget(int id)
        {
            return _budgetDAL.Delete(id);
        }

        /// <summary>
        /// Tính toán lại toàn bộ số tiền đã tiêu (SpentAmount) của các ngân sách dựa trên danh sách giao dịch hiện tại.
        /// </summary>
        public void RecalculateAllBudgets(List<TransactionDTO> allTransactions)
        {
            var budgets = _budgetDAL.GetAll();
            foreach (var budget in budgets)
            {
                // Lọc các giao dịch chi tiêu (IsIncome = false) thuộc danh mục này
                decimal totalSpent = allTransactions
                    .Where(t => !t.IsIncome && t.CategoryName.Equals(budget.CategoryName, StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Amount);

                budget.SpentAmount = totalSpent;
                _budgetDAL.Update(budget);
            }
        }
    }
}
