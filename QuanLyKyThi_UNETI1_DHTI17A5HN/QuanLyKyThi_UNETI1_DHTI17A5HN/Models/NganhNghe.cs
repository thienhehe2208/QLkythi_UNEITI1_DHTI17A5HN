// ============================================================
// Nội dung bổ sung: Nhóm ngành nghề dùng để phân loại chứng chỉ
// trên cổng thông tin dành cho thí sinh.
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Models
{
    [Table("NganhNghe")]
    public class NganhNghe
    {
        [Key]
        public int MaNganhNghe { get; set; }

        [Required, StringLength(120)]
        [Display(Name = "Tên ngành nghề")]
        public string TenNganhNghe { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [StringLength(50)]
        [Display(Name = "Biểu tượng")]
        public string? BieuTuong { get; set; }

        [Display(Name = "Đang hiển thị")]
        public bool DangHienThi { get; set; } = true;

        public ICollection<ChungChi> ChungChis { get; set; } = new List<ChungChi>();
    }
}
