using System;
using PersonalFinanceApp.DTO;
using PersonalFinanceApp.DAL;

namespace PersonalFinanceApp
{
    class Program
    {
        static TransactionDAL transDAL = new TransactionDAL();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ỨNG DỤNG QUẢN LÝ TÀI CHÍNH OFFLINE ===");
                Console.WriteLine("1. Thêm giao dịch mới");
                Console.WriteLine("2. Xem danh sách giao dịch");
                Console.WriteLine("3. Sửa giao dịch");
                Console.WriteLine("4. Xóa giao dịch");
                Console.WriteLine("0. Thoát");
                Console.Write("Chọn chức năng: ");
                
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1": AddTransaction(); break;
                    case "2": ShowTransactions(); break;
                    case "3": UpdateTransaction(); break;
                    case "4": DeleteTransaction(); break;
                    case "0": Environment.Exit(0); break;
                    default: Console.WriteLine("Lựa chọn không hợp lệ!"); break;
                }
                
                Console.WriteLine("\nNhấn Enter để tiếp tục...");
                Console.ReadLine();
            }
        }

        static string ChooseCategory(bool isIncome)
        {
            string[] defaultCategories = isIncome 
                ? new string[] { "Tiền lương", "Tiền thưởng", "Thu nhập kinh doanh", "Tiền lãi đầu tư" }
                : new string[] { "Ăn uống", "Đi lại (Xăng/Xe)", "Mua sắm", "Hóa đơn (Điện/Nước)", "Giải trí" };

            while (true)
            {
                Console.WriteLine("\n-- CHỌN DANH MỤC --");
                for (int i = 0; i < defaultCategories.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {defaultCategories[i]}");
                }
                
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
                    else
                    {
                        return defaultCategories[index - 1];
                    }
                }
                Console.WriteLine("-> Lựa chọn không hợp lệ, vui lòng chọn số trong danh sách!");
            }
        }

        static void AddTransaction()
        {
            Console.WriteLine("\n--- THÊM GIAO DỊCH ---");
            try
            {
                bool isIncome;
                while (true)
                {
                    Console.Write("Loại giao dịch (1 - Thu, 2 - Chi): ");
                    string typeInput = Console.ReadLine();
                    if (typeInput == "1") { isIncome = true; break; }
                    if (typeInput == "2") { isIncome = false; break; }
                    Console.WriteLine("Vui lòng chỉ nhập số 1 hoặc 2.");
                }

                Console.Write("Nhập số tiền: ");
                decimal amount = decimal.Parse(Console.ReadLine());

                string category = ChooseCategory(isIncome);

                Console.Write("Ghi chú (tùy chọn, ấn Enter để bỏ qua): ");
                string note = Console.ReadLine();

                Console.Write("Nhập ngày (dd/MM/yyyy) hoặc ấn Enter để lấy ngày hôm nay: ");
                string dateInput = Console.ReadLine();
                DateTime date = string.IsNullOrWhiteSpace(dateInput) 
                    ? DateTime.Now 
                    : DateTime.ParseExact(dateInput, "dd/MM/yyyy", null);

                var newTrans = new TransactionDTO
                {
                    IsIncome = isIncome,
                    Amount = amount,
                    CategoryName = category,
                    Note = note,
                    CreatedDate = date
                };

                transDAL.Insert(newTrans);
                Console.WriteLine("-> Thêm thành công và đã lưu offline!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"-> Lỗi nhập liệu: {ex.Message}");
            }
        }

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
                Console.WriteLine(t.GetSummary());
                if (t.IsIncome) tongThu += t.Amount;
                else tongChi += t.Amount;
            }
            
            Console.WriteLine("---------------------------");
            Console.WriteLine($"TỔNG THU: {tongThu:#,##0} VND | TỔNG CHI: {tongChi:#,##0} VND");
            Console.WriteLine($"SỐ DƯ LẠI: {tongThu - tongChi:#,##0} VND");
        }

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
    }
}
