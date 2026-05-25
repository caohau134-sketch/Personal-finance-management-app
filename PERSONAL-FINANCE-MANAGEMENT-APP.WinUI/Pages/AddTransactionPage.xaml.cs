using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PersonalFinanceApp.BLL;
using PersonalFinanceApp.DTO;
using PersonalFinanceWinUI.App.Services;

namespace PersonalFinanceWinUI.App.Pages;

public sealed partial class AddTransactionPage : Page
{
    private readonly TransactionBLL _transactionBLL = new();

    private readonly Dictionary<string, List<string>> _expenseCategories = new()
    {
        ["Sinh hoạt"] = new() { "Ăn uống", "Tiền nhà", "Điện", "Nước", "Internet" },
        ["Di chuyển"] = new() { "Xăng xe", "Gửi xe", "Taxi/Grab", "Bảo dưỡng xe" },
        ["Học tập"] = new() { "Học phí", "Sách vở", "Khóa học online" },
        ["Sức khỏe"] = new() { "Khám bệnh", "Thuốc", "Bảo hiểm" },
        ["Giải trí"] = new() { "Du lịch", "Xem phim", "Cà phê", "Mua sắm" }
    };

    private readonly Dictionary<string, List<string>> _incomeCategories = new()
    {
        ["Lương thưởng"] = new() { "Lương tháng", "Thưởng", "Tăng ca" },
        ["Kinh doanh"] = new() { "Bán hàng", "Dịch vụ", "Hoa hồng" },
        ["Đầu tư"] = new() { "Cổ tức", "Lãi tiết kiệm", "Lợi nhuận đầu tư" },
        ["Khác"] = new() { "Được cho/tặng", "Hoàn tiền", "Thu nhập khác" }
    };

    public AddTransactionPage()
    {
        this.InitializeComponent();
        this.Loaded += (_, _) => LoadCategoryGroups();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        var user = AppState.CurrentUser;
        if (user is null)
        {
            ResultText.Text = "Bạn cần đăng nhập trước.";
            return;
        }

        if (!double.TryParse(AmountBox.Text, out var amount) || amount <= 0)
        {
            ResultText.Text = "Số tiền không hợp lệ.";
            return;
        }

        var selectedCategory = CategoryDetailBox.SelectedItem?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(selectedCategory))
        {
            ResultText.Text = "Vui lòng chọn danh mục chi tiết.";
            return;
        }

        var tx = new TransactionDTO
        {
            UserId = user.Id,
            IsIncome = ((TypeBox.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Thu nhập"),
            Amount = amount,
            CategoryName = selectedCategory,
            Note = NoteBox.Text,
            Source = SourceBox.Text,
            IsEssential = EssentialBox.IsChecked == true,
            Date = DateTime.Now
        };

        await _transactionBLL.AddTransaction(tx);
        ResultText.Text = AppState.IsCloudConfigured ? "Đã lưu giao dịch (Cloud + Local)." : "Đã lưu giao dịch (Local).";
        AmountBox.Text = string.Empty;
        NoteBox.Text = string.Empty;
        SourceBox.Text = string.Empty;
        CategoryDetailBox.SelectedIndex = -1;
    }

    private void CategoryGroupBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadCategoryDetails();
    }

    private void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadCategoryGroups();
    }

    private void LoadCategoryGroups()
    {
        if (CategoryGroupBox is null || CategoryDetailBox is null || TypeBox is null) return;
        var selectedType = (TypeBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Chi tiêu";
        var source = selectedType == "Thu nhập" ? _incomeCategories : _expenseCategories;
        CategoryGroupBox.ItemsSource = source.Keys.ToList();
        if (source.Keys.Any())
        {
            CategoryGroupBox.SelectedIndex = 0;
        }
    }

    private void LoadCategoryDetails()
    {
        if (CategoryGroupBox is null || CategoryDetailBox is null || TypeBox is null) return;
        var selectedType = (TypeBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Chi tiêu";
        var source = selectedType == "Thu nhập" ? _incomeCategories : _expenseCategories;
        var group = CategoryGroupBox.SelectedItem?.ToString() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(group) && source.TryGetValue(group, out var details))
        {
            CategoryDetailBox.ItemsSource = details;
            CategoryDetailBox.SelectedIndex = 0;
        }
        else
        {
            CategoryDetailBox.ItemsSource = null;
        }
    }
}
