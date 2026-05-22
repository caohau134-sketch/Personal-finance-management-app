using System.Linq;
using PersonalFinanceApp.DTO;

namespace PersonalFinanceApp.DAL
{
    /// <summary>
    /// DAL chuyên xử lý giao dịch tài chính (thu/chi).
    ///
    /// MỤC ĐÍCH:
    ///   Triển khai các thao tác CRUD cụ thể cho TransactionDTO,
    ///   đồng thời tự động lưu xuống "transactions.json" sau mỗi thay đổi.
    ///
    /// KỸ THUẬT SỬ DỤNG:
    ///   [Kế thừa]   → Extends BaseDAL<TransactionDTO>, dùng lại SaveToFile/LoadFromFile/Storage
    ///   [Đa hình]   → Override Insert/Update/Delete từ abstract của BaseDAL
    ///   [Exception] → Không ném trực tiếp ở đây; Exception từ TransactionDTO.Amount setter
    ///                 sẽ nổi lên Program.cs để catch và hiển thị thông báo cho user
    /// </summary>
    public class TransactionDAL : BaseDAL<TransactionDTO>
    {
        /// <summary>
        /// Constructor: truyền tên file "transactions.json" lên BaseDAL.
        /// BaseDAL sẽ tự LoadFromFile() → khôi phục dữ liệu offline khi app mở lại.
        /// </summary>
        public TransactionDAL() : base("transactions.json") { }

        /// <summary>
        /// Thêm giao dịch mới vào Storage và lưu xuống file.
        ///
        /// Tạo Id tự động: nếu Storage đang rỗng → Id = 1,
        /// ngược lại → lấy Max Id hiện có + 1 (đảm bảo không trùng lặp).
        /// Storage.Any() dùng LINQ kiểm tra list có phần tử nào không.
        /// </summary>
        public override bool Insert(TransactionDTO item)
        {
            // Tạo Id tự tăng — tránh trùng khi thêm nhiều giao dịch
            item.Id = Storage.Any() ? Storage.Max(t => t.Id) + 1 : 1;

            Storage.Add(item);

            // Ghi xuống file ngay → dữ liệu không mất dù app tắt đột ngột
            SaveToFile();
            return true;
        }

        /// <summary>
        /// Cập nhật giao dịch theo Id.
        ///
        /// FindIndex() trả về vị trí index trong list, -1 nếu không tìm thấy.
        /// Gán Storage[index] = item thay thế toàn bộ object cũ bằng object mới.
        /// </summary>
        public override bool Update(TransactionDTO item)
        {
            int index = Storage.FindIndex(t => t.Id == item.Id);

            // Không tìm thấy Id → trả về false để Program.cs thông báo cho user
            if (index == -1) return false;

            Storage[index] = item; // Thay thế object cũ bằng object đã sửa
            SaveToFile();
            return true;
        }

        /// <summary>
        /// Xóa giao dịch theo Id.
        ///
        /// Find() trả về object đầu tiên khớp điều kiện, null nếu không tìm thấy.
        /// Remove() xóa object đó khỏi Storage.
        /// </summary>
        public override bool Delete(int id)
        {
            var item = Storage.Find(t => t.Id == id);

            if (item == null) return false; // Id không tồn tại

            Storage.Remove(item);
            SaveToFile();
            return true;
        }
    }
}