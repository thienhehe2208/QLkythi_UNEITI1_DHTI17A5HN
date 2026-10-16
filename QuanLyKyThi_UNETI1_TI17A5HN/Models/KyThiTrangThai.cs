
namespace QuanLyKyThi_UNETI1_TI17A5HN.Models
{
    // ============================================================
    // Họ và tên: Nguyễn Tiến Quang
    // Module 2: Quản lý kỳ thi
    // Nội dung: Danh sách trạng thái kỳ thi được phép sử dụng
    // ============================================================

    public static class KyThiTrangThai
    {
        public const string ChuaMo = "Chưa mở";
        public const string DangMoDangKy = "Đang mở đăng ký";
        public const string TamDung = "Tạm dừng";
        public const string DaDong = "Đã đóng";

        public static readonly string[] TatCa =
        {
            ChuaMo,
            DangMoDangKy,
            TamDung,
            DaDong
        };
    }
}
