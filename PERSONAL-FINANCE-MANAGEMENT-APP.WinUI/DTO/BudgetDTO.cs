namespace PersonalFinanceApp.DTO;

public class BudgetDTO
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal LimitAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount => LimitAmount - SpentAmount;
    public bool IsOverBudget => LimitAmount > 0 && SpentAmount > LimitAmount;
}
