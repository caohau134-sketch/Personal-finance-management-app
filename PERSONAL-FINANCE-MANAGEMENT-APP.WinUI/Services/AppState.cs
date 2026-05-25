using Newtonsoft.Json.Linq;
using PersonalFinanceWinUI.App.DTO;

namespace PersonalFinanceWinUI.App.Services;

public static class AppState
{
    public static User? CurrentUser { get; set; }
    public static string FirebaseIdToken { get; set; } = string.Empty;
    public static string FirebaseUid { get; set; } = string.Empty;
    public static string CloudProjectId => GetSetting("CloudAuth", "ProjectId");
    public static string CloudApiKey => GetSetting("CloudAuth", "ApiKey");
    public static bool IsCloudConfigured =>
        !string.IsNullOrWhiteSpace(CloudProjectId) &&
        !CloudProjectId.StartsWith("REPLACE_", StringComparison.OrdinalIgnoreCase) &&
        !string.IsNullOrWhiteSpace(CloudApiKey) &&
        !CloudApiKey.StartsWith("REPLACE_", StringComparison.OrdinalIgnoreCase);

    public static string GetCloudApiKey()
    {
        return CloudApiKey;
    }

    public static void ClearSession()
    {
        CurrentUser = null;
        FirebaseIdToken = string.Empty;
        FirebaseUid = string.Empty;
    }

    private static string GetSetting(string section, string key)
    {
        try
        {
            var jsonPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(jsonPath)) return string.Empty;
            var root = JObject.Parse(File.ReadAllText(jsonPath));
            return root[section]?[key]?.Value<string>() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
