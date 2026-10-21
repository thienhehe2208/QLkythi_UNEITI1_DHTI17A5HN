// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Khai báo khung Entity dùng chung theo QUY_UOC_NHOM
//   (trường, kiểu dữ liệu, độ dài, quan hệ). Nghiệp vụ của Entity này
//   do thành viên phụ trách module tương ứng thực hiện.
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Models
{
    [Table("ThiSinh")]
    public class ThiSinh
    {
        [Key]
        [Display(Name = "Mã thí sinh")]
        public int MaThiSinh { get; set; }

        /// <summary>FK tới TaiKhoan, unique: một tài khoản tối đa một hồ sơ thí sinh.</summary>
        [Display(Name = "Tài khoản")]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Column(TypeName = "date")]
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
        [Display(Name = "Email")]
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

        [Range(0, int.MaxValue, ErrorMessage = "Số lần thi trước đây phải >= 0")]
        [Display(Name = "Số lần thi trước đây")]
        public int SoLanThiTruocDay { get; set; }

        [StringLength(500)]
        [Display(Name = "Chứng chỉ đã có")]
        public string? ChungChiDaCo { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiThiSinh TrangThai { get; set; } = TrangThaiThiSinh.HoatDong;

        // Navigation
        public TaiKhoan? TaiKhoan { get; set; }
        public ICollection<HoSoDangKyThi> HoSoDangKyThis { get; set; } = new List<HoSoDangKyThi>();
    }
}
