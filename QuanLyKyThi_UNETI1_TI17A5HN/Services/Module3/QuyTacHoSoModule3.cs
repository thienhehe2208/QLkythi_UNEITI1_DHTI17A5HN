// Họ và tên: CHƯA CUNG CẤP. Mã sinh viên: CHƯA CUNG CẤP.
// Nội dung thực hiện: Module 3 — chính sách chống trùng, ngày sinh và hồ sơ bắt buộc theo TXT.
using System.ComponentModel.DataAnnotations;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.ViewModels.Module3;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Services.Module3;

public static class QuyTacHoSoModule3
{
    // Không dùng nhóm trạng thái giữ chỗ của Module 4 để chống đăng ký trùng.
    public static readonly TrangThaiHoSo[] DangXuLy =
        [TrangThaiHoSo.ChoDuyet, TrangThaiHoSo.DuDieuKien, TrangThaiHoSo.ChoThi, TrangThaiHoSo.DaThi];

    public static List<ValidationResult> KiemTraCaNhan(HoSoCaNhanFormViewModel model, DateTime bayGio)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), errors, true);
        if (model.NgaySinh.HasValue && (model.NgaySinh.Value.Date == DateTime.MinValue.Date || model.NgaySinh.Value.Date > bayGio.Date))
            errors.Add(new ValidationResult("Ngày sinh phải hợp lệ và không nằm trong tương lai.", [nameof(model.NgaySinh)]));
        return errors;
    }

    public static HoSoCaNhanViewModel CaNhan(ThiSinh t, DateTime bayGio)
    {
        var vm = new HoSoCaNhanViewModel
        {
            MaThiSinh = t.MaThiSinh, HoTen = t.HoTen, NgaySinh = t.NgaySinh, GioiTinh = t.GioiTinh,
            SoDienThoai = t.SoDienThoai, Email = t.Email, DiaChi = t.DiaChi, TrinhDoHienTai = t.TrinhDoHienTai,
            NgheNghiep = t.NgheNghiep, SoLanThiTruocDay = t.SoLanThiTruocDay, ChungChiDaCo = t.ChungChiDaCo, TrangThai = t.TrangThai
        };
        vm.ThongTinCanBoSung = KiemTraCaNhan(vm, bayGio).Select(e => e.ErrorMessage ?? "Thông tin cá nhân chưa hợp lệ.").ToList();
        return vm;
    }

    public static bool KetQuaDaCongBo(TrangThaiHoSo trangThai, KetQuaCuoiCung ketQua)
        => (trangThai == TrangThaiHoSo.Dat && ketQua == KetQuaCuoiCung.Dat)
           || (trangThai == TrangThaiHoSo.KhongDat && ketQua == KetQuaCuoiCung.KhongDat);
}
