using PersonalFinanceWinUI.App.DTO;

namespace PersonalFinanceApp.DTO;

public static class UserMapper
{
    public static UserDTO ToUserDTO(User user)
    {
        return new UserDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            Role = user.Role,
            MonthlyBudgetLimit = user.MonthlyBudgetLimit,
            ParentEmail = user.ParentEmail,
            CreatedAt = user.CreatedAt
        };
    }
}
