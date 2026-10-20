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
    [Table("KetQuaThi")]
    public class KetQuaThi
    {
        [Key]
        [Display(Name = "Mã kết quả")]
        public int MaKetQuaThi { get; set; }

        /// <summary>FK unique: mỗi hồ sơ tối đa một kết quả cuối cùng.</summary>
        [Display(Name = "Hồ sơ")]
        public int MaHoSo { get; set; }

        [Range(typeof(decimal), "0", "100", ErrorMessage = "Điểm đánh giá phải trong khoảng 0 - 100")]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Điểm đánh giá")]
        public decimal? DiemDanhGia { get; set; }

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Nhận xét")]
        public string? NhanXet { get; set; }

        [Display(Name = "Kết quả")]
        public KetQuaCuoiCung KetQua { get; set; }

        /// <summary>Server tự gán.</summary>
        [Display(Name = "Ngày cập nhật")]
        public DateTime NgayCapNhat { get; set; }

        // Navigation
        public HoSoDangKyThi? HoSoDangKyThi { get; set; }
    }
}
