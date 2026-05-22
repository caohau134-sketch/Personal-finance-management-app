using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace PersonalFinanceApp.DAL
{
    /// <summary>
    /// Abstract class triển khai phần CHUNG của mọi DAL: đọc/ghi file JSON.
    ///
    /// MỤC ĐÍCH:
    ///   Tập trung logic lưu trữ offline (JSON file) vào một chỗ duy nhất.
    ///   Các lớp con (TransactionDAL, UserDAL) chỉ cần lo phần nghiệp vụ riêng
    ///   (tạo Id, validate đặc thù...) còn đọc/ghi file thì gọi lên lớp cha.
    ///
    /// KỸ THUẬT SỬ DỤNG:
    ///   [Kế thừa + Interface] → BaseDAL<T> implements IDataAccess<T>,
    ///                           lớp con kế thừa BaseDAL và override các method abstract
    ///   [Abstract class]      → Có phần thân (SaveToFile, LoadFromFile, GetAll) khác interface,
    ///                           nhưng Insert/Update/Delete là abstract → bắt buộc lớp con tự làm
    ///   [Generic <T>]         → Một BaseDAL duy nhất dùng được cho mọi loại entity
    ///   [Field protected]     → FilePath, Storage: lớp con truy cập được, bên ngoài không được
    ///   [Đa hình]             → GetAll() có thể bị override ở lớp con nếu cần lọc thêm
    /// </summary>
    public abstract class BaseDAL<T> : IDataAccess<T>
    {
        // ── FIELDS PROTECTED ──────────────────────────────────────────────────────

        /// <summary>
        /// Đường dẫn file JSON lưu trữ dữ liệu offline.
        /// Được lớp con truyền vào qua constructor (ví dụ: "transactions.json").
        /// protected → lớp con đọc được nhưng bên ngoài không truy cập trực tiếp.
        /// </summary>
        protected string FilePath;

        /// <summary>
        /// Danh sách dữ liệu đang giữ trong bộ nhớ (RAM).
        /// Được load từ file JSON khi khởi tạo, và sync lại mỗi khi có thay đổi.
        /// protected → lớp con dùng trực tiếp (thêm, xóa, sửa phần tử).
        /// </summary>
        protected List<T> Storage = new List<T>();

        // ── CONSTRUCTOR ───────────────────────────────────────────────────────────

        /// <summary>
        /// Nhận tên file từ lớp con, gán FilePath rồi load dữ liệu từ file ngay lập tức.
        /// Nhờ vậy khi app khởi động, dữ liệu offline luôn được phục hồi tự động.
        /// </summary>
        public BaseDAL(string fileName)
        {
            FilePath = fileName;
            LoadFromFile(); // Đọc dữ liệu đã lưu trước đó vào Storage
        }

        // ── METHODS PROTECTED (dùng nội bộ trong DAL) ────────────────────────────

        /// <summary>
        /// Ghi toàn bộ Storage ra file JSON.
        /// Gọi sau mỗi Insert / Update / Delete để đảm bảo dữ liệu không mất khi tắt app.
        /// WriteIndented = true → JSON dễ đọc (có xuống dòng, thụt đầu dòng).
        /// </summary>
        protected void SaveToFile()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(Storage, options);
            File.WriteAllText(FilePath, json);
        }

        /// <summary>
        /// Đọc dữ liệu từ file JSON vào Storage khi app khởi động.
        /// Nếu file chưa tồn tại (lần đầu chạy) → Storage = danh sách rỗng, không lỗi.
        /// Deserialize<List<T>> → chuyển chuỗi JSON ngược lại thành List object.
        /// </summary>
        protected void LoadFromFile()
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                Storage = JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }
        }

        // ── ABSTRACT METHODS (lớp con BẮT BUỘC phải override) ────────────────────

        /// <summary>Lớp con tự xử lý: tạo Id, validate riêng, rồi gọi SaveToFile().</summary>
        public abstract bool Insert(T item);

        /// <summary>Lớp con tìm theo Id trong Storage, cập nhật, rồi gọi SaveToFile().</summary>
        public abstract bool Update(T item);

        /// <summary>Lớp con tìm theo Id, xóa khỏi Storage, rồi gọi SaveToFile().</summary>
        public abstract bool Delete(int id);

        // ── VIRTUAL METHOD (lớp con CÓ THỂ override nếu cần) ─────────────────────

        /// <summary>
        /// Trả về bản sao của Storage (ToList() tạo list mới, tránh bên ngoài sửa trực tiếp).
        /// virtual → lớp con có thể override để trả về danh sách đã lọc nếu cần.
        /// </summary>
        public virtual List<T> GetAll() => Storage.ToList();
    }
}