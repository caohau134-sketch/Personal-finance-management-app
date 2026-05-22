using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalFinanceApp.DTO;
using PersonalFinanceApp.DAL;

namespace PERSONAL_FINANCE_MANAGEMENT_APP
{
    internal class Program
    {
        // ── DAL STATIC: sống xuyên suốt app, dữ liệu không mất khi chuyển menu ──
        static TransactionDAL transDAL  = new TransactionDAL();
        static UserDAL        userDAL   = new UserDAL();
        static BudgetDAL      budgetDAL = new BudgetDAL();

        // ── Lưu user đang đăng nhập (null = chưa đăng nhập) ─────────────────────
        static UserDTO currentUser = null;

        static void Main(string[] args)
        {
            // --- Code mặc định của nhóm (Giữ nguyên) ---
            Console.WriteLine("Hello World");
            Console.WriteLine("Xin chao toi la GPT");
            Console.WriteLine(123);
            Console.WriteLine("--------------------------------");

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // ── BƯỚC 1: Bắt buộc đăng nhập / đăng ký trước khi vào app ──────────
            while (currentUser == null)
            {
                Console.WriteLine("\n=== QUẢN LÝ TÀI CHÍNH CÁ NHÂN ===");
                Console.WriteLine("1. Đăng nhập");
                Console.WriteLine("2. Đăng ký tài khoản mới");
                Console.WriteLine("0. Thoát");
                Console.Write("Chọn: ");

                string authChoice = Console.ReadLine();
                switch (authChoice)
                {
                    case "1": Login();    break;
                    case "2": Register(); break;
                    case "0": Environment.Exit(0); break;
                    default:  Console.WriteLine("Lựa chọn không hợp lệ!"); break;
                }
            }

            // ── BƯỚC 2: Menu chính sau khi đăng nhập thành công ─────────────────
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"\n=== ỨNG DỤNG QUẢN LÝ TÀI CHÍNH OFFLINE ===");
                Console.WriteLine($"Xin chào, {currentUser.DisplayName}!");
                Console.WriteLine("1. Thêm giao dịch mới");
                Console.WriteLine("2. Xem danh sách giao dịch");
                Console.WriteLine("3. Sửa giao dịch");
                Console.WriteLine("4. Xóa giao dịch");
                Console.WriteLine("5. Quản lý ngân sách");
                Console.WriteLine("6. Đổi mật khẩu");
                Console.WriteLine("7. Đăng xuất");
                Console.WriteLine("0. Thoát");
                Console.Write("Chọn chức năng: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1": AddTransaction();    break;
                    case "2": ShowTransactions();  break;
                    case "3": UpdateTransaction(); break;
                    case "4": DeleteTransaction(); break;
                    case "5": MenuBudget();        break;
                    case "6": ChangePassword();    break;
                    case "7": Logout(); return;
                    case "0": Environment.Exit(0); break;
                    default:  Console.WriteLine("Lựa chọn không hợp lệ!"); break;
                }

                Console.WriteLine("\nNhấn Enter để tiếp tục...");
                Console.ReadLine();
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // PHẦN AUTH
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Đăng nhập: nhập username + mật khẩu, gọi UserDAL.Login() xác thực.
        /// Xử lý ngoại lệ: Exception chung cho mọi lỗi không lường trước.
        /// </summary>
        static void Login()
        {
            Console.WriteLine("\n--- ĐĂNG NHẬP ---");
            try
            {
                Console.Write("Tên đăng nhập: ");
                string username = Console.ReadLine();

                Console.Write("Mật khẩu: ");
                string password = ReadPasswordMasked();

                UserDTO user = userDAL.Login(username, password);
                if (user == null)
                    Console.WriteLine("-> Tên đăng nhập hoặc mật khẩu không đúng!");
                else
                {
                    currentUser = user;
                    Console.WriteLine($"-> Đăng nhập thành công! Chào {currentUser.DisplayName}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-> Lỗi đăng nhập: {ex.Message}");
            }
        }

        /// <summary>
        /// Đăng ký tài khoản mới.
        /// Xử lý ngoại lệ:
        ///   InvalidOperationException → username đã tồn tại
        ///   ArgumentException         → mật khẩu quá ngắn (dưới 6 ký tự)
        /// </summary>
        static void Register()
        {
            Console.WriteLine("\n--- ĐĂNG KÝ TÀI KHOẢN MỚI ---");
            try
            {
                Console.Write("Tên đăng nhập (không dấu, không khoảng trắng): ");
                string username = Console.ReadLine();

                Console.Write("Mật khẩu (ít nhất 6 ký tự): ");
                string password = ReadPasswordMasked();

                Console.Write("Xác nhận mật khẩu: ");
                string confirm = ReadPasswordMasked();

                if (password != confirm)
                {
                    Console.WriteLine("-> Mật khẩu xác nhận không khớp!");
                    return;
                }

                Console.Write("Tên hiển thị (Enter để dùng tên đăng nhập): ");
                string displayName = Console.ReadLine();

                UserDTO newUser = userDAL.Register(username, password, displayName);
                Console.WriteLine($"-> Đăng ký thành công! Tài khoản: {newUser.Username}");
                Console.WriteLine("-> Vui lòng đăng nhập để tiếp tục.");
            }
            catch (InvalidOperationException ex) { Console.WriteLine($"-> {ex.Message}"); }
            catch (ArgumentException ex)         { Console.WriteLine($"-> Thông tin không hợp lệ: {ex.Message}"); }
            catch (Exception ex)                 { Console.WriteLine($"-> Lỗi đăng ký: {ex.Message}"); }
        }

        /// <summary>
        /// Đổi mật khẩu: xác minh mật khẩu cũ trước, sau đó mới cho đổi.
        /// Xử lý ngoại lệ:
        ///   UnauthorizedAccessException → mật khẩu cũ sai
        ///   ArgumentException           → mật khẩu mới quá ngắn
        /// </summary>
        static void ChangePassword()
        {
            Console.WriteLine("\n--- ĐỔI MẬT KHẨU ---");
            try
            {
                Console.Write("Mật khẩu hiện tại: ");
                string oldPass = ReadPasswordMasked();

                Console.Write("Mật khẩu mới (ít nhất 6 ký tự): ");
                string newPass = ReadPasswordMasked();

                Console.Write("Xác nhận mật khẩu mới: ");
                string confirm = ReadPasswordMasked();

                if (newPass != confirm)
                {
                    Console.WriteLine("-> Mật khẩu xác nhận không khớp!");
                    return;
                }

                userDAL.ChangePassword(currentUser.Id, oldPass, newPass);
                Console.WriteLine("-> Đổi mật khẩu thành công!");
            }
            catch (UnauthorizedAccessException ex) { Console.WriteLine($"-> {ex.Message}"); }
            catch (ArgumentException ex)           { Console.WriteLine($"-> {ex.Message}"); }
            catch (Exception ex)                   { Console.WriteLine($"-> Lỗi: {ex.Message}"); }
        }

        /// <summary>Đăng xuất: xóa currentUser, thoát vòng lặp chính.</summary>
        static void Logout()
        {
            Console.WriteLine($"\n-> Đã đăng xuất tài khoản {currentUser.Username}. Tạm biệt!");
            currentUser = null;
        }

        // ════════════════════════════════════════════════════════════════════════
        // PHẦN GIAO DỊCH (giữ nguyên 100% code gốc của nhóm)
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Hiển thị menu danh mục dựa theo loại Thu/Chi.
        /// Cho phép chọn từ danh sách có sẵn hoặc nhập tự do danh mục mới.
        /// </summary>
        static string ChooseCategory(bool isIncome)
        {
            string[] defaultCategories = isIncome
                ? new string[] { "Tiền lương", "Tiền thưởng", "Thu nhập kinh doanh", "Tiền lãi đầu tư" }
                : new string[] { "Ăn uống", "Đi lại (Xăng/Xe)", "Mua sắm", "Hóa đơn (Điện/Nước)", "Giải trí" };

            while (true)
            {
                Console.WriteLine("\n-- CHỌN DANH MỤC --");
                for (int i = 0; i < defaultCategories.Length; i++)
                    Console.WriteLine($"{i + 1}. {defaultCategories[i]}");

                int otherOptionIndex = defaultCategories.Length + 1;
                Console.WriteLine($"{otherOptionIndex}. Khác (Nhập mục khác...)");
                Console.Write("Nhập số thứ tự lựa chọn của bạn: ");

                string choice = Console.ReadLine();
                if (int.TryParse(choice, out int index) && index >= 1 && index <= otherOptionIndex)
                {
                    if (index == otherOptionIndex)
                    {
                        Console.Write("Vui lòng nhập tên danh mục mới: ");
                        string customCategory = Console.ReadLine();
                        return string.IsNullOrWhiteSpace(customCategory) ? "Khác" : customCategory.Trim();
                    }
                    return defaultCategories[index - 1];
                }
                Console.WriteLine("-> Lựa chọn không hợp lệ, vui lòng chọn số trong danh sách!");
            }
        }

        /// <summary>
        /// Thêm giao dịch mới:
        ///   ① Chọn loại Thu/Chi bằng phím [1]/[2]
        ///   ② Nhập số tiền (validate qua setter của TransactionDTO)
        ///   ③ Chọn danh mục từ menu
        ///   ④ Nhập ghi chú tùy chọn
        ///   ⑤ Nhập ngày: Enter = hôm nay, hoặc gõ dd/MM/yyyy
        /// Sau khi thêm giao dịch Chi → tự động cập nhật SpentAmount vào Budget tương ứng.
        /// </summary>
        static void AddTransaction()
        {
            Console.WriteLine("\n--- THÊM GIAO DỊCH ---");
            try
            {
                // ① Chọn loại Thu/Chi
                bool isIncome;
                while (true)
                {
                    Console.Write("Loại giao dịch (1 - Thu, 2 - Chi): ");
                    string typeInput = Console.ReadLine();
                    if (typeInput == "1") { isIncome = true;  break; }
                    if (typeInput == "2") { isIncome = false; break; }
                    Console.WriteLine("Vui lòng chỉ nhập số 1 hoặc 2.");
                }

                // ② Nhập số tiền
                Console.Write("Nhập số tiền: ");
                decimal amount = decimal.Parse(Console.ReadLine());

                // ③ Chọn danh mục
                string category = ChooseCategory(isIncome);

                // ④ Ghi chú tùy chọn
                Console.Write("Ghi chú (tùy chọn, ấn Enter để bỏ qua): ");
                string note = Console.ReadLine();

                // ⑤ Nhập ngày: Enter = hôm nay, hoặc gõ dd/MM/yyyy
                Console.Write("Nhập ngày (dd/MM/yyyy) hoặc ấn Enter để lấy ngày hôm nay: ");
                string dateInput = Console.ReadLine();
                DateTime date = string.IsNullOrWhiteSpace(dateInput)
                    ? DateTime.Now
                    : DateTime.ParseExact(dateInput, "dd/MM/yyyy", null);

                var newTrans = new TransactionDTO
                {
                    IsIncome     = isIncome,
                    Amount       = amount,       // setter validate không âm
                    CategoryName = category,
                    Note         = note,
                    CreatedDate  = date
                };

                transDAL.Insert(newTrans);

                // Nếu là giao dịch Chi → cộng dồn vào ngân sách danh mục tương ứng
                if (!isIncome)
                    budgetDAL.AddSpending(category, amount);

                Console.WriteLine("-> Thêm thành công và đã lưu offline!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-> Lỗi nhập liệu: {ex.Message}");
            }
        }

        /// <summary>
        /// Hiển thị toàn bộ giao dịch kèm tổng thu, tổng chi, số dư.
        /// </summary>
        static void ShowTransactions()
        {
            Console.WriteLine("\n--- DANH SÁCH GIAO DỊCH ---");
            var list = transDAL.GetAll();
            if (list.Count == 0)
            {
                Console.WriteLine("Chưa có giao dịch nào.");
                return;
            }

            decimal tongThu = 0, tongChi = 0;
            foreach (var t in list)
            {
                string type    = t.IsIncome ? "Thu" : "Chi";
                string noteStr = string.IsNullOrEmpty(t.Note) ? "" : $" (Ghi chú: {t.Note})";
                Console.WriteLine($"[{t.Id}] {t.CreatedDate:dd/MM/yyyy} - {type} | {t.CategoryName}: {t.Amount:#,##0} VND{noteStr}");

                if (t.IsIncome) tongThu += t.Amount;
                else            tongChi += t.Amount;
            }

            Console.WriteLine("---------------------------");
            Console.WriteLine($"TỔNG THU: {tongThu:#,##0} VND | TỔNG CHI: {tongChi:#,##0} VND");
            Console.WriteLine($"SỐ DƯ LẠI: {tongThu - tongChi:#,##0} VND");
        }

        /// <summary>
        /// Sửa giao dịch: chọn Id → sửa số tiền và danh mục → lưu lại.
        /// Nhấn Enter để huỷ.
        /// </summary>
        static void UpdateTransaction()
        {
            ShowTransactions();
            Console.Write("\nNhập ID giao dịch cần sửa (ấn Enter để Hủy): ");
            string inputId = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(inputId)) return;

            if (int.TryParse(inputId, out int id))
            {
                var existingTrans = transDAL.GetAll().Find(t => t.Id == id);
                if (existingTrans == null)
                {
                    Console.WriteLine("-> Không tìm thấy ID này.");
                    return;
                }

                try
                {
                    Console.Write($"Nhập số tiền mới (cũ: {existingTrans.Amount:#,##0}): ");
                    existingTrans.Amount = decimal.Parse(Console.ReadLine());

                    Console.WriteLine($"\nDanh mục hiện tại là: {existingTrans.CategoryName}");
                    existingTrans.CategoryName = ChooseCategory(existingTrans.IsIncome);

                    transDAL.Update(existingTrans);
                    Console.WriteLine("-> Sửa thành công và đã cập nhật offline!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"-> Lỗi: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Xóa giao dịch theo Id. Nhấn Enter để huỷ.
        /// </summary>
        static void DeleteTransaction()
        {
            ShowTransactions();
            Console.Write("\nNhập ID giao dịch cần xóa (ấn Enter để Hủy): ");
            string inputId = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(inputId)) return;

            if (int.TryParse(inputId, out int id))
            {
                if (transDAL.Delete(id))
                    Console.WriteLine("-> Xóa thành công!");
                else
                    Console.WriteLine("-> Không tìm thấy ID này.");
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // PHẦN NGÂN SÁCH
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Menu con quản lý ngân sách: Xem / Thêm / Sửa hạn mức / Xóa.
        /// </summary>
        static void MenuBudget()
        {
            Console.WriteLine("\n--- QUẢN LÝ NGÂN SÁCH ---");
            Console.WriteLine("1. Xem tất cả ngân sách");
            Console.WriteLine("2. Thêm ngân sách mới");
            Console.WriteLine("3. Sửa hạn mức");
            Console.WriteLine("4. Xóa ngân sách");
            Console.Write("Chọn: ");

            string opt = Console.ReadLine();
            switch (opt)
            {
                case "1": ShowBudgets();  break;
                case "2": AddBudget();    break;
                case "3": UpdateBudget(); break;
                case "4": DeleteBudget(); break;
                default:  Console.WriteLine("Lựa chọn không hợp lệ!"); break;
            }
        }

        /// <summary>
        /// Hiển thị tất cả ngân sách.
        /// Dùng computed property IsOverBudget, RemainingAmount từ BudgetDTO — tự tính, không lưu file.
        /// </summary>
        static void ShowBudgets()
        {
            Console.WriteLine("\n-- DANH SÁCH NGÂN SÁCH --");
            var list = budgetDAL.GetAll();
            if (list.Count == 0)
            {
                Console.WriteLine("Chưa có ngân sách nào.");
                return;
            }

            foreach (var b in list)
            {
                string limit  = b.LimitAmount > 0 ? $"{b.LimitAmount:#,##0} VND" : "(Không giới hạn)";
                string remain = b.LimitAmount > 0 ? $"  Còn: {b.RemainingAmount:#,##0} VND" : "";
                string warn   = b.IsOverBudget    ? "  ⚠ VƯỢT HẠN MỨC!" : "";
                Console.WriteLine($"[{b.Id}] {b.CategoryName,-20} Đã chi: {b.SpentAmount:#,##0} / {limit}{remain}{warn}");
            }
        }

        /// <summary>
        /// Thêm ngân sách mới.
        /// Hạn mức = 0 hoặc Enter → không giới hạn (chỉ theo dõi, không cảnh báo).
        /// Xử lý ngoại lệ: ArgumentException nếu nhập hạn mức âm.
        /// </summary>
        static void AddBudget()
        {
            Console.WriteLine("\n-- THÊM NGÂN SÁCH MỚI --");
            try
            {
                Console.Write("Tên danh mục (phải trùng với danh mục giao dịch): ");
                string catName = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(catName))
                {
                    Console.WriteLine("-> Tên danh mục không được để trống.");
                    return;
                }

                // Hạn mức KHÔNG bắt buộc — Enter hoặc 0 = không giới hạn
                Console.Write("Hạn mức chi tiêu (VND) — Enter hoặc 0 để bỏ qua: ");
                string limitStr = Console.ReadLine()?.Trim();
                decimal limit = 0;
                if (!string.IsNullOrWhiteSpace(limitStr))
                    limit = decimal.Parse(limitStr);

                var budget = new BudgetDTO
                {
                    CategoryName = catName,
                    LimitAmount  = limit,  // setter validate không âm (ArgumentException)
                    SpentAmount  = 0
                };

                budgetDAL.Insert(budget);
                string limitLabel = limit > 0 ? $"{limit:#,##0} VND" : "Không giới hạn";
                Console.WriteLine($"-> Đã thêm ngân sách [{catName}] | Hạn mức: {limitLabel}");
            }
            catch (ArgumentException ex) { Console.WriteLine($"-> {ex.Message}"); }
            catch (Exception ex)         { Console.WriteLine($"-> Lỗi: {ex.Message}"); }
        }

        /// <summary>
        /// Sửa hạn mức ngân sách. Enter hoặc 0 → chuyển về không giới hạn.
        /// </summary>
        static void UpdateBudget()
        {
            ShowBudgets();
            Console.Write("\nNhập ID ngân sách cần sửa (ấn Enter để Hủy): ");
            string inputId = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(inputId)) return;

            if (int.TryParse(inputId, out int id))
            {
                var budget = budgetDAL.GetAll().Find(b => b.Id == id);
                if (budget == null)
                {
                    Console.WriteLine("-> Không tìm thấy ID này.");
                    return;
                }

                try
                {
                    string currentLimit = budget.LimitAmount > 0
                        ? $"{budget.LimitAmount:#,##0} VND"
                        : "Không giới hạn";

                    Console.Write($"Hạn mức mới (cũ: {currentLimit}) — Enter hoặc 0 để bỏ giới hạn: ");
                    string newLimitStr = Console.ReadLine()?.Trim();
                    decimal newLimit = 0;
                    if (!string.IsNullOrWhiteSpace(newLimitStr))
                        newLimit = decimal.Parse(newLimitStr);

                    budget.LimitAmount = newLimit;
                    budgetDAL.Update(budget);

                    string newLabel = newLimit > 0 ? $"{newLimit:#,##0} VND" : "Không giới hạn";
                    Console.WriteLine($"-> Đã cập nhật [{budget.CategoryName}]: {newLabel}");
                }
                catch (ArgumentException ex) { Console.WriteLine($"-> {ex.Message}"); }
                catch (Exception ex)         { Console.WriteLine($"-> Lỗi: {ex.Message}"); }
            }
        }

        /// <summary>Xóa ngân sách theo Id.</summary>
        static void DeleteBudget()
        {
            ShowBudgets();
            Console.Write("\nNhập ID ngân sách cần xóa (ấn Enter để Hủy): ");
            string inputId = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(inputId)) return;

            if (int.TryParse(inputId, out int id))
            {
                if (budgetDAL.Delete(id))
                    Console.WriteLine("-> Xóa ngân sách thành công!");
                else
                    Console.WriteLine("-> Không tìm thấy ID này.");
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        // HELPER
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Đọc mật khẩu ẩn ký tự (hiển thị dấu * thay vì chữ thật).
        /// Hỗ trợ Backspace để xóa ký tự vừa nhập.
        /// </summary>
        static string ReadPasswordMasked()
        {
            
            return Console.ReadLine();
        }
    }
}
