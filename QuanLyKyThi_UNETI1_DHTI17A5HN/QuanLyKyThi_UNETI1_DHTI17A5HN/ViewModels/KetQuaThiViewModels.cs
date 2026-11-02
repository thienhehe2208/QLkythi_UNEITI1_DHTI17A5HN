// Họ và tên: Phạm Quang Minh
// Mã sinh viên: 23103100201
// Nội dung thực hiện: ViewModel danh sách, form và chi tiết KetQuaThi theo quy ước nhóm.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels
{
    public class KetQuaThiFormViewModel
    {
        public int MaKetQuaThi { get; set; }
        [Range(1, int.MaxValue), Display(Name = "Hồ sơ đã thi")]
        public int MaHoSo { get; set; }
        [Range(typeof(decimal), "0", "100", ErrorMessage = "Điểm phải từ 0 đến 100.")]
        [Display(Name = "Điểm đánh giá (0–100, có thể để trống)")]
        public decimal? DiemDanhGia { get; set; }
        [Required(ErrorMessage = "Cán bộ phải xác nhận kết quả."), EnumDataType(typeof(KetQuaCuoiCung))]
        [Display(Name = "Kết quả xác nhận")]
        public KetQuaCuoiCung? KetQua { get; set; }
        [StringLength(1000), Display(Name = "Nhận xét")]
        public string? NhanXet { get; set; }
        public List<SelectListItem> HoSos { get; set; } = [];
    }

    public class KetQuaThiIndexViewModel
    {
        public List<KetQuaThiDongViewModel> DanhSach { get; set; } = new();
        [Display(Name = "Từ khóa")]
        public string? TuKhoa { get; set; }
        [Display(Name = "Kết quả")]
        public KetQuaCuoiCung? KetQua { get; set; }
        public PhanTrangViewModel PhanTrang { get; set; } = new();
        public bool CoQuyenQuanLy { get; set; }
    }

    public class KetQuaThiDongViewModel
    {
        public int MaKetQuaThi { get; set; }
        public int MaHoSo { get; set; }
        public string HoTenThiSinh { get; set; } = string.Empty;
        public string TenKyThi { get; set; } = string.Empty;
        public decimal? DiemDanhGia { get; set; }
        public KetQuaCuoiCung KetQua { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class KetQuaThiChiTietViewModel : KetQuaThiDongViewModel
    {
        public TrangThaiHoSo TrangThaiHoSo { get; set; }
        public string? NhanXet { get; set; }
        public bool CoTheSua { get; set; }
    }
}

