// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: ViewModel quản lý chứng chỉ (danh sách + tìm kiếm/lọc/phân trang,
//   form thêm/sửa, chi tiết kèm kỳ thi liên quan).
// ============================================================

using System.ComponentModel.DataAnnotations;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels
{
    public class ChungChiDongViewModel
    {
        public int MaChungChi { get; set; }
        public string TenChungChi { get; set; } = string.Empty;
        public string? EmailLienHe { get; set; }
        public TrangThaiChungChi TrangThai { get; set; }
        public int SoKyThi { get; set; }
    }

    public class ChungChiIndexViewModel
    {
        public List<ChungChiDongViewModel> DanhSach { get; set; } = new();

        [Display(Name = "Từ khóa")]
        public string? TuKhoa { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiChungChi? TrangThai { get; set; }

        /// <summary>ten_asc | ten_desc | sokythi_desc</summary>
        [Display(Name = "Sắp xếp")]
        public string? SapXep { get; set; }

        public bool LaAdmin { get; set; }

        public PhanTrangViewModel PhanTrang { get; set; } = new();
    }

    public class ChungChiFormViewModel
    {
        public int MaChungChi { get; set; }

        [Required(ErrorMessage = "Tên chứng chỉ không được để trống")]
        [StringLength(150, ErrorMessage = "Tên chứng chỉ tối đa 150 ký tự")]
        [Display(Name = "Tên chứng chỉ")]
        public string TenChungChi { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [EmailAddress(ErrorMessage = "Email liên hệ không đúng định dạng")]
        [StringLength(150, ErrorMessage = "Email tối đa 150 ký tự")]
        [Display(Name = "Email liên hệ")]
        public string? EmailLienHe { get; set; }

        /// <summary>Chỉ hiển thị trên form sửa; đổi trạng thái qua action CapNhatTrangThai.</summary>
        [Display(Name = "Trạng thái")]
        public TrangThaiChungChi TrangThai { get; set; } = TrangThaiChungChi.DangHoatDong;
    }

    public class KyThiLienQuanViewModel
    {
        public int MaKyThi { get; set; }
        public string TenKyThi { get; set; } = string.Empty;
        public string CapDoThi { get; set; } = string.Empty;
        public DateTime HanDangKy { get; set; }
        public TrangThaiKyThi TrangThai { get; set; }
    }

    public class ChungChiChiTietViewModel
    {
        public int MaChungChi { get; set; }
        public string TenChungChi { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? EmailLienHe { get; set; }
        public TrangThaiChungChi TrangThai { get; set; }
        public int TongSoKyThi { get; set; }
        public int SoKyThiDangMo { get; set; }
        public List<KyThiLienQuanViewModel> KyThiGanDay { get; set; } = new();
        public bool LaAdmin { get; set; }

        /// <summary>Chỉ được xóa khi không có kỳ thi nào tham chiếu.</summary>
        public bool CoTheXoa => TongSoKyThi == 0;
    }
}
