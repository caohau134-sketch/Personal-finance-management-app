using PersonalFinanceApp.DTO;
using PersonalFinanceWinUI.App.BLL;

namespace PersonalFinanceApp.BLL;

public class TransactionBLL
{
    private readonly TransactionLogic _logic = new();

    public async Task<bool> AddTransaction(TransactionDTO transaction, Action<string>? setWarning = null)
    {
        await _logic.AddTransactionAsync(transaction);
        return true;
    }

    public async Task<List<TransactionDTO>> GetAllTransactions(string userId, DateTime? from = null, DateTime? to = null)
    {
        var list = await _logic.GetTransactionsOnlineFirstAsync(userId, from, to);
        return list.Select(t => new TransactionDTO
        {
            Id = t.Id,
            UserId = t.UserId,
            Type = t.Type,
            Amount = t.Amount,
            Category = t.Category,
            Note = t.Note,
            Date = t.Date,
            IsEssential = t.IsEssential,
            Source = t.Source,
            LinkedAccountId = t.LinkedAccountId
        }).ToList();
    }

    public decimal GetCurrentBalance(IEnumerable<TransactionDTO> transactions)
    {
        var income = transactions.Where(t => t.IsIncome).Sum(t => (decimal)t.Amount);
        var expense = transactions.Where(t => !t.IsIncome).Sum(t => (decimal)t.Amount);
        return income - expense;
    }
}
