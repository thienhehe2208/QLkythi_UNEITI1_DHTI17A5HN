// Họ và tên: CHƯA CUNG CẤP — điền người thực hiện thật trước khi nộp.
// Mã sinh viên: CHƯA CUNG CẤP.
// Nội dung thực hiện: Module 3 — dữ liệu cá nhân được phép nhập, không nhận định danh/trạng thái.
using System.ComponentModel.DataAnnotations;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels.Module3;

public class HoSoCaNhanFormViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên."), StringLength(100), Display(Name = "Họ tên")]
    public string HoTen { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập ngày sinh."), DataType(DataType.Date), Display(Name = "Ngày sinh")]
    public DateTime? NgaySinh { get; set; }
    [Required(ErrorMessage = "Vui lòng chọn giới tính."), EnumDataType(typeof(GioiTinh)), Display(Name = "Giới tính")]
    public GioiTinh? GioiTinh { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại."), Phone(ErrorMessage = "Số điện thoại không hợp lệ."), StringLength(20), Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập Email."), EmailAddress(ErrorMessage = "Email không hợp lệ."), StringLength(150), Display(Name = "Email")]
    public string Email { get; set; } = "";
    [StringLength(500), Display(Name = "Địa chỉ")]
    public string? DiaChi { get; set; }
    [StringLength(100), Display(Name = "Trình độ hiện tại")]
    public string? TrinhDoHienTai { get; set; }
    [StringLength(150), Display(Name = "Nghề nghiệp")]
    public string? NgheNghiep { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập số lần thi trước đây."), Range(0, int.MaxValue), Display(Name = "Số lần thi trước đây")]
    public int? SoLanThiTruocDay { get; set; }
    [StringLength(500), Display(Name = "Chứng chỉ đã có")]
    public string? ChungChiDaCo { get; set; }
}
