namespace PersonalFinanceWinUI.App.DTO;

public class BankLink
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string MaskedNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = "Card";
    public bool IsActive { get; set; } = true;
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
}
