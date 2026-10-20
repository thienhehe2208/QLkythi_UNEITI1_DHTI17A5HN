// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: ViewModel trang chủ - số liệu nhanh theo vai trò.
// ============================================================

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels
{
    public class TrangChuViewModel
    {
        public int SoChungChiHoatDong { get; set; }
        public int SoKyThi { get; set; }

        // Chỉ tính cho Admin
        public int SoTaiKhoan { get; set; }
        public int SoTaiKhoanBiKhoa { get; set; }
    }
}
