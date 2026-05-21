using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using DTO;
using System.Text.RegularExpressions;

    namespace QuanLyTaiChinh.BLL
    {
        public class AccountBLL
        {
            private AccountDAL _dal = new AccountDAL();

            // 1.ĐĂNG NHẬP (Tài khoản, Mật khẩu, SĐT)
            public string Login(string username, string password, string phone)
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(phone))
                    return "Vui lòng nhập đầy đủ thông tin!";

                var account = _dal.GetAccount(username, password, phone);

                if (account != null)
                    return "Success";

                return "Thông tin đăng nhập không chính xác!";
            }

            // 2.ĐĂNG KÝ (TK, MK, Nhập lại MK, SĐT, Gmail)
            public string Register(AccountDTO acc)
            {
                // Kiểm tra trống
                if (string.IsNullOrWhiteSpace(acc.Username) || string.IsNullOrWhiteSpace(acc.Password) ||
                    string.IsNullOrWhiteSpace(acc.PhoneNumber) || string.IsNullOrWhiteSpace(acc.Email))
                {
                    return "Thông tin không được để trống, vui lòng nhập thông tin!";
                }

                // Kiểm tra khớp mật khẩu
                if (acc.Password != acc.ConfirmPassword)
                    return "Mật khẩu nhập lại không khớp, vui lòng kiểm tra lại!";

                // Kiểm tra định dạng Email đơn giản
                if (!acc.Email.Contains("@") || !acc.Email.Contains("."))
                    return "Email không hợp lệ!";

                // Kiểm tra tài khoản đã tồn tại chưa
                if (_dal.IsExist(acc.Username))
                    return "Tài khoản này đã tồn tại!";

                // Lưu dữ liệu
                bool result = _dal.AddAccount(acc);
                return result ? "Success" : "Có lỗi xảy ra khi đăng ký!";
        }
    }


