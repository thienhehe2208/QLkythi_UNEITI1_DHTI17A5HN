// ============================================================
// Họ và tên: Chu Văn Trọng
// Nội dung thực hiện: Module 3 - quản lý và tra cứu thí sinh.
// ============================================================

using System.ComponentModel.DataAnnotations;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels.Module3;

public class QuanLyThiSinhViewModel
{
    public List<ThiSinhDongViewModel> DanhSach { get; set; } = [];
    public string? TuKhoa { get; set; }
    public TrangThaiThiSinh? TrangThai { get; set; }
    public PhanTrangViewModel PhanTrang { get; set; } = new();
}

public class ThiSinhDongViewModel
{
    public int MaThiSinh { get; set; }
    public string HoTen { get; set; } = "";
    public string TenDangNhap { get; set; } = "";
    public string Email { get; set; } = "";
    public string SoDienThoai { get; set; } = "";
    public TrangThaiThiSinh TrangThai { get; set; }
}

public class ChiTietThiSinhViewModel
{
    public HoSoCaNhanViewModel CaNhan { get; set; } = new();
    public string TenDangNhap { get; set; } = "";
    public List<HoSoThiDongViewModel> HoSos { get; set; } = [];
    public PhanTrangViewModel PhanTrang { get; set; } = new() { Action = "Details" };
    public bool CoQuyenDoiTrangThai { get; set; }
}

public class CapNhatTrangThaiThiSinhViewModel
{
    [Required, EnumDataType(typeof(TrangThaiThiSinh)), Display(Name = "Trạng thái thí sinh")]
    public TrangThaiThiSinh? TrangThai { get; set; }
}
