using Newtonsoft.Json.Linq;
using PersonalFinanceWinUI.App.DTO;
using PersonalFinanceWinUI.App.Services;

namespace PersonalFinanceWinUI.App.BLL;

public class CloudDataService
{
    private readonly HttpClient _http = new();
    private readonly string _projectId;
    private readonly string _apiKey;

    public CloudDataService(string projectId, string apiKey)
    {
        _projectId = projectId;
        _apiKey = apiKey;
    }

    private bool IsReady =>
        !string.IsNullOrWhiteSpace(_projectId) &&
        !string.IsNullOrWhiteSpace(_apiKey) &&
        !_projectId.StartsWith("REPLACE_", StringComparison.OrdinalIgnoreCase) &&
        !_apiKey.StartsWith("REPLACE_", StringComparison.OrdinalIgnoreCase);

    private string BaseUrl => $"https://firestore.googleapis.com/v1/projects/{_projectId}/databases/(default)/documents";

    public async Task UpsertUserAsync(User user)
    {
        if (!IsReady) return;
        var url = $"{BaseUrl}/users/{user.Id}";
        var body = new JObject
        {
            ["fields"] = new JObject
            {
                ["id"] = Str(user.Id),
                ["fullName"] = Str(user.FullName),
                ["email"] = Str(user.Email),
                ["role"] = Str(user.Role.ToString()),
                ["monthlyBudgetLimit"] = Num(user.MonthlyBudgetLimit),
                ["parentEmail"] = Str(user.ParentEmail ?? string.Empty),
                ["createdAt"] = Str(user.CreatedAt.ToString("O"))
            }
        };
        await PatchAsync(url, body);
    }

    public async Task AddTransactionAsync(Transaction tx)
    {
        if (!IsReady) return;
        var url = $"{BaseUrl}/users/{tx.UserId}/transactions/{tx.Id}";
        var body = new JObject
        {
            ["fields"] = new JObject
            {
                ["id"] = Str(tx.Id),
                ["userId"] = Str(tx.UserId),
                ["type"] = Str(tx.Type.ToString()),
                ["amount"] = Num(tx.Amount),
                ["category"] = Str(tx.Category),
                ["note"] = Str(tx.Note),
                ["date"] = Str(tx.Date.ToString("O")),
                ["isEssential"] = Bool(tx.IsEssential),
                ["source"] = Str(tx.Source),
                ["linkedAccountId"] = Str(tx.LinkedAccountId)
            }
        };
        await PatchAsync(url, body);
    }

    public async Task<List<Transaction>> GetTransactionsAsync(string userId)
    {
        if (!IsReady) return new List<Transaction>();
        var url = $"{BaseUrl}/users/{userId}/transactions";
        var req = CreateRequest(HttpMethod.Get, url);
        var res = await _http.SendAsync(req);
        if (!res.IsSuccessStatusCode) return new List<Transaction>();
        var json = JObject.Parse(await res.Content.ReadAsStringAsync());
        var docs = json["documents"] as JArray;
        if (docs is null) return new List<Transaction>();

        return docs.Select(ParseTransaction).Where(x => x is not null).Cast<Transaction>().ToList();
    }

    public async Task AddBankLinkAsync(BankLink link)
    {
        if (!IsReady) return;
        var url = $"{BaseUrl}/users/{link.UserId}/bankLinks/{link.Id}";
        var body = new JObject
        {
            ["fields"] = new JObject
            {
                ["id"] = Str(link.Id),
                ["userId"] = Str(link.UserId),
                ["providerName"] = Str(link.ProviderName),
                ["maskedNumber"] = Str(link.MaskedNumber),
                ["accountType"] = Str(link.AccountType),
                ["isActive"] = Bool(link.IsActive),
                ["linkedAt"] = Str(link.LinkedAt.ToString("O"))
            }
        };
        await PatchAsync(url, body);
    }

    private async Task PatchAsync(string url, JObject body)
    {
        var req = CreateRequest(HttpMethod.Patch, url);
        req.Content = new StringContent(body.ToString(), System.Text.Encoding.UTF8, "application/json");
        await _http.SendAsync(req);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url)
    {
        var req = new HttpRequestMessage(method, url);
        if (!string.IsNullOrWhiteSpace(AppState.FirebaseIdToken))
        {
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AppState.FirebaseIdToken);
        }
        return req;
    }

    private static JObject Str(string v) => new() { ["stringValue"] = v };
    private static JObject Num(double v) => new() { ["doubleValue"] = v };
    private static JObject Bool(bool v) => new() { ["booleanValue"] = v };

    private static Transaction? ParseTransaction(JToken doc)
    {
        var f = doc["fields"];
        if (f is null) return null;
        return new Transaction
        {
            Id = GetStr(f, "id"),
            UserId = GetStr(f, "userId"),
            Type = Enum.TryParse<TransactionType>(GetStr(f, "type"), out var t) ? t : TransactionType.Expense,
            Amount = GetNum(f, "amount"),
            Category = GetStr(f, "category"),
            Note = GetStr(f, "note"),
            Date = DateTime.TryParse(GetStr(f, "date"), out var d) ? d : DateTime.Now,
            IsEssential = GetBool(f, "isEssential"),
            Source = GetStr(f, "source"),
            LinkedAccountId = GetStr(f, "linkedAccountId")
        };
    }

    private static string GetStr(JToken fields, string key) => fields[key]?["stringValue"]?.Value<string>() ?? string.Empty;
    private static double GetNum(JToken fields, string key)
    {
        var raw = fields[key]?["doubleValue"]?.Value<string>() ?? fields[key]?["integerValue"]?.Value<string>() ?? "0";
        return double.TryParse(raw, out var n) ? n : 0;
    }
    private static bool GetBool(JToken fields, string key) => fields[key]?["booleanValue"]?.Value<bool>() ?? false;
}
