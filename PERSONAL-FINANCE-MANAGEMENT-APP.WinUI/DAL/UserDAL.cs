using PersonalFinanceApp.DTO;
using PersonalFinanceWinUI.App.BLL;
using PersonalFinanceWinUI.App.DAL;
using PersonalFinanceWinUI.App.DTO;
using PersonalFinanceWinUI.App.Services;

namespace PersonalFinanceApp.DAL;

public class UserDAL
{
    private readonly FinanceRepository _repo = new();
    private readonly AuthLogic _auth = new(AppState.GetCloudApiKey(), AppState.CloudProjectId);

    public async Task<(bool Success, string Message)> Register(string fullName, string email, string password, UserRole role, string parentEmail)
    {
        return await _auth.RegisterAsync(fullName, email, password, role, parentEmail);
    }

    public async Task<(bool Success, UserDTO? User, string Message)> Login(string email, string password)
    {
        var result = await _auth.LoginAsync(email, password);
        return (result.Success, result.User is null ? null : UserMapper.ToUserDTO(result.User), result.Message);
    }

    public UserDTO? FindByEmail(string email)
    {
        var user = _repo.GetUserByEmail(email);
        return user is null ? null : UserMapper.ToUserDTO(user);
    }
}
