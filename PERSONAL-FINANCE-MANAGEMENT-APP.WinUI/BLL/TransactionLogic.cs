using PersonalFinanceWinUI.App.DAL;
using PersonalFinanceWinUI.App.DTO;
using PersonalFinanceWinUI.App.Services;

namespace PersonalFinanceWinUI.App.BLL;

public class TransactionLogic
{
    private readonly FinanceRepository _repo = new();
    private readonly CloudDataService _cloud = new(AppState.CloudProjectId, AppState.CloudApiKey);

    public void AddTransaction(Transaction tx) => _repo.AddTransaction(tx);
    public async Task AddTransactionAsync(Transaction tx)
    {
        _repo.AddTransaction(tx);
        await _cloud.AddTransactionAsync(tx);
    }

    public List<Transaction> GetTransactions(string userId, DateTime? from = null, DateTime? to = null)
        => _repo.GetTransactions(userId, from, to);
    
    public async Task<List<Transaction>> GetTransactionsOnlineFirstAsync(string userId, DateTime? from = null, DateTime? to = null)
    {
        var cloud = await _cloud.GetTransactionsAsync(userId);
        if (cloud.Count == 0) return _repo.GetTransactions(userId, from, to);
        var filtered = cloud.AsEnumerable();
        if (from is not null) filtered = filtered.Where(x => x.Date >= from.Value);
        if (to is not null) filtered = filtered.Where(x => x.Date <= to.Value);
        return filtered.OrderByDescending(x => x.Date).ToList();
    }

    public double TotalIncome(IEnumerable<Transaction> list)
        => list.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount);

    public double TotalExpense(IEnumerable<Transaction> list)
        => list.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount);

    public Dictionary<string, double> ExpenseByCategory(IEnumerable<Transaction> list)
        => list.Where(x => x.Type == TransactionType.Expense)
               .GroupBy(x => x.Category)
               .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
}
