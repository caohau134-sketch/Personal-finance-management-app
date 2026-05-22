using System;
using PersonalFinanceApp.Models;

namespace PersonalFinanceApp.DTO
{
    /// <summary>
    /// DTO (Data Transfer Object) đại diện cho một giao dịch tài chính (thu hoặc chi).
    ///
    /// MỤC ĐÍCH CỦA DTO:
    ///   Là "gói dữ liệu" truyền giữa tầng giao diện (Program.cs) ↔ tầng DAL.
    ///   Không chứa logic nghiệp vụ phức tạp — chỉ giữ dữ liệu và validate cơ bản.
    ///
    /// KỸ THUẬT SỬ DỤNG:
    ///   [Kế thừa]        → Extends BaseEntity, kế thừa sẵn Id và CreatedDate
    ///   [Field private]  → _amount ẩn bên trong, bên ngoài chỉ truy cập qua Property
    ///   [Property]       → Amount có getter/setter tùy chỉnh để validate dữ liệu
    ///   [Exception]      → Ném ArgumentException khi số tiền âm — ngăn dữ liệu rác vào Storage
    ///   [Auto-property]  → Note, CategoryName, IsIncome dùng { get; set; } đơn giản
    /// </summary>
    public class TransactionDTO : BaseEntity
    {
        // ── FIELD PRIVATE ─────────────────────────────────────────────────────────
        // _amount lưu giá trị thực của số tiền.
        // Đặt private để bên ngoài KHÔNG thể gán trực tiếp — buộc phải qua Property Amount.
        private decimal _amount;

        // ── PROPERTIES ────────────────────────────────────────────────────────────

        /// <summary>
        /// Số tiền của giao dịch (đơn vị: VND).
        /// Getter: trả về _amount.
        /// Setter: kiểm tra không được âm trước khi lưu → nếu âm ném Exception ngay lập tức.
        /// </summary>
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                // XỬ LÝ NGOẠI LỆ: chặn số tiền âm tại điểm nhập vào
                if (value < 0)
                    throw new ArgumentException("Số tiền giao dịch không được âm.");
                _amount = value;
            }
        }

        /// <summary>
        /// Ghi chú tùy chọn của giao dịch. Có thể null/rỗng nếu user bỏ qua.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Tên danh mục của giao dịch (ví dụ: "Ăn uống", "Lương").
        /// Được gán từ kết quả của ChooseCategory() ở Program.cs.
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Phân loại giao dịch:
        ///   true  = Thu nhập (ví dụ: lương, thưởng)
        ///   false = Chi tiêu (ví dụ: ăn uống, đi lại)
        /// Được set bởi lựa chọn [1] Thu / [2] Chi của user — không nhập tay chuỗi ký tự.
        /// </summary>
        public bool IsIncome { get; set; }
    }
}