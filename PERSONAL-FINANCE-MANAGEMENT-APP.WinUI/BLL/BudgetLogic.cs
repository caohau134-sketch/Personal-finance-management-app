namespace PersonalFinanceWinUI.App.BLL;

public enum BudgetStatus
{
    Safe,
    Warning,
    Danger
}

public static class BudgetLogic
{
    public static BudgetStatus Evaluate(double currentExpense, double budgetLimit)
    {
        if (budgetLimit <= 0) return BudgetStatus.Danger;
        var ratio = currentExpense / budgetLimit;
        if (ratio < 0.8) return BudgetStatus.Safe;
        if (ratio <= 1.0) return BudgetStatus.Warning;
        return BudgetStatus.Danger;
    }
}
