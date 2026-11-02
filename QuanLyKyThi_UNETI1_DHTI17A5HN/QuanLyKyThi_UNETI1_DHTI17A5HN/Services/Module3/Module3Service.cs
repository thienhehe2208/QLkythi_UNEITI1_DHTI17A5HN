// ============================================================
// Họ và tên: Chu Văn Trọng
// Nội dung thực hiện: Module 3 - hồ sơ cá nhân, nộp/hủy hồ sơ
//   theo transaction và theo dõi tiến trình dự thi của thí sinh.
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.ViewModels.Module3;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Services.Module3;

public class Module3Service(
    ApplicationDbContext context,
    TimeProvider timeProvider,
    ILogger<Module3Service> logger)
{
    public DateTime BayGio => timeProvider.GetUtcNow().ToOffset(TimeSpan.FromHours(7)).DateTime;

    private async Task<TaiKhoan> LayTaiKhoanHopLeAsync(int maTaiKhoan, VaiTro vaiTro)
    {
        var taiKhoan = await context.TaiKhoans.SingleOrDefaultAsync(t =>
            t.MaTaiKhoan == maTaiKhoan && t.VaiTro == vaiTro && t.TrangThai == TrangThaiTaiKhoan.HoatDong);
        return taiKhoan ?? throw new NghiepVuModule3Exception(
            "TAI_KHOAN",
            "Tài khoản không còn quyền thực hiện thao tác. Vui lòng đăng nhập lại.");
    }

    public async Task<HoSoCaNhanViewModel> DocHoSoCaNhanAsync(int maTaiKhoan)
    {
        var thiSinh = await context.ThiSinhs.AsNoTracking()
            .SingleOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);
        return thiSinh == null ? new() : QuyTacHoSoModule3.CaNhan(thiSinh, BayGio);
    }

    public async Task LuuHoSoCaNhanAsync(int maTaiKhoan, HoSoCaNhanFormViewModel model)
    {
        var errors = QuyTacHoSoModule3.KiemTraCaNhan(model, BayGio);
        if (errors.Count != 0)
        {
            throw new NghiepVuModule3Exception(
                "CA_NHAN_KHONG_HOP_LE",
                errors[0].ErrorMessage!,
                errors[0].MemberNames.FirstOrDefault() ?? "");
        }

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var taiKhoan = await LayTaiKhoanHopLeAsync(maTaiKhoan, VaiTro.ThiSinh);
        var thiSinh = await context.ThiSinhs.SingleOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);
        if (thiSinh == null)
        {
            thiSinh = new ThiSinh
            {
                MaTaiKhoan = maTaiKhoan,
                TrangThai = TrangThaiThiSinh.HoatDong
            };
            context.ThiSinhs.Add(thiSinh);
        }

        thiSinh.HoTen = model.HoTen.Trim();
        thiSinh.NgaySinh = model.NgaySinh!.Value.Date;
        thiSinh.GioiTinh = model.GioiTinh!.Value;
        thiSinh.SoDienThoai = model.SoDienThoai.Trim();
        thiSinh.Email = model.Email.Trim();
        thiSinh.DiaChi = model.DiaChi?.Trim();
        thiSinh.TrinhDoHienTai = model.TrinhDoHienTai?.Trim();
        thiSinh.NgheNghiep = model.NgheNghiep?.Trim();
        thiSinh.SoLanThiTruocDay = model.SoLanThiTruocDay!.Value;
        thiSinh.ChungChiDaCo = model.ChungChiDaCo?.Trim();

        // Đồng bộ hai trường dùng trên thanh tài khoản của giao diện hiện tại.
        taiKhoan.HoTen = thiSinh.HoTen;
        taiKhoan.Email = thiSinh.Email;

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task NapFormNopAsync(int maTaiKhoan, NopHoSoThiViewModel model)
    {
        var bayGio = BayGio;
        model.CaNhan = await DocHoSoCaNhanAsync(maTaiKhoan);
        model.LyDoKhongTheNop = [];
        if (!model.CaNhan.DaKhaiBao)
        {
            model.LyDoKhongTheNop.Add("Bạn cần khai báo hồ sơ cá nhân trước khi nộp.");
        }
        else
        {
            if (model.CaNhan.TrangThai != TrangThaiThiSinh.HoatDong)
                model.LyDoKhongTheNop.Add("Hồ sơ thí sinh đang ngừng hoạt động. Vui lòng liên hệ quản trị viên.");
            model.LyDoKhongTheNop.AddRange(model.CaNhan.ThongTinCanBoSung);
        }

        model.KyThis = await context.KyThis.AsNoTracking()
            .Where(k => k.TrangThai == TrangThaiKyThi.DangMoDangKy
                && k.NgayBatDauDangKy <= bayGio && k.HanDangKy >= bayGio)
            .OrderBy(k => k.TenKyThi)
            .Select(k => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = k.MaKyThi.ToString(),
                Text = k.TenKyThi + " — " + k.ChungChi!.TenChungChi
            })
            .ToListAsync();

        model.KyThi = null;
        if (!model.MaKyThi.HasValue) return;

        model.KyThi = await context.KyThis.AsNoTracking()
            .Where(k => k.MaKyThi == model.MaKyThi.Value)
            .Select(k => new KyThiNopViewModel
            {
                TenKyThi = k.TenKyThi,
                TenChungChi = k.ChungChi!.TenChungChi,
                CapDoThi = k.CapDoThi,
                DiemDauVaoToiThieu = k.DiemDauVaoToiThieu,
                LePhiDuThi = k.LePhiDuThi,
                NgayBatDauDangKy = k.NgayBatDauDangKy,
                HanDangKy = k.HanDangKy,
                DieuKienDuThi = k.DieuKienDuThi,
                MoTaKyThi = k.MoTaKyThi,
                ConNhanHoSo = k.TrangThai == TrangThaiKyThi.DangMoDangKy
                    && k.NgayBatDauDangKy <= bayGio && k.HanDangKy >= bayGio
            })
            .SingleOrDefaultAsync();

        if (model.KyThi == null)
            model.LyDoKhongTheNop.Add("Kỳ thi đã chọn không tồn tại. Hãy chọn lại kỳ thi.");
        else if (!model.KyThi.ConNhanHoSo)
            model.LyDoKhongTheNop.Add("Kỳ thi đã chọn không còn nhận hồ sơ tại thời điểm này.");
    }

    public async Task<int> NopHoSoAsync(int maTaiKhoan, NopHoSoThiViewModel model)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(model, new ValidationContext(model), errors, true))
        {
            throw new NghiepVuModule3Exception(
                "FORM_KHONG_HOP_LE",
                errors[0].ErrorMessage!,
                errors[0].MemberNames.FirstOrDefault() ?? "");
        }

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        await LayTaiKhoanHopLeAsync(maTaiKhoan, VaiTro.ThiSinh);

        var thiSinh = await context.ThiSinhs.AsNoTracking()
            .SingleOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan)
            ?? throw new NghiepVuModule3Exception(
                "CHUA_CO_CA_NHAN",
                "Bạn cần khai báo hồ sơ cá nhân trước khi nộp.");

        if (thiSinh.TrangThai != TrangThaiThiSinh.HoatDong)
            throw new NghiepVuModule3Exception("THI_SINH_NGUNG", "Thí sinh đang ngừng hoạt động, không được nộp hồ sơ mới.");

        var thieu = QuyTacHoSoModule3.CaNhan(thiSinh, BayGio).ThongTinCanBoSung;
        if (thieu.Count != 0)
            throw new NghiepVuModule3Exception("CA_NHAN_CHUA_DU", "Cần cập nhật hồ sơ cá nhân: " + string.Join(" ", thieu));

        var kyThi = await context.KyThis.AsNoTracking().SingleOrDefaultAsync(k => k.MaKyThi == model.MaKyThi)
            ?? throw new NghiepVuModule3Exception("KY_THI_KHONG_TON_TAI", "Kỳ thi không tồn tại.", nameof(model.MaKyThi));
        var bayGio = BayGio;
        if (kyThi.TrangThai != TrangThaiKyThi.DangMoDangKy
            || kyThi.NgayBatDauDangKy > bayGio || kyThi.HanDangKy < bayGio)
        {
            throw new NghiepVuModule3Exception(
                "KY_THI_KHONG_NHAN",
                "Kỳ thi chưa mở, đã đóng/tạm dừng hoặc ngoài thời gian nhận hồ sơ.",
                nameof(model.MaKyThi));
        }

        if (await context.HoSoDangKyThis.AnyAsync(h =>
            h.MaThiSinh == thiSinh.MaThiSinh && h.MaKyThi == kyThi.MaKyThi
            && QuyTacHoSoModule3.KhongChoDangKyTrung.Contains(h.TrangThai)))
        {
            throw new NghiepVuModule3Exception(
                "TRUNG_HO_SO",
                "Bạn đã có hồ sơ cho kỳ thi này.",
                nameof(model.MaKyThi));
        }

        var hoSo = new HoSoDangKyThi
        {
            MaThiSinh = thiSinh.MaThiSinh,
            MaKyThi = kyThi.MaKyThi,
            NgayNop = bayGio,
            MucDichDuThi = model.MucDichDuThi.Trim(),
            GhiChu = model.GhiChu?.Trim(),
            TrangThai = TrangThaiHoSo.ChoDuyet,
            NgayXuLy = null,
            NhanXetXetDuyet = null,
            MaNguoiXuLy = null
        };
        context.HoSoDangKyThis.Add(hoSo);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return hoSo.MaHoSo;
    }

    public async Task HuyHoSoAsync(int maTaiKhoan, int maHoSo)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        await LayTaiKhoanHopLeAsync(maTaiKhoan, VaiTro.ThiSinh);
        var hoSo = await context.HoSoDangKyThis
            .SingleOrDefaultAsync(h => h.MaHoSo == maHoSo && h.ThiSinh!.MaTaiKhoan == maTaiKhoan)
            ?? throw new NghiepVuModule3Exception("KHONG_TIM_THAY", "Không tìm thấy hồ sơ.");

        if ((hoSo.TrangThai != TrangThaiHoSo.ChoDuyet && hoSo.TrangThai != TrangThaiHoSo.DuDieuKien)
            || await context.LichThis.AnyAsync(l => l.MaHoSo == maHoSo && l.TrangThai != TrangThaiLichThi.DaHuy)
            || await context.KetQuaThis.AnyAsync(k => k.MaHoSo == maHoSo))
        {
            throw new NghiepVuModule3Exception(
                "KHONG_THE_HUY",
                "Hồ sơ không còn được phép hủy. Chỉ hủy hồ sơ Chờ duyệt/Đủ điều kiện, không có lịch còn hiệu lực và chưa có kết quả.");
        }

        // Giữ nguyên lịch sử xét duyệt của Module 4, chỉ chuyển trạng thái.
        hoSo.TrangThai = TrangThaiHoSo.DaHuy;
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<ChiTietHoSoThiViewModel?> DocChiTietHoSoAsync(int maTaiKhoan, int maHoSo)
    {
        var hoSo = await context.HoSoDangKyThis.AsNoTracking().AsSplitQuery()
            .Where(h => h.MaHoSo == maHoSo && h.ThiSinh!.MaTaiKhoan == maTaiKhoan)
            .Include(h => h.ThiSinh)
            .Include(h => h.KyThi).ThenInclude(k => k!.ChungChi)
            .Include(h => h.LichThis)
            .Include(h => h.KetQuaThi)
            .SingleOrDefaultAsync();
        if (hoSo == null) return null;

        var model = new ChiTietHoSoThiViewModel
        {
            MaHoSo = hoSo.MaHoSo,
            HoTenThiSinh = hoSo.ThiSinh!.HoTen,
            TenKyThi = hoSo.KyThi!.TenKyThi,
            TenChungChi = hoSo.KyThi.ChungChi!.TenChungChi,
            NgayNop = hoSo.NgayNop,
            TrangThai = hoSo.TrangThai,
            MucDichDuThi = hoSo.MucDichDuThi,
            GhiChu = hoSo.GhiChu,
            NgayXuLy = hoSo.NgayXuLy,
            NhanXetXetDuyet = hoSo.NhanXetXetDuyet,
            CoTheHuy = (hoSo.TrangThai == TrangThaiHoSo.ChoDuyet || hoSo.TrangThai == TrangThaiHoSo.DuDieuKien)
                && !hoSo.LichThis.Any(l => l.TrangThai != TrangThaiLichThi.DaHuy)
                && hoSo.KetQuaThi == null,
            LichThis = hoSo.LichThis
                .OrderByDescending(l => l.ThoiGianBatDau)
                .ThenByDescending(l => l.MaLichThi)
                .Select(l => new LichTheoDoiViewModel
                {
                    MaLichThi = l.MaLichThi,
                    ThoiGianBatDau = l.ThoiGianBatDau,
                    ThoiGianKetThuc = l.ThoiGianKetThuc,
                    HinhThucThi = l.HinhThucThi,
                    DiaDiemHoacLienKet = l.DiaDiemHoacLienKet,
                    PhongThi = l.PhongThi,
                    GiamThi = l.GiamThi,
                    TrangThai = l.TrangThai
                }).ToList()
        };

        if (hoSo.KetQuaThi is { } ketQua)
        {
            if (QuyTacHoSoModule3.KetQuaDaCongBo(hoSo.TrangThai, ketQua.KetQua))
            {
                model.KetQua = new KetQuaTheoDoiViewModel
                {
                    MaKetQuaThi = ketQua.MaKetQuaThi,
                    DiemDanhGia = ketQua.DiemDanhGia,
                    KetQua = ketQua.KetQua,
                    NhanXet = ketQua.NhanXet,
                    NgayCapNhat = ketQua.NgayCapNhat
                };
            }
            else
            {
                logger.LogWarning(
                    "Module 3: kết quả và trạng thái hồ sơ {MaHoSo} không khớp, chưa hiển thị kết quả.",
                    hoSo.MaHoSo);
            }
        }

        return model;
    }

    public async Task CapNhatTrangThaiAsync(int maAdmin, int maThiSinh, TrangThaiThiSinh trangThai)
    {
        if (!Enum.IsDefined(trangThai))
            throw new NghiepVuModule3Exception("TRANG_THAI", "Trạng thái thí sinh không hợp lệ.");

        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        await LayTaiKhoanHopLeAsync(maAdmin, VaiTro.Admin);
        var thiSinh = await context.ThiSinhs.SingleOrDefaultAsync(t => t.MaThiSinh == maThiSinh)
            ?? throw new NghiepVuModule3Exception("KHONG_TIM_THAY", "Không tìm thấy thí sinh.");
        thiSinh.TrangThai = trangThai;
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
