using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PersonalFinanceWinUI.App.BLL;
using PersonalFinanceWinUI.App.DAL;
using PersonalFinanceWinUI.App.Services;

namespace PersonalFinanceWinUI.App.Pages;

public sealed partial class CloudSyncPage : Page
{
    private readonly TransactionLogic _txLogic = new();
    private readonly FinanceRepository _repo = new();
    private readonly CloudDataService _cloud = new(AppState.CloudProjectId, AppState.CloudApiKey);

    public CloudSyncPage()
    {
        this.InitializeComponent();
    }

    private async void SyncNow_Click(object sender, RoutedEventArgs e)
    {
        var user = AppState.CurrentUser;
        if (user is null)
        {
            SyncStatusText.Text = "Ban can dang nhap truoc.";
            return;
        }

        if (!AppState.IsCloudConfigured || string.IsNullOrWhiteSpace(AppState.FirebaseIdToken))
        {
            SyncStatusText.Text = "Cloud chua cau hinh hoac chua co token dang nhap Firebase.";
            return;
        }

        try
        {
            SyncStatusText.Text = "Dang dong bo...";
            await _cloud.UpsertUserAsync(user);

            var txs = _txLogic.GetTransactions(user.Id);
            var links = _repo.GetBankLinks(user.Id);

            foreach (var tx in txs)
            {
                await _cloud.AddTransactionAsync(tx);
            }

            foreach (var link in links)
            {
                await _cloud.AddBankLinkAsync(link);
            }

            SyncStatusText.Text = $"Da dong bo: {txs.Count} giao dich, {links.Count} lien ket.";
        }
        catch (Exception ex)
        {
            SyncStatusText.Text = $"Dong bo that bai: {ex.Message}";
        }
    }
}
