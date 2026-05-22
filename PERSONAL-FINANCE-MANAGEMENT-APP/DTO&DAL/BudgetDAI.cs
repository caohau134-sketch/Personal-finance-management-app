using System.Linq;
using PersonalFinanceApp.DTO;

namespace PersonalFinanceApp.DAL
{
    /// <summary>
    /// DAL chuyên xử lý ngân sách theo danh mục.
    ///
    /// MỤC ĐÍCH:
    ///   Lưu trữ và quản lý danh sách ngân sách offline vào "budgets.json".
    ///   Cung cấp thêm method AddSpending() để tự động cộng dồn chi tiêu
    ///   mỗi khi user thêm giao dịch Chi — giữ SpentAmount luôn đồng bộ.
    ///
    /// KỸ THUẬT SỬ DỤNG:
    ///   [Kế thừa]   → Extends BaseDAL<BudgetDTO>, tái sử dụng Storage, SaveToFile, LoadFromFile
    ///   [Đa hình]   → Override Insert/Update/Delete (abstract) từ BaseDAL
    ///   [Exception] → ArgumentException từ BudgetDTO.LimitAmount setter nổi lên Program.cs
    /// </summary>
    public class BudgetDAL : BaseDAL<BudgetDTO>
    {
        /// <summary>
        /// Truyền "budgets.json" lên BaseDAL → tách riêng file lưu ngân sách
        /// khỏi "transactions.json" và "users.json".
        /// BaseDAL tự LoadFromFile() khi khởi tạo → dữ liệu offline được phục hồi.
        /// </summary>
        public BudgetDAL() : base("budgets.json") { }

        // ── OVERRIDE CÁC METHOD BẮT BUỘC ─────────────────────────────────────────

        /// <summary>
        /// Thêm ngân sách mới.
        /// Id tự tăng: Storage rỗng → Id = 1, ngược lại → Max Id hiện có + 1.
        /// Storage.Any() dùng LINQ kiểm tra list có phần tử không.
        /// Storage.Max() dùng LINQ lấy Id lớn nhất hiện có.
        /// </summary>
        public override bool Insert(BudgetDTO item)
        {
            item.Id = Storage.Any() ? Storage.Max(t => t.Id) + 1 : 1;
            Storage.Add(item);
            SaveToFile(); // Lưu ngay → không mất dữ liệu khi tắt app
            return true;
        }

        /// <summary>
        /// Cập nhật ngân sách theo Id (thường dùng khi user sửa hạn mức).
        /// FindIndex() trả về vị trí trong list, -1 nếu không tìm thấy.
        /// Gán Storage[index] = item thay thế toàn bộ object cũ.
        /// </summary>
        public override bool Update(BudgetDTO item)
        {
            int index = Storage.FindIndex(b => b.Id == item.Id);
            if (index == -1) return false;
            Storage[index] = item;
            SaveToFile();
            return true;
        }

        /// <summary>
        /// Xóa ngân sách theo Id.
        /// Find() tìm object đầu tiên khớp điều kiện, null nếu không tìm thấy.
        /// </summary>
        public override bool Delete(int id)
        {
            var item = Storage.Find(b => b.Id == id);
            if (item == null) return false;
            Storage.Remove(item);
            SaveToFile();
            return true;
        }

        // ── METHOD NGHIỆP VỤ RIÊNG ───────────────────────────────────────────────

        /// <summary>
        /// Cộng thêm số tiền vừa chi vào SpentAmount của ngân sách tương ứng.
        /// Được gọi từ AddTransaction() trong Program.cs mỗi khi user thêm giao dịch Chi.
        ///
        /// Tìm ngân sách theo CategoryName (không phân biệt hoa/thường).
        /// Nếu không có ngân sách cho danh mục đó → bỏ qua, không báo lỗi.
        /// </summary>
        public void AddSpending(string categoryName, decimal amount)
        {
            var budget = Storage.Find(b =>
                b.CategoryName.Equals(categoryName, System.StringComparison.OrdinalIgnoreCase));

            if (budget != null)
            {
                budget.SpentAmount += amount;
                SaveToFile(); // Cập nhật SpentAmount mới xuống file ngay
            }
        }
    }
}