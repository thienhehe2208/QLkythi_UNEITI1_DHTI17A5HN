using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using System.ComponentModel.DataAnnotations;
namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels
{
    public class XetDuyetHoSoDongViewModel
    {
        public int MaHoSo { get; set; }

        public string HoTenThiSinh { get; set; } = string.Empty;

        public string TenKyThi { get; set; } = string.Empty;

        public string TenChungChi { get; set; } = string.Empty;

        public DateTime NgayNop { get; set; }

        public TrangThaiHoSo TrangThai { get; set; }
    }
    public class XetDuyetHoSoIndexViewModel
    {
        // Danh sách các hồ sơ hiển thị trên trang
        public List<XetDuyetHoSoDongViewModel> DanhSach { get; set; } = new();

        // Nội dung người dùng nhập vào ô tìm kiếm
        public string? TuKhoa { get; set; }

        // Trạng thái được chọn trong bộ lọc
        public TrangThaiHoSo? TrangThai { get; set; }

        // Thông tin trang hiện tại, tổng bản ghi và tổng số trang
        public PhanTrangViewModel PhanTrang { get; set; } = new();
    }
    public class XetDuyetHoSoChiTietViewModel
    {
        // Thông tin hồ sơ
        public int MaHoSo { get; set; }
        public DateTime NgayNop { get; set; }
        public string MucDichDuThi { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
        public TrangThaiHoSo TrangThai { get; set; }

        // Thông tin thí sinh
        public int MaThiSinh { get; set; }
        public string HoTenThiSinh { get; set; } = string.Empty;
        public DateTime NgaySinh { get; set; }
        public GioiTinh GioiTinh { get; set; }
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? DiaChi { get; set; }
        public string? TrinhDoHienTai { get; set; }
        public string? NgheNghiep { get; set; }
        public int SoLanThiTruocDay { get; set; }
        public string? ChungChiDaCo { get; set; }
        public TrangThaiThiSinh TrangThaiThiSinh { get; set; }

        // Thông tin kỳ thi
        public int MaKyThi { get; set; }
        public string TenKyThi { get; set; } = string.Empty;
        public string TenChungChi { get; set; } = string.Empty;
        public string CapDoThi { get; set; } = string.Empty;
        public decimal DiemDauVaoToiThieu { get; set; }
        public string? DieuKienDuThi { get; set; }
        public DateTime NgayBatDauDangKy { get; set; }
        public DateTime HanDangKy { get; set; }

        // Thông tin chỉ tiêu
        public int SoChoThiToiDa { get; set; }
        public int SoSuatDaSuDung { get; set; }

        public int SoChoConLai
            => Math.Max(0, SoChoThiToiDa - SoSuatDaSuDung);

        // Thông tin xử lý
        public DateTime? NgayXuLy { get; set; }
        public string? TenNguoiXuLy { get; set; }
        public string? NhanXetXetDuyet { get; set; }

        // Chỉ hồ sơ chờ duyệt mới được thao tác.
        public bool CoTheXetDuyet
            => TrangThai == TrangThaiHoSo.ChoDuyet;
    }
    public class XetDuyetHoSoFormViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Mã hồ sơ không hợp lệ")]
        public int MaHoSo { get; set; }

        [StringLength(
            1000,
            ErrorMessage = "Nhận xét xét duyệt tối đa 1000 ký tự")]
        [Display(Name = "Nhận xét xét duyệt")]
        public string? NhanXet { get; set; }
    }
}
