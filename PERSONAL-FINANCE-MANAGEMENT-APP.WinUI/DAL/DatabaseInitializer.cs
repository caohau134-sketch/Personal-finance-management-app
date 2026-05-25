using Microsoft.Data.Sqlite;

namespace PersonalFinanceWinUI.App.DAL;

public static class DatabaseInitializer
{
    public static string DbPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PersonalFinanceWinUI", "finance.db");

    public static void Initialize()
    {
        var folder = Path.GetDirectoryName(DbPath)!;
        Directory.CreateDirectory(folder);

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY,
                FullName TEXT NOT NULL,
                Email TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                Role INTEGER NOT NULL,
                MonthlyBudgetLimit REAL NOT NULL,
                ParentEmail TEXT NULL,
                CreatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Transactions (
                Id TEXT PRIMARY KEY,
                UserId TEXT NOT NULL,
                Type INTEGER NOT NULL,
                Amount REAL NOT NULL,
                Category TEXT NOT NULL,
                Note TEXT NULL,
                Date TEXT NOT NULL,
                IsEssential INTEGER NOT NULL,
                Source TEXT NULL,
                LinkedAccountId TEXT NULL
            );

            CREATE TABLE IF NOT EXISTS BankLinks (
                Id TEXT PRIMARY KEY,
                UserId TEXT NOT NULL,
                ProviderName TEXT NOT NULL,
                MaskedNumber TEXT NOT NULL,
                AccountType TEXT NOT NULL,
                IsActive INTEGER NOT NULL,
                LinkedAt TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }
}
