// Họ và tên: CHƯA CUNG CẤP. Mã sinh viên: CHƯA CUNG CẤP.
// Nội dung thực hiện: Module 3 — danh sách hồ sơ theo chủ sở hữu và phân trang phía máy chủ.
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels.Module3;

public class DanhSachHoSoThiViewModel
{
    public List<HoSoThiDongViewModel> DanhSach { get; set; } = [];
    public PhanTrangViewModel PhanTrang { get; set; } = new();
    public string? TuKhoa { get; set; }
    public TrangThaiHoSo? TrangThai { get; set; }
}

public class HoSoThiDongViewModel
{
    public int MaHoSo { get; set; }
    public string TenKyThi { get; set; } = "";
    public string TenChungChi { get; set; } = "";
    public DateTime NgayNop { get; set; }
    public TrangThaiHoSo TrangThai { get; set; }
    public bool CoTheHuy { get; set; }
}
