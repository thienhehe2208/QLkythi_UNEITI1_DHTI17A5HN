using System.ComponentModel.DataAnnotations;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels
{
    public class ThiSinhDongViewModel
    {
        public int MaThiSinh { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string TenDangNhap { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public TrangThaiThiSinh TrangThai { get; set; }
        public int SoHoSo { get; set; }
    }

    public class ThiSinhIndexViewModel
    {
        public List<ThiSinhDongViewModel> DanhSach { get; set; } = new();
        public string? TuKhoa { get; set; }
        public TrangThaiThiSinh? TrangThai { get; set; }
        public PhanTrangViewModel PhanTrang { get; set; } = new();
    }

    public class ThiSinhFormViewModel
    {
        public int MaThiSinh { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime NgaySinh { get; set; }

        [Display(Name = "Giới tính")]
        public GioiTinh GioiTinh { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(100)]
        [Display(Name = "Trình độ hiện tại")]
        public string? TrinhDoHienTai { get; set; }

        [StringLength(150)]
        [Display(Name = "Nghề nghiệp")]
        public string? NgheNghiep { get; set; }

        [Range(0, 100, ErrorMessage = "Số lần thi trước đây phải từ 0 đến 100")]
        [Display(Name = "Số lần thi trước đây")]
        public int SoLanThiTruocDay { get; set; }

        [StringLength(500)]
        [Display(Name = "Chứng chỉ đã có")]
        public string? ChungChiDaCo { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiThiSinh TrangThai { get; set; } = TrangThaiThiSinh.HoatDong;
    }
}
