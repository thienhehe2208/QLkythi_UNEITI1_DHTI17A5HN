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
    [Table("LichThi")]
    public class LichThi
    {
        [Key]
        [Display(Name = "Mã lịch thi")]
        public int MaLichThi { get; set; }

        [Display(Name = "Hồ sơ")]
        public int MaHoSo { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Thời gian bắt đầu")]
        public DateTime ThoiGianBatDau { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Thời gian kết thúc")]
        public DateTime ThoiGianKetThuc { get; set; }

        [Display(Name = "Hình thức thi")]
        public HinhThucThi HinhThucThi { get; set; }

        [Required(ErrorMessage = "Địa điểm hoặc liên kết không được để trống")]
        [StringLength(500)]
        [Display(Name = "Địa điểm / Liên kết")]
        public string DiaDiemHoacLienKet { get; set; } = string.Empty;

        /// <summary>Bổ sung đề xuất (quy ước 4.6): dùng kiểm tra trùng phòng khi thi trực tiếp.</summary>
        [StringLength(100)]
        [Display(Name = "Phòng thi")]
        public string? PhongThi { get; set; }

        [Required(ErrorMessage = "Giám thị không được để trống")]
        [StringLength(100)]
        [Display(Name = "Giám thị")]
        public string GiamThi { get; set; } = string.Empty;

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiLichThi TrangThai { get; set; } = TrangThaiLichThi.DaLenLich;

        // Navigation
        public HoSoDangKyThi? HoSoDangKyThi { get; set; }
    }
}
