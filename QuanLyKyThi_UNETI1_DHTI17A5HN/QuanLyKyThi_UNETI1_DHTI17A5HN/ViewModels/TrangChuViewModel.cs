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

        // Số liệu tổng quan dùng chung cho Admin và Cán bộ khảo thí.
        public int TongHoSo { get; set; }
        public int SoHoSoChoDuyet { get; set; }
        public int SoHoSoDuDieuKien { get; set; }
        public int SoHoSoKhongDuDieuKien { get; set; }
        public int SoLichThiSapToi { get; set; }
        public int SoKetQuaDat { get; set; }
        public int SoKetQuaKhongDat { get; set; }
        public List<LichThiSapToiViewModel> LichThiSapToi { get; set; } = new();

        // Nội dung trang chủ dành cho thí sinh.
        public List<NganhNgheTheViewModel> NganhNghes { get; set; } = new();
        public List<KyThiDangMoItemViewModel> KyThiDangMo { get; set; } = new();

        // Chỉ tính cho Admin
        public int SoTaiKhoan { get; set; }
        public int SoTaiKhoanBiKhoa { get; set; }
    }

    public class LichThiSapToiViewModel
    {
        public int MaLichThi { get; set; }
        public string TenKyThi { get; set; } = string.Empty;
        public string HoTenThiSinh { get; set; } = string.Empty;
        public DateTime ThoiGianBatDau { get; set; }
        public string DiaDiem { get; set; } = string.Empty;
    }
}
