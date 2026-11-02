// ============================================================
// Nội dung bổ sung: ViewModel cổng thông tin dành cho thí sinh.
// ============================================================

using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels
{
    public class NganhNgheTheViewModel
    {
        public int MaNganhNghe { get; set; }
        public string TenNganhNghe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string BieuTuong { get; set; } = "bi-grid";
        public int SoChungChi { get; set; }
    }

    public class ChungChiTheViewModel
    {
        public int MaChungChi { get; set; }
        public string TenChungChi { get; set; } = string.Empty;
        public string? TenNganhNghe { get; set; }
        public string? MoTa { get; set; }
        public bool DaYeuThich { get; set; }
        public int SoKyThiDangMo { get; set; }
        public decimal? LePhiThapNhat { get; set; }
    }

    public class KhamPhaChungChiViewModel
    {
        public List<NganhNgheTheViewModel> NganhNghes { get; set; } = new();
        public List<ChungChiTheViewModel> ChungChis { get; set; } = new();
        public int? MaNganhNghe { get; set; }
        public string? TuKhoa { get; set; }
    }

    public class KyThiDangMoItemViewModel
    {
        public int MaKyThi { get; set; }
        public string TenKyThi { get; set; } = string.Empty;
        public string TenChungChi { get; set; } = string.Empty;
        public string? TenNganhNghe { get; set; }
        public string CapDoThi { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public decimal LePhiDuThi { get; set; }
        public DateTime HanDangKy { get; set; }
        public int SoChoThiToiDa { get; set; }
        public int SoChoDaGiu { get; set; }
        public bool DaDangKy { get; set; }
    }

    public class KyThiDangMoViewModel
    {
        public List<KyThiDangMoItemViewModel> DanhSach { get; set; } = new();
        public int? MaChungChi { get; set; }
    }

    public class LichThiCuaToiItemViewModel
    {
        public int MaLichThi { get; set; }
        public string TenKyThi { get; set; } = string.Empty;
        public string TenChungChi { get; set; } = string.Empty;
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public HinhThucThi HinhThucThi { get; set; }
        public string DiaDiem { get; set; } = string.Empty;
        public string? PhongThi { get; set; }
        public TrangThaiLichThi TrangThai { get; set; }
    }

    public class LichThiCuaToiViewModel
    {
        public DateTime ThangHienThi { get; set; }
        public List<LichThiCuaToiItemViewModel> DanhSach { get; set; } = new();
    }

    public class LichSuDuThiItemViewModel
    {
        public int MaHoSo { get; set; }
        public string TenKyThi { get; set; } = string.Empty;
        public string TenChungChi { get; set; } = string.Empty;
        public DateTime NgayNop { get; set; }
        public TrangThaiHoSo TrangThai { get; set; }
        public decimal? DiemDanhGia { get; set; }
        public KetQuaCuoiCung? KetQua { get; set; }
    }
}
