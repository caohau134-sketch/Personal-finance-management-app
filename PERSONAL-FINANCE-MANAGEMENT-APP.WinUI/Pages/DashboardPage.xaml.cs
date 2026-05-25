using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PersonalFinanceWinUI.App.BLL;
using PersonalFinanceWinUI.App.Services;

namespace PersonalFinanceWinUI.App.Pages;

public sealed partial class DashboardPage : Page
{
    private readonly TransactionLogic _logic = new();

    public DashboardPage()
    {
        this.InitializeComponent();
        this.Loaded += async (_, _) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var user = AppState.CurrentUser;
        if (user is null) return;

        var now = DateTime.Now;
        var from = new DateTime(now.Year, now.Month, 1);
        var list = await _logic.GetTransactionsOnlineFirstAsync(user.Id, from, now);
        var income = _logic.TotalIncome(list);
        var expense = _logic.TotalExpense(list);
        var balance = income - expense;

        IncomeText.Text = $"Tong thu: {income:N0} VND";
        ExpenseText.Text = $"Tong chi: {expense:N0} VND";
        BalanceText.Text = $"So du: {balance:N0} VND";

        var ratio = user.MonthlyBudgetLimit <= 0 ? 100 : Math.Min(100, (expense / user.MonthlyBudgetLimit) * 100);
        BudgetProgress.Value = ratio;
        var status = BudgetLogic.Evaluate(expense, user.MonthlyBudgetLimit);
        BudgetStatusText.Text = $"Ngan sach: {status} ({ratio:N0}% han muc)";
        BudgetStatusText.Foreground = status switch
        {
            BudgetStatus.Safe => new SolidColorBrush(Colors.ForestGreen),
            BudgetStatus.Warning => new SolidColorBrush(Colors.DarkOrange),
            _ => new SolidColorBrush(Colors.IndianRed)
        };

        CategoryList.ItemsSource = _logic.ExpenseByCategory(list).Select(x => $"{x.Key}: {x.Value:N0} VND").ToList();
        RecentTransactionsList.ItemsSource = list.Take(10).Select(x => $"{x.Date:dd/MM} | {x.Type} | {x.Category} | {x.Amount:N0}").ToList();
    }
}
