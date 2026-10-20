// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Entity ChungChi - quản lý chứng chỉ (Module 1).
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Models
{
    [Table("ChungChi")]
    public class ChungChi
    {
        [Key]
        [Display(Name = "Mã chứng chỉ")]
        public int MaChungChi { get; set; }

        [Required(ErrorMessage = "Tên chứng chỉ không được để trống")]
        [StringLength(150, ErrorMessage = "Tên chứng chỉ tối đa 150 ký tự")]
        [Display(Name = "Tên chứng chỉ")]
        public string TenChungChi { get; set; } = string.Empty;

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [EmailAddress(ErrorMessage = "Email liên hệ không đúng định dạng")]
        [StringLength(150)]
        [Display(Name = "Email liên hệ")]
        public string? EmailLienHe { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiChungChi TrangThai { get; set; } = TrangThaiChungChi.DangHoatDong;

        // Navigation: ChungChi (1) - (n) KyThi
        public ICollection<KyThi> KyThis { get; set; } = new List<KyThi>();
    }
}
