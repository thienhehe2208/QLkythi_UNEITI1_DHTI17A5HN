
using System.ComponentModel.DataAnnotations;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Models
{
    // ============================================================
    // Họ và tên: Nguyễn Tiến Quang
    // Module 2: Quản lý kỳ thi
    // Nội dung: Model chứng chỉ phục vụ lựa chọn kỳ thi
    // ============================================================

    public class ChungChi
    {
        [Key]
        public int MaChungChi { get; set; }

        [Required(ErrorMessage = "Tên chứng chỉ không được để trống.")]
        [StringLength(200)]
        [Display(Name = "Tên chứng chỉ")]
        public string TenChungChi { get; set; } = string.Empty;
    }
}
