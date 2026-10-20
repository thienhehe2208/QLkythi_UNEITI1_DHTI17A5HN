// ============================================================
// Họ và tên: Nguyễn Tiến Quang
// Mã sinh viên: Cần bổ sung theo bảng phân công của nhóm
// Nội dung thực hiện: ViewModel quản lý kỳ thi, tìm kiếm, lọc,
//   sắp xếp và phân trang (Module 2).
// ============================================================

using System.ComponentModel.DataAnnotations;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels
{
    public class KyThiDongViewModel
    {
        public int MaKyThi { get; set; }
        public string TenKyThi { get; set; } = string.Empty;
        public string TenChungChi { get; set; } = string.Empty;
        public string CapDoThi { get; set; } = string.Empty;
        public int SoChoThiToiDa { get; set; }
        public decimal DiemDauVaoToiThieu { get; set; }
        public DateTime NgayBatDauDangKy { get; set; }
        public DateTime HanDangKy { get; set; }
        public TrangThaiKyThi TrangThai { get; set; }
        public int SoHoSo { get; set; }
        public bool DangNhanDangKy { get; set; }
    }

    public class KyThiIndexViewModel
    {
        public List<KyThiDongViewModel> DanhSach { get; set; } = new();

        [Display(Name = "Từ khóa")]
        public string? TuKhoa { get; set; }

        [Display(Name = "Chứng chỉ")]
        public int? MaChungChi { get; set; }

        [Display(Name = "Cấp độ")]
        public string? CapDoThi { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiKyThi? TrangThai { get; set; }

        [Range(typeof(decimal), "0", "999.99", ErrorMessage = "Mức điểm phải từ 0 đến 999,99")]
        [Display(Name = "Điểm yêu cầu tối đa")]
        public decimal? DiemToiDa { get; set; }

        [Display(Name = "Thời hạn")]
        public string? ThoiHan { get; set; }

        [Display(Name = "Sắp xếp")]
        public string? SapXep { get; set; }

        public bool LaQuanLy { get; set; }
        public PhanTrangViewModel PhanTrang { get; set; } = new();
    }

    public class KyThiFormViewModel
    {
        public int MaKyThi { get; set; }

        [Required(ErrorMessage = "Tên kỳ thi không được để trống")]
        [StringLength(200)]
        [Display(Name = "Tên kỳ thi")]
        public string TenKyThi { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn chứng chỉ")]
        [Display(Name = "Chứng chỉ")]
        public int MaChungChi { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số chỗ thi tối đa phải lớn hơn 0")]
        [Display(Name = "Số chỗ thi tối đa")]
        public int SoChoThiToiDa { get; set; } = 1;

        [Required(ErrorMessage = "Cấp độ thi không được để trống")]
        [StringLength(50)]
        [Display(Name = "Cấp độ thi")]
        public string CapDoThi { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "999.99", ErrorMessage = "Điểm đầu vào tối thiểu phải từ 0 đến 999,99")]
        [Display(Name = "Điểm đầu vào tối thiểu")]
        public decimal DiemDauVaoToiThieu { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày bắt đầu đăng ký")]
        public DateTime NgayBatDauDangKy { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Hạn đăng ký")]
        public DateTime HanDangKy { get; set; }

        [StringLength(2000)]
        [Display(Name = "Mô tả kỳ thi")]
        public string? MoTaKyThi { get; set; }

        [StringLength(2000)]
        [Display(Name = "Điều kiện dự thi")]
        public string? DieuKienDuThi { get; set; }
    }
}
