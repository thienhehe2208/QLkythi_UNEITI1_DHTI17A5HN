using System.ComponentModel.DataAnnotations;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels
{
    public class HoSoDangKyDongViewModel
    {
        public int MaHoSo { get; set; }
        public string HoTenThiSinh { get; set; } = string.Empty;
        public string TenKyThi { get; set; } = string.Empty;
        public string TenChungChi { get; set; } = string.Empty;
        public DateTime NgayNop { get; set; }
        public TrangThaiHoSo TrangThai { get; set; }
    }

    public class HoSoDangKyIndexViewModel
    {
        public List<HoSoDangKyDongViewModel> DanhSach { get; set; } = new();
        public string? TuKhoa { get; set; }
        public TrangThaiHoSo? TrangThai { get; set; }
        public bool LaThiSinh { get; set; }
        public PhanTrangViewModel PhanTrang { get; set; } = new();
    }

    public class HoSoDangKyFormViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn kỳ thi")]
        [Display(Name = "Kỳ thi")]
        public int MaKyThi { get; set; }

        [Required(ErrorMessage = "Mục đích dự thi không được để trống")]
        [StringLength(1000)]
        [Display(Name = "Mục đích dự thi")]
        public string MucDichDuThi { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }
}
