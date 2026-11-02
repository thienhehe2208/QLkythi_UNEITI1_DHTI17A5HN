// ============================================================
// Họ và tên: Chu Văn Trọng
// Nội dung thực hiện: Module 3 - quy tắc hồ sơ cá nhân và đăng ký dự thi.
// ============================================================

using System.ComponentModel.DataAnnotations;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.ViewModels.Module3;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Services.Module3;

public static class QuyTacHoSoModule3
{
    // Không cho tạo hồ sơ thứ hai cho cùng một kỳ thi khi hồ sơ cũ còn hiệu lực
    // hoặc đã có kết quả cuối cùng. Hồ sơ bị từ chối/đã hủy có thể đăng ký lại.
    public static readonly TrangThaiHoSo[] KhongChoDangKyTrung =
    [
        TrangThaiHoSo.ChoDuyet,
        TrangThaiHoSo.DuDieuKien,
        TrangThaiHoSo.ChoThi,
        TrangThaiHoSo.DaThi,
        TrangThaiHoSo.Dat,
        TrangThaiHoSo.KhongDat
    ];

    public static List<ValidationResult> KiemTraCaNhan(HoSoCaNhanFormViewModel model, DateTime bayGio)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), errors, true);
        if (model.NgaySinh.HasValue
            && (model.NgaySinh.Value.Date == DateTime.MinValue.Date || model.NgaySinh.Value.Date > bayGio.Date))
        {
            errors.Add(new ValidationResult(
                "Ngày sinh phải hợp lệ và không nằm trong tương lai.",
                [nameof(model.NgaySinh)]));
        }
        return errors;
    }

    public static HoSoCaNhanViewModel CaNhan(ThiSinh thiSinh, DateTime bayGio)
    {
        var model = new HoSoCaNhanViewModel
        {
            MaThiSinh = thiSinh.MaThiSinh,
            HoTen = thiSinh.HoTen,
            NgaySinh = thiSinh.NgaySinh,
            GioiTinh = thiSinh.GioiTinh,
            SoDienThoai = thiSinh.SoDienThoai,
            Email = thiSinh.Email,
            DiaChi = thiSinh.DiaChi,
            TrinhDoHienTai = thiSinh.TrinhDoHienTai,
            NgheNghiep = thiSinh.NgheNghiep,
            SoLanThiTruocDay = thiSinh.SoLanThiTruocDay,
            ChungChiDaCo = thiSinh.ChungChiDaCo,
            TrangThai = thiSinh.TrangThai
        };
        model.ThongTinCanBoSung = KiemTraCaNhan(model, bayGio)
            .Select(error => error.ErrorMessage ?? "Thông tin cá nhân chưa hợp lệ.")
            .ToList();
        return model;
    }

    public static bool KetQuaDaCongBo(TrangThaiHoSo trangThai, KetQuaCuoiCung ketQua)
        => (trangThai == TrangThaiHoSo.Dat && ketQua == KetQuaCuoiCung.Dat)
           || (trangThai == TrangThaiHoSo.KhongDat && ketQua == KetQuaCuoiCung.KhongDat);
}
