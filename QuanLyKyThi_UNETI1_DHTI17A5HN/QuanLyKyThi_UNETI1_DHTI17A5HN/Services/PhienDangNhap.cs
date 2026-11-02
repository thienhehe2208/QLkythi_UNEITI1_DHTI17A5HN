// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Quy ước khóa Session và hàm đọc/ghi thông tin đăng nhập
//   (MaTaiKhoan, HoTen, VaiTro) dùng chung cho mọi Controller/View.
// ============================================================

using QuanLyKyThi_UNETI1_TI17A5HN.Models;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Services
{
    public static class PhienDangNhap
    {
        public const string KhoaMaTaiKhoan = "MaTaiKhoan";
        public const string KhoaHoTen = "HoTen";
        public const string KhoaVaiTro = "VaiTro";

        /// <summary>Ghi thông tin tối thiểu vào Session sau khi đăng nhập thành công.</summary>
        public static void GhiDangNhap(this ISession session, TaiKhoan taiKhoan)
        {
            session.SetInt32(KhoaMaTaiKhoan, taiKhoan.MaTaiKhoan);
            session.SetString(KhoaHoTen, taiKhoan.HoTen);
            session.SetString(KhoaVaiTro, taiKhoan.VaiTro.ToString());
        }

        public static int? LayMaTaiKhoan(this ISession session)
            => session.GetInt32(KhoaMaTaiKhoan);

        public static string? LayHoTen(this ISession session)
            => session.GetString(KhoaHoTen);

        public static VaiTro? LayVaiTro(this ISession session)
            => Enum.TryParse<VaiTro>(session.GetString(KhoaVaiTro), out var vaiTro) ? vaiTro : null;

        public static bool DaDangNhap(this ISession session)
            => session.LayMaTaiKhoan().HasValue;

        public static bool LaVaiTro(this ISession session, params VaiTro[] vaiTros)
        {
            var vaiTro = session.LayVaiTro();
            return vaiTro.HasValue && vaiTros.Contains(vaiTro.Value);
        }
    }
}
