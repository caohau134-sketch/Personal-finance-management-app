namespace PersonalFinanceWinUI.App.DTO;

public enum UserRole
{
    User = 0,
    Admin = 1
}

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public double MonthlyBudgetLimit { get; set; } = 5000000;
    public string? ParentEmail { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
