using System;
using PersonalFinanceApp.Models;

namespace PersonalFinanceApp.DTO
{
    /// <summary>
    /// DTO đại diện cho tài khoản người dùng trong hệ thống.
    ///
    /// MỤC ĐÍCH:
    ///   Lưu thông tin đăng nhập và định danh user. Mỗi giao dịch trong tương lai
    ///   sẽ gắn với một UserId — chuẩn bị nền cho tính năng đồng bộ online sau này.
    ///
    /// KỸ THUẬT SỬ DỤNG:
    ///   [Kế thừa]        → Extends BaseEntity (có sẵn Id, CreatedDate)
    ///   [Field private]  → _username, _password ẩn, truy cập qua Property
    ///   [Property]       → Validate username không rỗng, password đủ dài
    ///   [Exception]      → ArgumentException khi dữ liệu không hợp lệ
    ///   [Đa hình]        → GetSummary() ở mỗi DTO trả về string khác nhau (override sau nếu dùng abstract)
    /// </summary>
    public class UserDTO : BaseEntity
    {
        // ── FIELDS PRIVATE ────────────────────────────────────────────────────────
        // Hai field này ẩn hoàn toàn — bên ngoài chỉ tương tác qua Property bên dưới.
        private string _username;
        private string _passwordHash; // Lưu hash, KHÔNG lưu mật khẩu gốc

        // ── PROPERTIES ────────────────────────────────────────────────────────────

        /// <summary>
        /// Tên đăng nhập. Bắt buộc, không được rỗng, tự động trim khoảng trắng.
        /// </summary>
        public string Username
        {
            get { return _username; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Tên đăng nhập không được để trống.");
                _username = value.Trim();
            }
        }

        /// <summary>
        /// Mật khẩu đã được băm (hash). KHÔNG bao giờ lưu mật khẩu gốc.
        /// Setter kiểm tra không rỗng để tránh lưu hash rỗng vào file.
        /// </summary>
        public string PasswordHash
        {
            get { return _passwordHash; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Mật khẩu không hợp lệ.");
                _passwordHash = value;
            }
        }

        /// <summary>
        /// Tên hiển thị của user trong giao diện (có thể khác username).
        /// Ví dụ: Username = "khanh99", DisplayName = "Nguyễn Minh Khánh"
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Trạng thái tài khoản.
        /// true = đang hoạt động, false = đã bị vô hiệu hóa.
        /// Mặc định true khi đăng ký mới.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thời điểm đăng nhập thành công gần nhất.
        /// null nếu chưa từng đăng nhập (tài khoản mới tạo).
        /// </summary>
        public DateTime? LastLoginDate { get; set; } = null;

        // ── HELPER METHOD ─────────────────────────────────────────────────────────

        /// <summary>
        /// Băm mật khẩu gốc bằng thuật toán SHA-256 trước khi lưu.
        /// Gọi method này ở DAL khi đăng ký, KHÔNG gọi trực tiếp từ Program.cs.
        ///
        /// Lý do dùng SHA-256: đơn giản, đủ an toàn cho demo offline.
        /// Thực tế online nên dùng BCrypt hoặc PBKDF2 có salt.
        /// </summary>
        public static string HashPassword(string rawPassword)
        {
            if (string.IsNullOrWhiteSpace(rawPassword) || rawPassword.Length < 6)
                throw new ArgumentException("Mật khẩu phải có ít nhất 6 ký tự.");

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(rawPassword);
                byte[] hash  = sha256.ComputeHash(bytes);
                // Chuyển mảng byte → chuỗi hex 64 ký tự để lưu vào JSON
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Kiểm tra mật khẩu user nhập vào có khớp với hash đã lưu không.
        /// Dùng khi đăng nhập: hash mật khẩu nhập → so sánh với PasswordHash.
        /// </summary>
        public bool VerifyPassword(string rawPassword)
        {
            string inputHash = HashPassword(rawPassword);
            return inputHash == _passwordHash;
        }

        /// <summary>
        /// Trả về chuỗi tóm tắt thông tin user để hiển thị trên console.
        /// </summary>
        public string GetSummary()
        {
            string lastLogin = LastLoginDate.HasValue
                ? LastLoginDate.Value.ToString("dd/MM/yyyy HH:mm")
                : "Chưa đăng nhập";
            string status = IsActive ? "Hoạt động" : "Vô hiệu";
            return $"[{Id}] {Username} ({DisplayName}) | {status} | Đăng nhập lần cuối: {lastLogin}";
        }
    }
}