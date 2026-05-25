using PersonalFinanceWinUI.App.DAL;
using PersonalFinanceWinUI.App.DTO;
using PersonalFinanceWinUI.App.Services;

namespace PersonalFinanceWinUI.App.BLL;

public class AuthLogic
{
    private readonly FinanceRepository _repo = new();
    private readonly CloudAuthService _cloud;
    private readonly CloudDataService _cloudData;

    public AuthLogic(string cloudApiKey, string cloudProjectId)
    {
        _cloud = new CloudAuthService(cloudApiKey);
        _cloudData = new CloudDataService(cloudProjectId, cloudApiKey);
    }

    public async Task<(bool Success, string Message)> RegisterAsync(string name, string email, string password, UserRole role, string parentEmail)
    {
        if (_repo.GetUserByEmail(email) is not null)
        {
            return (false, "Email da ton tai.");
        }

        string userId = Guid.NewGuid().ToString();
        if (AppState.IsCloudConfigured)
        {
            var cloudRegister = await _cloud.RegisterCloudAccountAsync(email, password);
            if (!cloudRegister.Success)
            {
                return (false, cloudRegister.Message);
            }
            userId = cloudRegister.LocalId;
            AppState.FirebaseIdToken = cloudRegister.IdToken;
            AppState.FirebaseUid = cloudRegister.LocalId;
        }

        var user = new User
        {
            Id = userId,
            FullName = name,
            Email = email,
            PasswordHash = PasswordHasher.Hash(password),
            Role = role,
            ParentEmail = string.IsNullOrWhiteSpace(parentEmail) ? null : parentEmail
        };

        _repo.UpsertUser(user);
        await _cloudData.UpsertUserAsync(user);
        return (true, "Dang ky thanh cong.");
    }

    public async Task<(bool Success, User? User, string Message)> LoginAsync(string email, string password)
    {
        var user = _repo.GetUserByEmail(email);
        if (user is null)
        {
            return (false, null, "Tai khoan khong ton tai.");
        }

        if (user.PasswordHash != PasswordHasher.Hash(password))
        {
            return (false, null, "Sai mat khau.");
        }

        if (AppState.IsCloudConfigured)
        {
            var cloudLogin = await _cloud.LoginCloudAccountAsync(email, password);
            if (!cloudLogin.Success)
            {
                return (false, null, "Dang nhap cloud that bai. Kiem tra Firebase.");
            }
            AppState.FirebaseIdToken = cloudLogin.IdToken;
            AppState.FirebaseUid = cloudLogin.LocalId;
        }

        return (true, user, "Dang nhap thanh cong.");
    }
}
