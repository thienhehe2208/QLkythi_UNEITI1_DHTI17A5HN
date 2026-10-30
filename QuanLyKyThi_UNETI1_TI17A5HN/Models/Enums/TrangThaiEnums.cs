// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Khai báo các enum vai trò/trạng thái dùng chung
//   theo QUY_UOC_NHOM (giá trị nội bộ không dấu, nhãn tiếng Việt qua [Display]).
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums
{
    /// <summary>Vai trò tài khoản (TaiKhoan.VaiTro).</summary>
    public enum VaiTro
    {
        [Display(Name = "Admin")] Admin,
        [Display(Name = "Cán bộ khảo thí")] CanBoKhaoThi,
        [Display(Name = "Thí sinh")] ThiSinh
    }

    /// <summary>Trạng thái tài khoản (TaiKhoan.TrangThai).</summary>
    public enum TrangThaiTaiKhoan
    {
        [Display(Name = "Hoạt động")] HoatDong,
        [Display(Name = "Bị khóa")] BiKhoa
    }

    /// <summary>Trạng thái chứng chỉ (ChungChi.TrangThai).</summary>
    public enum TrangThaiChungChi
    {
        [Display(Name = "Đang hoạt động")] DangHoatDong,
        [Display(Name = "Ngừng hoạt động")] NgungHoatDong
    }

    /// <summary>Trạng thái kỳ thi do cán bộ quản lý (KyThi.TrangThai).</summary>
    public enum TrangThaiKyThi
    {
        [Display(Name = "Chưa mở")] ChuaMo,
        [Display(Name = "Đang mở đăng ký")] DangMoDangKy,
        [Display(Name = "Tạm dừng")] TamDung,
        [Display(Name = "Đã đóng")] DaDong
    }

    /// <summary>Giới tính thí sinh (ThiSinh.GioiTinh).</summary>
    public enum GioiTinh
    {
        [Display(Name = "Nam")] Nam,
        [Display(Name = "Nữ")] Nu,
        [Display(Name = "Khác")] Khac
    }

    /// <summary>Trạng thái hồ sơ cá nhân thí sinh (ThiSinh.TrangThai).</summary>
    public enum TrangThaiThiSinh
    {
        [Display(Name = "Hoạt động")] HoatDong,
        [Display(Name = "Ngừng hoạt động")] NgungHoatDong
    }

    /// <summary>Trạng thái hồ sơ đăng ký dự thi (mục 5.1 quy ước).</summary>
    public enum TrangThaiHoSo
    {
        [Display(Name = "Chờ duyệt")] ChoDuyet,
        [Display(Name = "Đủ điều kiện dự thi")] DuDieuKien,
        [Display(Name = "Không đủ điều kiện dự thi")] KhongDuDieuKien,
        [Display(Name = "Chờ thi")] ChoThi,
        [Display(Name = "Đã thi")] DaThi,
        [Display(Name = "Đạt")] Dat,
        [Display(Name = "Không đạt")] KhongDat,
        [Display(Name = "Đã hủy")] DaHuy
    }

    /// <summary>Hình thức thi (LichThi.HinhThucThi).</summary>
    public enum HinhThucThi
    {
        [Display(Name = "Trực tiếp")] TrucTiep,
        [Display(Name = "Trực tuyến")] TrucTuyen
    }

    /// <summary>Trạng thái lịch thi (LichThi.TrangThai).</summary>
    public enum TrangThaiLichThi
    {
        [Display(Name = "Đã lên lịch")] DaLenLich,
        [Display(Name = "Đã hoàn thành")] DaHoanThanh,
        [Display(Name = "Đã hủy")] DaHuy
    }

    /// <summary>Kết quả cuối cùng (KetQuaThi.KetQua).</summary>
    public enum KetQuaCuoiCung
    {
        [Display(Name = "Đạt")] Dat,
        [Display(Name = "Không đạt")] KhongDat
    }
}
