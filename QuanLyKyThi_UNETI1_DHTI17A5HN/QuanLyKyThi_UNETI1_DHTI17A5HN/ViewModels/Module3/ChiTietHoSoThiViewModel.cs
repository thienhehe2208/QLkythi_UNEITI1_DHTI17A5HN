// ============================================================
// Họ và tên: Chu Văn Trọng
// Nội dung thực hiện: Module 3 - theo dõi hồ sơ, lịch và kết quả.
// ============================================================

using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels.Module3;

public class ChiTietHoSoThiViewModel : HoSoThiDongViewModel
{
    public string HoTenThiSinh { get; set; } = "";
    public string MucDichDuThi { get; set; } = "";
    public string? GhiChu { get; set; }
    public DateTime? NgayXuLy { get; set; }
    public string? NhanXetXetDuyet { get; set; }
    public List<LichTheoDoiViewModel> LichThis { get; set; } = [];
    public KetQuaTheoDoiViewModel? KetQua { get; set; }
}

public class LichTheoDoiViewModel
{
    public int MaLichThi { get; set; }
    public DateTime ThoiGianBatDau { get; set; }
    public DateTime ThoiGianKetThuc { get; set; }
    public HinhThucThi HinhThucThi { get; set; }
    public string DiaDiemHoacLienKet { get; set; } = "";
    public string? PhongThi { get; set; }
    public string GiamThi { get; set; } = "";
    public TrangThaiLichThi TrangThai { get; set; }
}

public class KetQuaTheoDoiViewModel
{
    public int MaKetQuaThi { get; set; }
    public decimal? DiemDanhGia { get; set; }
    public KetQuaCuoiCung KetQua { get; set; }
    public string? NhanXet { get; set; }
    public DateTime NgayCapNhat { get; set; }
}
