// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Hàm hỗ trợ hiển thị dùng chung trên View:
//   màu badge theo trạng thái, chữ cái đầu cho avatar.
// ============================================================

using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Services
{
    public static class GiaoDienHelper
    {
        /// <summary>
        /// Trả về class màu cho .badge-status: success | info | warning | danger | muted.
        /// Module khác thêm trạng thái của mình vào switch bên dưới.
        /// </summary>
        public static string LopBadge(this Enum trangThai) => trangThai switch
        {
            TrangThaiTaiKhoan.HoatDong => "success",
            TrangThaiTaiKhoan.BiKhoa => "danger",

            TrangThaiChungChi.DangHoatDong => "success",
            TrangThaiChungChi.NgungHoatDong => "muted",

            TrangThaiKyThi.ChuaMo => "muted",
            TrangThaiKyThi.DangMoDangKy => "success",
            TrangThaiKyThi.TamDung => "warning",
            TrangThaiKyThi.DaDong => "danger",

            TrangThaiThiSinh.HoatDong => "success",
            TrangThaiThiSinh.NgungHoatDong => "muted",

            TrangThaiHoSo.ChoDuyet => "warning",
            TrangThaiHoSo.DuDieuKien => "info",
            TrangThaiHoSo.ChoThi => "info",
            TrangThaiHoSo.DaThi => "info",
            TrangThaiHoSo.Dat => "success",
            TrangThaiHoSo.KhongDuDieuKien => "danger",
            TrangThaiHoSo.KhongDat => "danger",
            TrangThaiHoSo.DaHuy => "muted",

            TrangThaiLichThi.DaLenLich => "info",
            TrangThaiLichThi.DaHoanThanh => "success",
            TrangThaiLichThi.DaHuy => "muted",

            KetQuaCuoiCung.Dat => "success",
            KetQuaCuoiCung.KhongDat => "danger",

            _ => "muted"
        };

        /// <summary>Lấy chữ cái đầu của họ và tên (vd. "Nguyễn Đắc Tiến" -> "NT").</summary>
        public static string ChuCaiDau(string? hoTen)
        {
            // Chỉ lấy các từ bắt đầu bằng chữ cái (bỏ số, dấu ngoặc...).
            var tu = (hoTen ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(t => char.IsLetter(t[0]))
                .ToArray();
            if (tu.Length == 0) return "?";
            if (tu.Length == 1) return tu[0][..1].ToUpper();
            return (tu[0][..1] + tu[^1][..1]).ToUpper();
        }

        /// <summary>Icon Bootstrap Icons cho từng vai trò.</summary>
        public static string IconVaiTro(VaiTro vaiTro) => vaiTro switch
        {
            VaiTro.Admin => "bi-shield-lock",
            VaiTro.CanBoKhaoThi => "bi-clipboard-check",
            _ => "bi-person"
        };
    }
}
