// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Băm (hash) và kiểm tra mật khẩu tài khoản.
//   Dùng PasswordHasher (PBKDF2 + salt ngẫu nhiên) có sẵn trong ASP.NET Core,
//   database chỉ lưu chuỗi hash, không lưu mật khẩu thuần.
// ============================================================

using Microsoft.AspNetCore.Identity;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Services
{
    public interface IMatKhauService
    {
        string BamMatKhau(TaiKhoan taiKhoan, string matKhau);

        /// <summary>Trả về kết quả kiểm tra; SuccessRehashNeeded nghĩa là đúng nhưng nên băm lại.</summary>
        PasswordVerificationResult KiemTra(TaiKhoan taiKhoan, string matKhauNhap);
    }

    public class MatKhauService : IMatKhauService
    {
        private readonly PasswordHasher<TaiKhoan> _hasher = new();

        public string BamMatKhau(TaiKhoan taiKhoan, string matKhau)
            => _hasher.HashPassword(taiKhoan, matKhau);

        public PasswordVerificationResult KiemTra(TaiKhoan taiKhoan, string matKhauNhap)
        {
            if (string.IsNullOrEmpty(taiKhoan.MatKhau) || string.IsNullOrEmpty(matKhauNhap))
                return PasswordVerificationResult.Failed;

            try
            {
                return _hasher.VerifyHashedPassword(taiKhoan, taiKhoan.MatKhau, matKhauNhap);
            }
            catch (FormatException)
            {
                // Chuỗi trong DB không phải hash hợp lệ (vd. dữ liệu nhập tay) -> coi như sai.
                return PasswordVerificationResult.Failed;
            }
        }
    }
}
