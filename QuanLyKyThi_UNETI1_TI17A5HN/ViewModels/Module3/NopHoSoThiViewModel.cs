// Họ và tên: CHƯA CUNG CẤP. Mã sinh viên: CHƯA CUNG CẤP.
// Nội dung thực hiện: Module 3 — form nộp hồ sơ và thông tin kỳ thi đọc từ Database.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels.Module3;

public class NopHoSoThiViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn kỳ thi."), Range(1, int.MaxValue), Display(Name = "Kỳ thi")]
    public int? MaKyThi { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập mục đích dự thi."), StringLength(1000), Display(Name = "Mục đích dự thi")]
    public string MucDichDuThi { get; set; } = "";
    [StringLength(1000), Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }
    [BindNever, ValidateNever] public List<SelectListItem> KyThis { get; set; } = [];
    [BindNever, ValidateNever] public HoSoCaNhanViewModel CaNhan { get; set; } = new();
    [BindNever, ValidateNever] public KyThiNopViewModel? KyThi { get; set; }
    [BindNever, ValidateNever] public List<string> LyDoKhongTheNop { get; set; } = [];
}

public class KyThiNopViewModel
{
    public string TenKyThi { get; set; } = "";
    public string TenChungChi { get; set; } = "";
    public string CapDoThi { get; set; } = "";
    public decimal DiemDauVaoToiThieu { get; set; }
    public DateTime NgayBatDauDangKy { get; set; }
    public DateTime HanDangKy { get; set; }
    public string? DieuKienDuThi { get; set; }
    public string? MoTaKyThi { get; set; }
    public bool ConNhanHoSo { get; set; }
}
