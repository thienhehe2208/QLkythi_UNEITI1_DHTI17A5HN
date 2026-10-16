
using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Models
{
    // ============================================================
    // Họ và tên: Nguyễn Tiến Quang
    // Module 2: Quản lý kỳ thi
    // Nội dung: Entity KyThi và kiểm tra dữ liệu đầu vào
    // ============================================================

    public class KyThi
    {
        [Key]
        public int MaKyThi { get; set; }

        [Required(ErrorMessage = "Tên kỳ thi không được để trống.")]
        [StringLength(200, ErrorMessage = "Tên kỳ thi không được vượt quá 200 ký tự.")]
        [Display(Name = "Tên kỳ thi")]
        public string TenKyThi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn chứng chỉ.")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn chứng chỉ hợp lệ.")]
        [Display(Name = "Chứng chỉ")]
        public int MaChungChi { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số chỗ thi tối đa phải lớn hơn 0.")]
        [Display(Name = "Số chỗ thi tối đa")]
        public int SoChoThiToiDa { get; set; }

        [StringLength(100)]
        [Display(Name = "Cấp độ thi")]
        public string? CapDoThi { get; set; }

        [Range(typeof(decimal), "0", "999999999",
            ErrorMessage = "Điểm đầu vào tối thiểu phải lớn hơn hoặc bằng 0.")]
        [Display(Name = "Điểm đầu vào tối thiểu")]
        public decimal DiemDauVaoToiThieu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày bắt đầu đăng ký.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu đăng ký")]
        public DateTime NgayBatDauDangKy { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập hạn đăng ký.")]
        [DataType(DataType.Date)]
        [Display(Name = "Hạn đăng ký")]
        public DateTime HanDangKy { get; set; }

        [StringLength(2000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả kỳ thi")]
        public string? MoTaKyThi { get; set; }

        [StringLength(2000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Điều kiện dự thi")]
        public string? DieuKienDuThi { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Chưa mở";

        // Kiểm tra các quy tắc nghiệp vụ trước khi lưu.
        public bool DuLieuHopLe()
        {
            return
                !string.IsNullOrWhiteSpace(TenKyThi)
                && MaChungChi > 0
                && SoChoThiToiDa > 0
                && DiemDauVaoToiThieu >= 0
                && HanDangKy.Date >= NgayBatDauDangKy.Date;
        }

        // Chỉ cho phép thí sinh đăng ký khi kỳ thi đang mở
        // và ngày hiện tại nằm trong thời gian nhận hồ sơ.
        public bool DangNhanDangKy(DateTime ngayHienTai)
        {
            return TrangThai == "Đang mở đăng ký"
                && ngayHienTai.Date >= NgayBatDauDangKy.Date
                && ngayHienTai.Date <= HanDangKy.Date;
        }
    }
}
