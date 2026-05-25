using PersonalFinanceApp.DTO;

namespace PersonalFinanceApp.BLL;

public class BudgetBLL
{
    public List<BudgetDTO> RecalculateAllBudgets(IEnumerable<TransactionDTO> allTransactions, IEnumerable<BudgetDTO> budgets)
    {
        foreach (var budget in budgets)
        {
            var spent = allTransactions
                .Where(t => !t.IsIncome && string.Equals(t.CategoryName, budget.CategoryName, StringComparison.OrdinalIgnoreCase))
                .Sum(t => (decimal)t.Amount);
            budget.SpentAmount = spent;
        }
        return budgets.ToList();
    }
}
