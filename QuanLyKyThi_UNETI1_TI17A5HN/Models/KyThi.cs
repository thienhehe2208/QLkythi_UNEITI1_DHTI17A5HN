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
    [Table("KyThi")]
    public class KyThi
    {
        [Key]
        [Display(Name = "Mã kỳ thi")]
        public int MaKyThi { get; set; }

        [Required(ErrorMessage = "Tên kỳ thi không được để trống")]
        [StringLength(200)]
        [Display(Name = "Tên kỳ thi")]
        public string TenKyThi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn chứng chỉ")]
        [Display(Name = "Chứng chỉ")]
        public int MaChungChi { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số chỗ thi tối đa phải lớn hơn 0")]
        [Display(Name = "Số chỗ thi tối đa")]
        public int SoChoThiToiDa { get; set; }

        [Required(ErrorMessage = "Cấp độ thi không được để trống")]
        [StringLength(50)]
        [Display(Name = "Cấp độ thi")]
        public string CapDoThi { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "999.99", ErrorMessage = "Điểm đầu vào tối thiểu phải >= 0")]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Điểm đầu vào tối thiểu")]
        public decimal DiemDauVaoToiThieu { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày bắt đầu đăng ký")]
        public DateTime NgayBatDauDangKy { get; set; }

        [DataType(DataType.DateTime)]
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

        [Display(Name = "Trạng thái")]
        public TrangThaiKyThi TrangThai { get; set; } = TrangThaiKyThi.ChuaMo;

        // Navigation
        public ChungChi? ChungChi { get; set; }
        public ICollection<HoSoDangKyThi> HoSoDangKyThis { get; set; } = new List<HoSoDangKyThi>();
    }
}
