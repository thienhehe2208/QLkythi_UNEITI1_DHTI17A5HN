// Họ và tên: Phạm Quang Minh
// Mã sinh viên: 23103100201
// Nội dung thực hiện: ViewModel danh sách, form và chi tiết LichThi theo quy ước nhóm.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels
{
    public class LichThiFormViewModel
    {
        public int MaLichThi { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn hồ sơ đủ điều kiện.")]
        [Display(Name = "Hồ sơ dự thi")]
        public int MaHoSo { get; set; }
        [Required, Display(Name = "Thời gian bắt đầu")]
        public DateTime? ThoiGianBatDau { get; set; }
        [Required, Display(Name = "Thời gian kết thúc")]
        public DateTime? ThoiGianKetThuc { get; set; }
        [EnumDataType(typeof(HinhThucThi)), Display(Name = "Hình thức thi")]
        public HinhThucThi HinhThucThi { get; set; }
        [Required, StringLength(500), Display(Name = "Địa điểm / Liên kết")]
        public string DiaDiemHoacLienKet { get; set; } = "";
        [StringLength(100), Display(Name = "Phòng thi (bắt buộc khi thi trực tiếp)")]
        public string? PhongThi { get; set; }
        [Required, StringLength(100), Display(Name = "Giám thị")]
        public string GiamThi { get; set; } = "";
        [StringLength(1000), Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
        public List<SelectListItem> HoSos { get; set; } = [];
    }

    public class LichThiIndexViewModel
    {
        public List<LichThiDongViewModel> DanhSach { get; set; } = new();
        [Display(Name = "Từ khóa")]
        public string? TuKhoa { get; set; }
        [Display(Name = "Trạng thái")]
        public TrangThaiLichThi? TrangThai { get; set; }
        public PhanTrangViewModel PhanTrang { get; set; } = new();
        public bool CoQuyenQuanLy { get; set; }
    }

    public class LichThiDongViewModel
    {
        public int MaLichThi { get; set; }
        public int MaHoSo { get; set; }
        public int MaThiSinh { get; set; }
        public string HoTenThiSinh { get; set; } = string.Empty;
        public string TenKyThi { get; set; } = string.Empty;
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public string DiaDiemHoacLienKet { get; set; } = string.Empty;
        public string? PhongThi { get; set; }
        public string GiamThi { get; set; } = string.Empty;
        public TrangThaiLichThi TrangThai { get; set; }
    }

    public class LichThiChiTietViewModel : LichThiDongViewModel
    {
        public TrangThaiHoSo TrangThaiHoSo { get; set; }
        public HinhThucThi HinhThucThi { get; set; }
        public string? GhiChu { get; set; }
        public bool CoTheSua { get; set; }
        public bool CoTheHuy { get; set; }
        public bool CoTheHoanThanh { get; set; }
        public bool CoTheNhapKetQua { get; set; }
    }
}
