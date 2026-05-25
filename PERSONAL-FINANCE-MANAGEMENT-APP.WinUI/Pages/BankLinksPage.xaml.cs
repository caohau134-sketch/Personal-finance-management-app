using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PersonalFinanceWinUI.App.BLL;
using PersonalFinanceWinUI.App.DAL;
using PersonalFinanceWinUI.App.DTO;
using PersonalFinanceWinUI.App.Services;

namespace PersonalFinanceWinUI.App.Pages;

public sealed partial class BankLinksPage : Page
{
    private readonly FinanceRepository _repo = new();
    private readonly CloudDataService _cloud = new(AppState.CloudProjectId, AppState.CloudApiKey);

    public BankLinksPage()
    {
        this.InitializeComponent();
        LoadLinks();
    }

    private async void AddLink_Click(object sender, RoutedEventArgs e)
    {
        var user = AppState.CurrentUser;
        if (user is null)
        {
            StatusText.Text = "Ban can dang nhap.";
            return;
        }

        var link = new BankLink
        {
            UserId = user.Id,
            ProviderName = ProviderBox.Text,
            MaskedNumber = MaskBox.Text,
            AccountType = (TypeBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Card"
        };
        _repo.AddBankLink(link);
        await _cloud.AddBankLinkAsync(link);
        StatusText.Text = "Da them lien ket.";
        ProviderBox.Text = string.Empty;
        MaskBox.Text = string.Empty;
        LoadLinks();
    }

    private void LoadLinks()
    {
        var user = AppState.CurrentUser;
        if (user is null) return;
        var links = _repo.GetBankLinks(user.Id);
        LinksList.ItemsSource = links.Select(x => $"{x.ProviderName} - {x.MaskedNumber} - {x.AccountType}").ToList();
    }
}
