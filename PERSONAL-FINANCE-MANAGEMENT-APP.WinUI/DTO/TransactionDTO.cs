using PersonalFinanceWinUI.App.DTO;

namespace PersonalFinanceApp.DTO;

public class TransactionDTO : Transaction
{
    public bool IsIncome
    {
        get => Type == TransactionType.Income;
        set => Type = value ? TransactionType.Income : TransactionType.Expense;
    }

    public string CategoryName
    {
        get => Category;
        set => Category = value;
    }
}
