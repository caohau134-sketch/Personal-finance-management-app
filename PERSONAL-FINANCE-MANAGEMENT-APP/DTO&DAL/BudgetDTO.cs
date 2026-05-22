using System;
using PersonalFinanceApp.Models;

namespace PersonalFinanceApp.DTO
{
    /// <summary>
    /// DTO đại diện cho một ngân sách theo danh mục chi tiêu.
    ///
    /// MỤC ĐÍCH:
    ///   Giúp user đặt hạn mức chi tiêu cho từng danh mục (ví dụ: Ăn uống tối đa 3 triệu/tháng).
    ///   Tự động tính số tiền còn lại và cảnh báo khi vượt hạn mức qua computed property.
    ///
    /// KỸ THUẬT SỬ DỤNG:
    ///   [Kế thừa]           → Extends BaseEntity, có sẵn Id và CreatedDate
    ///   [Field private]     → _limitAmount ẩn bên trong, truy cập qua Property LimitAmount
    ///   [Property + getter/setter tùy chỉnh] → Validate hạn mức không được âm
    ///   [Computed property] → RemainingAmount, IsOverBudget chỉ có get, tự tính từ 2 field khác
    ///   [Exception]         → ArgumentException khi LimitAmount nhận giá trị âm
    /// </summary>
    public class BudgetDTO : BaseEntity
    {
        // ── FIELD PRIVATE ─────────────────────────────────────────────────────────
        // _limitAmount lưu giá trị thực của hạn mức.
        // private → bên ngoài không gán trực tiếp, phải đi qua Property LimitAmount bên dưới.
        private decimal _limitAmount;

        // ── PROPERTIES ────────────────────────────────────────────────────────────

        /// <summary>
        /// Tên danh mục áp dụng ngân sách này (ví dụ: "Ăn uống", "Đi lại").
        /// Dùng để khớp với CategoryName trong TransactionDTO khi tính SpentAmount.
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Hạn mức chi tiêu tối đa (đơn vị: VND).
        /// Getter: trả về _limitAmount.
        /// Setter: kiểm tra không được âm → nếu âm ném Exception ngay.
        /// Cho phép = 0 (không đặt hạn mức cụ thể, chỉ theo dõi chi tiêu).
        /// </summary>
        public decimal LimitAmount
        {
            get { return _limitAmount; }
            set
            {
                // XỬ LÝ NGOẠI LỆ: hạn mức không được âm, nhưng có thể = 0
                if (value < 0)
                    throw new ArgumentException("Hạn mức ngân sách không được âm.");
                _limitAmount = value;
            }
        }

        /// <summary>
        /// Tổng số tiền đã chi trong danh mục này.
        /// Được cập nhật mỗi khi user thêm giao dịch Chi thuộc danh mục tương ứng.
        /// Auto-property vì không cần validate đặc biệt.
        /// </summary>
        public decimal SpentAmount { get; set; }

        // ── COMPUTED PROPERTIES (chỉ có get, tự tính từ các property khác) ───────

        /// <summary>
        /// Số tiền còn có thể chi thêm = Hạn mức - Đã chi.
        /// Có thể âm nếu đã vượt hạn mức → dùng để hiển thị cảnh báo.
        /// Không có setter vì đây là giá trị tính toán, không lưu trực tiếp.
        /// </summary>
        public decimal RemainingAmount => LimitAmount - SpentAmount;

        /// <summary>
        /// Kiểm tra có vượt hạn mức không.
        /// Chỉ cảnh báo khi LimitAmount > 0 (tức là có đặt hạn mức).
        /// LimitAmount = 0 nghĩa là "chỉ theo dõi, không giới hạn" → không cảnh báo.
        /// </summary>
        public bool IsOverBudget => LimitAmount > 0 && SpentAmount > LimitAmount;
    }
}