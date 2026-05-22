using System;

namespace PersonalFinanceApp.Models
{
    /// <summary>
    /// Abstract class đóng vai trò lớp cha chung cho toàn bộ Entity trong hệ thống.
    ///
    /// MỤC ĐÍCH:
    ///   Tập trung các trường dữ liệu CHUNG mà mọi đối tượng đều có (Id, ngày tạo),
    ///   tránh lặp code ở từng lớp con. Khi cần thêm trường chung (ví dụ: UpdatedDate),
    ///   chỉ cần thêm một chỗ ở đây — tất cả lớp con tự động có theo.
    ///
    /// KỸ THUẬT SỬ DỤNG:
    ///   [Abstract class]  → Không thể "new BaseEntity()" trực tiếp, buộc phải kế thừa
    ///   [Auto-property]   → { get; set; } để C# tự tạo field ẩn bên dưới
    ///   [Initializer]     → "= DateTime.Now" gán giá trị mặc định khi khởi tạo object
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Khóa chính định danh duy nhất mỗi bản ghi.
        /// Được DAL tự gán khi Insert: lấy Max(Id) hiện có + 1.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ngày giờ tạo bản ghi. Mặc định = lúc khởi tạo object.
        /// Có thể ghi đè khi user chọn ngày giao dịch khác ngày hôm nay.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}