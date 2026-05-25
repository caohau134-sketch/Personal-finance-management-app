using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PersonalFinanceApp.DAL;
using PersonalFinanceWinUI.App.DTO;
using PersonalFinanceWinUI.App.Services;

namespace PersonalFinanceWinUI.App.Pages;

public sealed partial class LoginPage : Page
{
    private readonly UserDAL _userDAL;

    public LoginPage()
    {
        this.InitializeComponent();
        _userDAL = new UserDAL();
    }

    private async void Register_Click(object sender, RoutedEventArgs e)
    {
        if (PasswordBox.Password != ConfirmPasswordBox.Password)
        {
            StatusText.Text = "Mat khau xac nhan khong khop.";
            return;
        }

        var role = ((RoleBox.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Admin") ? UserRole.Admin : UserRole.User;
        var result = await _userDAL.Register(NameBox.Text, EmailBox.Text, PasswordBox.Password, role, ParentEmailBox.Text);
        StatusText.Text = result.Message;
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        var result = await _userDAL.Login(EmailBox.Text, PasswordBox.Password);
        if (!result.Success || result.User is null)
        {
            StatusText.Text = result.Message;
            return;
        }

        AppState.CurrentUser = result.User;
        StatusText.Text = "Dang nhap thanh cong.";
        if (App.MainWindowInstance is MainWindow window)
        {
            window.EnableMainNavigation();
        }
    }
}
