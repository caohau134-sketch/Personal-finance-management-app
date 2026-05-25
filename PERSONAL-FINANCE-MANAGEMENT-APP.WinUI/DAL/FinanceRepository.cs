using Microsoft.Data.Sqlite;
using PersonalFinanceWinUI.App.DTO;

namespace PersonalFinanceWinUI.App.DAL;

public class FinanceRepository
{
    private readonly string _connectionString = $"Data Source={DatabaseInitializer.DbPath}";

    public void UpsertUser(User user)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Users (Id, FullName, Email, PasswordHash, Role, MonthlyBudgetLimit, ParentEmail, CreatedAt)
            VALUES ($id, $name, $email, $hash, $role, $budget, $parent, $created)
            ON CONFLICT(Id) DO UPDATE SET
              FullName = excluded.FullName,
              Email = excluded.Email,
              PasswordHash = excluded.PasswordHash,
              Role = excluded.Role,
              MonthlyBudgetLimit = excluded.MonthlyBudgetLimit,
              ParentEmail = excluded.ParentEmail;
            """;
        cmd.Parameters.AddWithValue("$id", user.Id);
        cmd.Parameters.AddWithValue("$name", user.FullName);
        cmd.Parameters.AddWithValue("$email", user.Email);
        cmd.Parameters.AddWithValue("$hash", user.PasswordHash);
        cmd.Parameters.AddWithValue("$role", (int)user.Role);
        cmd.Parameters.AddWithValue("$budget", user.MonthlyBudgetLimit);
        cmd.Parameters.AddWithValue("$parent", (object?)user.ParentEmail ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$created", user.CreatedAt.ToString("O"));
        cmd.ExecuteNonQuery();
    }

    public User? GetUserByEmail(string email)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Users WHERE Email = $email LIMIT 1;";
        cmd.Parameters.AddWithValue("$email", email);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    public User? GetUserById(string userId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Users WHERE Id = $id LIMIT 1;";
        cmd.Parameters.AddWithValue("$id", userId);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    public void AddTransaction(Transaction tx)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Transactions (Id, UserId, Type, Amount, Category, Note, Date, IsEssential, Source, LinkedAccountId)
            VALUES ($id, $userId, $type, $amount, $category, $note, $date, $isEssential, $source, $linked);
            """;
        cmd.Parameters.AddWithValue("$id", tx.Id);
        cmd.Parameters.AddWithValue("$userId", tx.UserId);
        cmd.Parameters.AddWithValue("$type", (int)tx.Type);
        cmd.Parameters.AddWithValue("$amount", tx.Amount);
        cmd.Parameters.AddWithValue("$category", tx.Category);
        cmd.Parameters.AddWithValue("$note", tx.Note);
        cmd.Parameters.AddWithValue("$date", tx.Date.ToString("O"));
        cmd.Parameters.AddWithValue("$isEssential", tx.IsEssential ? 1 : 0);
        cmd.Parameters.AddWithValue("$source", tx.Source);
        cmd.Parameters.AddWithValue("$linked", tx.LinkedAccountId);
        cmd.ExecuteNonQuery();
    }

    public List<Transaction> GetTransactions(string userId, DateTime? from = null, DateTime? to = null)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT * FROM Transactions
            WHERE UserId = $userId
            AND ($from IS NULL OR Date >= $from)
            AND ($to IS NULL OR Date <= $to)
            ORDER BY Date DESC;
            """;
        cmd.Parameters.AddWithValue("$userId", userId);
        cmd.Parameters.AddWithValue("$from", from?.ToString("O") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$to", to?.ToString("O") ?? (object)DBNull.Value);

        var list = new List<Transaction>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Transaction
            {
                Id = reader.GetString(0),
                UserId = reader.GetString(1),
                Type = (TransactionType)reader.GetInt32(2),
                Amount = reader.GetDouble(3),
                Category = reader.GetString(4),
                Note = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                Date = DateTime.Parse(reader.GetString(6)),
                IsEssential = reader.GetInt32(7) == 1,
                Source = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                LinkedAccountId = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
            });
        }
        return list;
    }

    public void AddBankLink(BankLink bankLink)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO BankLinks (Id, UserId, ProviderName, MaskedNumber, AccountType, IsActive, LinkedAt)
            VALUES ($id, $userId, $provider, $masked, $type, $active, $linkedAt);
            """;
        cmd.Parameters.AddWithValue("$id", bankLink.Id);
        cmd.Parameters.AddWithValue("$userId", bankLink.UserId);
        cmd.Parameters.AddWithValue("$provider", bankLink.ProviderName);
        cmd.Parameters.AddWithValue("$masked", bankLink.MaskedNumber);
        cmd.Parameters.AddWithValue("$type", bankLink.AccountType);
        cmd.Parameters.AddWithValue("$active", bankLink.IsActive ? 1 : 0);
        cmd.Parameters.AddWithValue("$linkedAt", bankLink.LinkedAt.ToString("O"));
        cmd.ExecuteNonQuery();
    }

    public List<BankLink> GetBankLinks(string userId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM BankLinks WHERE UserId = $userId ORDER BY LinkedAt DESC;";
        cmd.Parameters.AddWithValue("$userId", userId);

        var list = new List<BankLink>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new BankLink
            {
                Id = reader.GetString(0),
                UserId = reader.GetString(1),
                ProviderName = reader.GetString(2),
                MaskedNumber = reader.GetString(3),
                AccountType = reader.GetString(4),
                IsActive = reader.GetInt32(5) == 1,
                LinkedAt = DateTime.Parse(reader.GetString(6))
            });
        }
        return list;
    }

    private static User MapUser(SqliteDataReader reader)
    {
        return new User
        {
            Id = reader.GetString(0),
            FullName = reader.GetString(1),
            Email = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            Role = (UserRole)reader.GetInt32(4),
            MonthlyBudgetLimit = reader.GetDouble(5),
            ParentEmail = reader.IsDBNull(6) ? null : reader.GetString(6),
            CreatedAt = DateTime.Parse(reader.GetString(7))
        };
    }
}
