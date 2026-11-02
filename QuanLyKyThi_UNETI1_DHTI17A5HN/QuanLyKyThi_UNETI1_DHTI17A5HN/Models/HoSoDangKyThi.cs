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
    [Table("HoSoDangKyThi")]
    public class HoSoDangKyThi
    {
        [Key]
        [Display(Name = "Mã hồ sơ")]
        public int MaHoSo { get; set; }

        [Display(Name = "Thí sinh")]
        public int MaThiSinh { get; set; }

        [Display(Name = "Kỳ thi")]
        public int MaKyThi { get; set; }

        /// <summary>Server tự gán khi nộp, không nhận từ form.</summary>
        [Display(Name = "Ngày nộp")]
        public DateTime NgayNop { get; set; }

        [Required(ErrorMessage = "Mục đích dự thi không được để trống")]
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Mục đích dự thi")]
        public string MucDichDuThi { get; set; } = string.Empty;

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiHoSo TrangThai { get; set; } = TrangThaiHoSo.ChoDuyet;

        [Display(Name = "Ngày xử lý")]
        public DateTime? NgayXuLy { get; set; }

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Nhận xét xét duyệt")]
        public string? NhanXetXetDuyet { get; set; }

        /// <summary>Bổ sung đề xuất (quy ước 4.5): tài khoản cán bộ xử lý hồ sơ.</summary>
        [Display(Name = "Người xử lý")]
        public int? MaNguoiXuLy { get; set; }

        // Navigation
        public ThiSinh? ThiSinh { get; set; }
        public KyThi? KyThi { get; set; }
        public TaiKhoan? NguoiXuLy { get; set; }
        public ICollection<LichThi> LichThis { get; set; } = new List<LichThi>();
        public KetQuaThi? KetQuaThi { get; set; }
    }
}
