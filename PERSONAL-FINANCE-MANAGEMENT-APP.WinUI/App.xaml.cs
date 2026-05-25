using Microsoft.UI.Xaml;
using PersonalFinanceWinUI.App.DAL;

namespace PersonalFinanceWinUI.App;

public partial class App : Application
{
    public static Window? MainWindowInstance { get; private set; }

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        DatabaseInitializer.Initialize();
        MainWindowInstance = new MainWindow();
        MainWindowInstance.Activate();
    }
}
