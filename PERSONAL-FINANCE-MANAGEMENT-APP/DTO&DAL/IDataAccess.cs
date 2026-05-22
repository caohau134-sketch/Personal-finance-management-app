using System.Collections.Generic;

namespace PersonalFinanceApp.DAL
{
    /// <summary>
    /// Interface định nghĩa "hợp đồng" (contract) các thao tác truy cập dữ liệu cơ bản.
    ///
    /// MỤC ĐÍCH:
    ///   Bất kỳ DAL nào (TransactionDAL, UserDAL...) đều phải thực hiện đủ 4 thao tác này.
    ///   Nhờ interface, tầng trên (Service/Program) có thể gọi các method mà không cần biết
    ///   bên dưới lưu vào file JSON hay database thật — đây là nguyên tắc "Dependency Inversion".
    ///
    /// KỸ THUẬT SỬ DỤNG:
    ///   [Interface]   → Chỉ khai báo chữ ký method, không có phần thân (body)
    ///   [Generic <T>] → T là kiểu dữ liệu bất kỳ (TransactionDTO, UserDTO...),
    ///                   giúp tái sử dụng interface cho nhiều loại entity khác nhau
    /// </summary>
    public interface IDataAccess<T>
    {
        /// <summary>Thêm một bản ghi mới vào storage. Trả về true nếu thành công.</summary>
        bool Insert(T item);

        /// <summary>Cập nhật bản ghi đã tồn tại (tìm theo Id). Trả về true nếu tìm thấy và cập nhật.</summary>
        bool Update(T item);

        /// <summary>Xóa bản ghi theo Id. Trả về true nếu tìm thấy và xóa thành công.</summary>
        bool Delete(int id);

        /// <summary>Lấy toàn bộ danh sách bản ghi hiện có.</summary>
        List<T> GetAll();
    }
}