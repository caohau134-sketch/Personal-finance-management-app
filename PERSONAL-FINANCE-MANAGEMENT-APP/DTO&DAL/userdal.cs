using System;
using System.Linq;
using PersonalFinanceApp.DTO;
 
namespace PersonalFinanceApp.DAL
{
    /// <summary>
    /// DAL chuyên xử lý tài khoản người dùng.
    ///
    /// MỤC ĐÍCH:
    ///   Quản lý đăng ký, đăng nhập, tìm kiếm user — lưu offline vào "users.json".
    ///   Chuẩn bị nền cho tính năng đồng bộ online sau: mỗi giao dịch sẽ gắn UserId,
    ///   khi online thì upload dữ liệu theo UserId đó lên server.
    ///
    /// KỸ THUẬT SỬ DỤNG:
    ///   [Kế thừa]   → Extends BaseDAL<UserDTO>, tái sử dụng toàn bộ cơ chế JSON
    ///   [Đa hình]   → Override Insert/Update/Delete từ BaseDAL
    ///   [Exception] → Ném InvalidOperationException khi username đã tồn tại,
    ///                 ArgumentException khi mật khẩu quá ngắn (từ UserDTO.HashPassword)
    /// </summary>
    public class UserDAL : BaseDAL<UserDTO>
    {
        /// <summary>
        /// Lưu riêng vào "users.json" — tách biệt hoàn toàn với "transactions.json".
        /// </summary>
        public UserDAL() : base("users.json") { }
 
        // ── OVERRIDE CÁC METHOD BẮT BUỘC ─────────────────────────────────────────
 
        /// <summary>
        /// Đăng ký tài khoản mới.
        ///
        /// LUỒNG XỬ LÝ:
        ///   1. Kiểm tra username đã tồn tại chưa (không phân biệt hoa/thường)
        ///   2. Băm mật khẩu trước khi lưu (gọi UserDTO.HashPassword)
        ///   3. Tạo Id tự tăng, thêm vào Storage, lưu file
        ///
        /// EXCEPTION:
        ///   InvalidOperationException → username đã có người dùng
        ///   ArgumentException         → mật khẩu quá ngắn (< 6 ký tự)
        /// </summary>
        public override bool Insert(UserDTO item)
        {
            // Kiểm tra trùng username (OrdinalIgnoreCase = không phân biệt hoa/thường)
            bool isDuplicate = Storage.Any(u =>
                u.Username.Equals(item.Username, StringComparison.OrdinalIgnoreCase));
 
            if (isDuplicate)
                throw new InvalidOperationException($"Tên đăng nhập '{item.Username}' đã tồn tại.");
 
            // Tạo Id tự tăng
            item.Id = Storage.Any() ? Storage.Max(u => u.Id) + 1 : 1;
 
            // Mật khẩu đã được hash sẵn từ bên ngoài trước khi gọi Insert
            // (Xem Register() bên dưới — nơi duy nhất tạo UserDTO để Insert)
            Storage.Add(item);
            SaveToFile();
            return true;
        }
 
        /// <summary>
        /// Cập nhật thông tin tài khoản (DisplayName, trạng thái...).
        /// Không dùng để đổi mật khẩu — dùng ChangePassword() riêng.
        /// </summary>
        public override bool Update(UserDTO item)
        {
            int index = Storage.FindIndex(u => u.Id == item.Id);
            if (index == -1) return false;
 
            Storage[index] = item;
            SaveToFile();
            return true;
        }
 
        /// <summary>Xóa tài khoản theo Id.</summary>
        public override bool Delete(int id)
        {
            var item = Storage.Find(u => u.Id == id);
            if (item == null) return false;
 
            Storage.Remove(item);
            SaveToFile();
            return true;
        }
 
        // ── METHODS NGHIỆP VỤ RIÊNG CỦA USER ────────────────────────────────────
 
        /// <summary>
        /// Đăng ký tài khoản mới — wrapper gọn gàng cho Program.cs dùng.
        ///
        /// LUỒNG:
        ///   1. HashPassword(rawPassword) → chuỗi hash 64 ký tự
        ///   2. Tạo UserDTO với hash đó
        ///   3. Gọi Insert() để lưu
        ///
        /// Tách method này ra để Program.cs KHÔNG bao giờ tự gán PasswordHash thủ công
        /// → tránh quên hash mật khẩu rồi lưu plaintext.
        /// </summary>
        public UserDTO Register(string username, string rawPassword, string displayName = null)
        {
            // UserDTO.HashPassword ném ArgumentException nếu mật khẩu < 6 ký tự
            string hash = UserDTO.HashPassword(rawPassword);
 
            var newUser = new UserDTO
            {
                Username     = username,     // setter validate không rỗng
                PasswordHash = hash,
                DisplayName  = string.IsNullOrWhiteSpace(displayName) ? username : displayName.Trim(),
                IsActive     = true,
                CreatedDate  = DateTime.Now
            };
 
            Insert(newUser); // Có thể ném InvalidOperationException nếu username trùng
            return newUser;
        }
 
        /// <summary>
        /// Đăng nhập: tìm username rồi verify mật khẩu.
        ///
        /// Trả về UserDTO nếu đúng, null nếu sai username hoặc sai mật khẩu.
        /// Cập nhật LastLoginDate khi đăng nhập thành công.
        ///
        /// Lưu ý bảo mật: không thông báo riêng "sai username" hay "sai mật khẩu"
        /// → tránh kẻ tấn công biết username nào tồn tại.
        /// </summary>
        public UserDTO Login(string username, string rawPassword)
        {
            // Tìm user theo username (không phân biệt hoa/thường)
            var user = Storage.Find(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)
                && u.IsActive); // Chỉ cho đăng nhập nếu tài khoản đang hoạt động
 
            if (user == null) return null; // Không tìm thấy username hoặc bị vô hiệu
 
            // VerifyPassword: hash rawPassword rồi so sánh với PasswordHash đã lưu
            if (!user.VerifyPassword(rawPassword)) return null; // Sai mật khẩu
 
            // Đăng nhập thành công → cập nhật lần đăng nhập cuối
            user.LastLoginDate = DateTime.Now;
            Update(user); // Lưu lại LastLoginDate xuống file
            return user;
        }
 
        /// <summary>
        /// Đổi mật khẩu: xác minh mật khẩu cũ trước khi cho đổi.
        ///
        /// EXCEPTION:
        ///   ArgumentException → mật khẩu mới quá ngắn
        ///   UnauthorizedAccessException → mật khẩu cũ sai
        ///   KeyNotFoundException → không tìm thấy userId
        /// </summary>
        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var user = Storage.Find(u => u.Id == userId);
 
            if (user == null)
                throw new System.Collections.Generic.KeyNotFoundException($"Không tìm thấy tài khoản Id={userId}");
 
            // Xác minh mật khẩu cũ trước
            if (!user.VerifyPassword(oldPassword))
                throw new UnauthorizedAccessException("Mật khẩu cũ không đúng.");
 
            // HashPassword sẽ ném ArgumentException nếu newPassword < 6 ký tự
            user.PasswordHash = UserDTO.HashPassword(newPassword);
            Update(user);
            return true;
        }
 
        /// <summary>
        /// Tìm user theo username. Dùng để kiểm tra tồn tại hoặc lấy thông tin.
        /// Trả về null nếu không tìm thấy.
        /// </summary>
        public UserDTO FindByUsername(string username)
        {
            return Storage.Find(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
    }
}