using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PersonalFinanceWinUI.App.Pages;
using PersonalFinanceWinUI.App.Services;

namespace PersonalFinanceWinUI.App;

public sealed partial class MainWindow : Window
{
    private readonly List<NavigationViewItem> _menuItems = new();

    public MainWindow()
    {
        this.InitializeComponent();
        _menuItems.AddRange(AppNavView.MenuItems.OfType<NavigationViewItem>());
        SetMenuEnabled(false);
        ContentFrame.Navigate(typeof(LoginPage));
    }

    public void EnableMainNavigation()
    {
        SetMenuEnabled(true);
        ContentFrame.Navigate(typeof(DashboardPage));
    }

    private void SetMenuEnabled(bool enabled)
    {
        foreach (var item in _menuItems)
        {
            item.IsEnabled = enabled;
        }
    }

    private void AppNavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (AppState.CurrentUser is null) return;
        if (args.SelectedItemContainer is not NavigationViewItem item) return;

        switch (item.Tag?.ToString())
        {
            case "dashboard":
                ContentFrame.Navigate(typeof(DashboardPage));
                break;
            case "add":
                ContentFrame.Navigate(typeof(AddTransactionPage));
                break;
            case "reports":
                ContentFrame.Navigate(typeof(ReportsPage));
                break;
            case "bank":
                ContentFrame.Navigate(typeof(BankLinksPage));
                break;
            case "sync":
                ContentFrame.Navigate(typeof(CloudSyncPage));
                break;
            case "logout":
                AppState.ClearSession();
                SetMenuEnabled(false);
                ContentFrame.Navigate(typeof(LoginPage));
                break;
        }
    }
}
