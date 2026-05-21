using System;
using System.Collections.Generic;
using PersonalFinanceApp.DAL;
using PersonalFinanceApp.DTO;

namespace PersonalFinanceApp.BLL
{
    public class TransactionBLL
    {
        private readonly TransactionDAL _transactionDAL;
        private readonly BudgetBLL _budgetBLL;

        public TransactionBLL()
        {
            _transactionDAL = new TransactionDAL();
            _budgetBLL = new BudgetBLL();
        }

        public List<TransactionDTO> GetAllTransactions()
        {
            return _transactionDAL.GetAll();
        }

        /// <summary>
        /// Thêm giao dịch mới và tự động cập nhật lại tình trạng ngân sách liên quan
        /// </summary>
        public bool AddTransaction(TransactionDTO transaction, out string warningMessage)
        {
            warningMessage = string.Empty;

            if (transaction == null) throw new ArgumentNullException(nameof(transaction), "Dữ liệu giao dịch trống.");
            if (transaction.Amount <= 0) throw new ArgumentException("Số tiền giao dịch phải lớn hơn 0.");
            if (string.IsNullOrWhiteSpace(transaction.CategoryName)) throw new ArgumentException("Danh mục không được bỏ trống.");

            transaction.CategoryName = transaction.CategoryName.Trim();
            
            // Thực hiện thêm giao dịch vào file lưu trữ
            bool result = _transactionDAL.Insert(transaction);

            if (result)
            {
                // Đồng bộ và tính toán lại ngân sách chi tiêu
                _budgetBLL.RecalculateAllBudgets(_transactionDAL.GetAll());

                // Nếu là khoản chi, kiểm tra xem có chạm ngưỡng vượt hạn mức không để đưa ra cảnh báo cho GUI hiển thị
                if (!transaction.IsIncome)
                {
                    var budget = _budgetBLL.GetBudgetByCategory(transaction.CategoryName);
                    if (budget != null && budget.IsOverBudget)
                    {
                        warningMessage = $"⚠ Cảnh báo: Danh mục chi tiêu '{budget.CategoryName}' đã vượt quá hạn mức ngân sách cho phép!";
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Cập nhật giao dịch cũ và tính toán lại dòng ngân sách bị tác động
        /// </summary>
        public bool UpdateTransaction(TransactionDTO transaction, out string warningMessage)
        {
            warningMessage = string.Empty;

            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            if (transaction.Amount <= 0) throw new ArgumentException("Số tiền giao dịch phải lớn hơn 0.");

            transaction.CategoryName = transaction.CategoryName.Trim();
            bool result = _transactionDAL.Update(transaction);

            if (result)
            {
                _budgetBLL.RecalculateAllBudgets(_transactionDAL.GetAll());

                if (!transaction.IsIncome)
                {
                    var budget = _budgetBLL.GetBudgetByCategory(transaction.CategoryName);
                    if (budget != null && budget.IsOverBudget)
                    {
                        warningMessage = $"⚠ Cảnh báo: Sau khi cập nhật, danh mục '{budget.CategoryName}' đã bị vượt hạn mức ngân sách!";
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Xóa bỏ giao dịch và hoàn trả lại hạn mức trống cho ngân sách danh mục
        /// </summary>
        public bool DeleteTransaction(int id)
        {
            bool result = _transactionDAL.Delete(id);
            if (result)
            {
                // Sau khi xóa, tính toán giảm trừ số tiền tiêu thụ của danh mục ngân sách đó
                _budgetBLL.RecalculateAllBudgets(_transactionDAL.GetAll());
            }
            return result;
        }

        /// <summary>
        /// Tính toán tổng số dư khả dụng hiện tại (Thu nhập - Chi tiêu)
        /// </summary>
        public decimal GetCurrentBalance()
        {
            var transactions = _transactionDAL.GetAll();
            decimal balance = 0;

            foreach (var t in transactions)
            {
                if (t.IsIncome) balance += t.Amount;
                else balance -= t.Amount;
            }

            return balance;
        }
    }
}
