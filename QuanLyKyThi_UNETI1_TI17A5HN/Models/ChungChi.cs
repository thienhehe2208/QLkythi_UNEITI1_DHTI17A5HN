// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Quản lý chứng chỉ
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Models
{
    [Table("ChungChi")]
    public class ChungChi
    {
        [Key]
        [Display(Name = "Mã chứng chỉ")]
        public int MaChungChi { get; set; }

        [Required(ErrorMessage = "Tên chứng chỉ không được để trống")]
        [StringLength(150)]
        [Display(Name = "Tên chứng chỉ")]
        public string TenChungChi { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        [DataType(DataType.MultilineText)]
        public string? MoTa { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        [Display(Name = "Email liên hệ")]
        public string? EmailLienHe { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        // Navigation Property
        public ICollection<KyThi>? DanhSachKyThi { get; set; }
    }
}
