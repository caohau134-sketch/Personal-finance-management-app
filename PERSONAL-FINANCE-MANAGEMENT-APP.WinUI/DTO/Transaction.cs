namespace PersonalFinanceWinUI.App.DTO;

public enum TransactionType
{
    Income = 0,
    Expense = 1
}

public class Transaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public double Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
    public bool IsEssential { get; set; }
    public string Source { get; set; } = string.Empty;
    public string LinkedAccountId { get; set; } = string.Empty;
}
