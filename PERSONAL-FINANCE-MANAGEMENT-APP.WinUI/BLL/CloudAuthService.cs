using Newtonsoft.Json.Linq;

namespace PersonalFinanceWinUI.App.BLL;

public record CloudAuthResult(bool Success, string IdToken, string LocalId, string Message);

public class CloudAuthService
{
    private readonly HttpClient _httpClient = new();
    private readonly string _apiKey;

    public CloudAuthService(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<CloudAuthResult> RegisterCloudAccountAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.StartsWith("REPLACE_"))
        {
            return new CloudAuthResult(false, string.Empty, string.Empty, "Cloud chua cau hinh.");
        }

        var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}";
        var payload = new JObject
        {
            ["email"] = email,
            ["password"] = password,
            ["returnSecureToken"] = true
        };

        var response = await _httpClient.PostAsync(url, new StringContent(payload.ToString(), System.Text.Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode)
        {
            var detail = await ExtractFirebaseErrorAsync(response);
            return new CloudAuthResult(false, string.Empty, string.Empty, $"Dang ky cloud that bai: {detail}");
        }

        var json = JObject.Parse(await response.Content.ReadAsStringAsync());
        return new CloudAuthResult(
            true,
            json["idToken"]?.Value<string>() ?? string.Empty,
            json["localId"]?.Value<string>() ?? string.Empty,
            "OK");
    }

    public async Task<CloudAuthResult> LoginCloudAccountAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.StartsWith("REPLACE_"))
        {
            return new CloudAuthResult(false, string.Empty, string.Empty, "Cloud chua cau hinh.");
        }

        var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}";
        var payload = new JObject
        {
            ["email"] = email,
            ["password"] = password,
            ["returnSecureToken"] = true
        };
        var response = await _httpClient.PostAsync(url, new StringContent(payload.ToString(), System.Text.Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode)
        {
            var detail = await ExtractFirebaseErrorAsync(response);
            return new CloudAuthResult(false, string.Empty, string.Empty, $"Dang nhap cloud that bai: {detail}");
        }

        var json = JObject.Parse(await response.Content.ReadAsStringAsync());
        return new CloudAuthResult(
            true,
            json["idToken"]?.Value<string>() ?? string.Empty,
            json["localId"]?.Value<string>() ?? string.Empty,
            "OK");
    }

    private static async Task<string> ExtractFirebaseErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return $"{(int)response.StatusCode} {response.ReasonPhrase}";
            }

            var json = JObject.Parse(raw);
            var code = json["error"]?["message"]?.Value<string>();
            return string.IsNullOrWhiteSpace(code)
                ? $"{(int)response.StatusCode} {response.ReasonPhrase}"
                : code;
        }
        catch
        {
            return $"{(int)response.StatusCode} {response.ReasonPhrase}";
        }
    }
}
