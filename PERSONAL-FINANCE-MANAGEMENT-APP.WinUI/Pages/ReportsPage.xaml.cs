using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PersonalFinanceWinUI.App.BLL;
using PersonalFinanceWinUI.App.Services;

namespace PersonalFinanceWinUI.App.Pages;

public sealed partial class ReportsPage : Page
{
    private readonly TransactionLogic _logic = new();

    public ReportsPage()
    {
        this.InitializeComponent();
        FromDate.Date = DateTimeOffset.Now.AddMonths(-1);
        ToDate.Date = DateTimeOffset.Now;
        this.Loaded += async (_, _) => await LoadDataAsync();
    }

    private async void Filter_Click(object sender, RoutedEventArgs e) => await LoadDataAsync();

    private async Task LoadDataAsync()
    {
        var user = AppState.CurrentUser;
        if (user is null) return;
        var from = FromDate.Date.DateTime;
        var to = ToDate.Date.DateTime;
        var list = await _logic.GetTransactionsOnlineFirstAsync(user.Id, from, to);
        ReportList.ItemsSource = list.Select(x => $"{x.Date:dd/MM/yyyy} | {x.Type} | {x.Category} | {x.Amount:N0} | {x.Note}").ToList();
    }
}
