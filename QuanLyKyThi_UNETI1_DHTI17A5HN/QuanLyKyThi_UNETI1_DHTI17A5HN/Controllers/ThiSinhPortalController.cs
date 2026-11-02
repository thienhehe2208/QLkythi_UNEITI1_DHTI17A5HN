// ============================================================
// Nội dung bổ sung: Cổng tra cứu và đăng ký dành riêng cho thí sinh.
// Thí sinh đăng ký kỳ thi; lịch thi chính thức do cán bộ sắp xếp.
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;
using QuanLyKyThi_UNETI1_TI17A5HN.Filters;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.Services;
using QuanLyKyThi_UNETI1_TI17A5HN.ViewModels;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Controllers
{
    [PhanQuyen(VaiTro.ThiSinh)]
    public class ThiSinhPortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ThiSinhPortalController(ApplicationDbContext context) => _context = context;

        private int MaTaiKhoan => HttpContext.Session.LayMaTaiKhoan()!.Value;

        public async Task<IActionResult> KhamPha(int? maNganhNghe, string? tuKhoa)
        {
            var maThiSinh = await LayMaThiSinhAsync();
            if (!maThiSinh.HasValue) return ChuyenDenHoSoCaNhan();

            var bayGio = DateTime.Now;
            var query = _context.ChungChis.AsNoTracking()
                .Where(c => c.TrangThai == TrangThaiChungChi.DangHoatDong);
            if (maNganhNghe.HasValue) query = query.Where(c => c.MaNganhNghe == maNganhNghe);
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim();
                query = query.Where(c => c.TenChungChi.Contains(tk) || (c.MoTa != null && c.MoTa.Contains(tk)));
            }

            var model = new KhamPhaChungChiViewModel
            {
                MaNganhNghe = maNganhNghe,
                TuKhoa = tuKhoa,
                NganhNghes = await _context.NganhNghes.AsNoTracking()
                    .Where(n => n.DangHienThi)
                    .OrderBy(n => n.TenNganhNghe)
                    .Select(n => new NganhNgheTheViewModel
                    {
                        MaNganhNghe = n.MaNganhNghe,
                        TenNganhNghe = n.TenNganhNghe,
                        MoTa = n.MoTa,
                        BieuTuong = n.BieuTuong ?? "bi-grid",
                        SoChungChi = n.ChungChis.Count(c => c.TrangThai == TrangThaiChungChi.DangHoatDong)
                    }).ToListAsync(),
                ChungChis = await query.OrderBy(c => c.TenChungChi)
                    .Select(c => new ChungChiTheViewModel
                    {
                        MaChungChi = c.MaChungChi,
                        TenChungChi = c.TenChungChi,
                        TenNganhNghe = c.NganhNghe != null ? c.NganhNghe.TenNganhNghe : null,
                        MoTa = c.MoTa,
                        DaYeuThich = c.ChungChiYeuThichs.Any(y => y.MaThiSinh == maThiSinh.Value),
                        SoKyThiDangMo = c.KyThis.Count(k => k.TrangThai == TrangThaiKyThi.DangMoDangKy
                            && k.NgayBatDauDangKy <= bayGio && k.HanDangKy >= bayGio),
                        LePhiThapNhat = c.KyThis.Where(k => k.TrangThai == TrangThaiKyThi.DangMoDangKy
                            && k.NgayBatDauDangKy <= bayGio && k.HanDangKy >= bayGio)
                            .Select(k => (decimal?)k.LePhiDuThi).Min()
                    }).ToListAsync()
            };
            return View(model);
        }

        public async Task<IActionResult> KyThiDangMo(int? maChungChi)
        {
            var maThiSinh = await LayMaThiSinhAsync();
            if (!maThiSinh.HasValue) return ChuyenDenHoSoCaNhan();
            var bayGio = DateTime.Now;
            var query = _context.KyThis.AsNoTracking()
                .Where(k => k.TrangThai == TrangThaiKyThi.DangMoDangKy
                    && k.NgayBatDauDangKy <= bayGio && k.HanDangKy >= bayGio);
            if (maChungChi.HasValue) query = query.Where(k => k.MaChungChi == maChungChi);

            return View(new KyThiDangMoViewModel
            {
                MaChungChi = maChungChi,
                DanhSach = await query.OrderBy(k => k.HanDangKy)
                    .Select(k => new KyThiDangMoItemViewModel
                    {
                        MaKyThi = k.MaKyThi,
                        TenKyThi = k.TenKyThi,
                        TenChungChi = k.ChungChi!.TenChungChi,
                        TenNganhNghe = k.ChungChi.NganhNghe != null ? k.ChungChi.NganhNghe.TenNganhNghe : null,
                        CapDoThi = k.CapDoThi,
                        MoTa = k.MoTaKyThi,
                        LePhiDuThi = k.LePhiDuThi,
                        HanDangKy = k.HanDangKy,
                        SoChoThiToiDa = k.SoChoThiToiDa,
                        SoChoDaGiu = k.HoSoDangKyThis.Count(h => h.TrangThai == TrangThaiHoSo.DuDieuKien
                            || h.TrangThai == TrangThaiHoSo.ChoThi || h.TrangThai == TrangThaiHoSo.DaThi
                            || h.TrangThai == TrangThaiHoSo.Dat || h.TrangThai == TrangThaiHoSo.KhongDat),
                        DaDangKy = k.HoSoDangKyThis.Any(h => h.MaThiSinh == maThiSinh.Value
                            && h.TrangThai != TrangThaiHoSo.DaHuy && h.TrangThai != TrangThaiHoSo.KhongDuDieuKien)
                    }).ToListAsync()
            });
        }

        public async Task<IActionResult> LichThiCuaToi(int? thang, int? nam)
        {
            var maThiSinh = await LayMaThiSinhAsync();
            if (!maThiSinh.HasValue) return ChuyenDenHoSoCaNhan();
            var homNay = DateTime.Today;
            var namHienThi = nam is >= 2000 and <= 2100 ? nam.Value : homNay.Year;
            var thangHienThi = thang is >= 1 and <= 12 ? thang.Value : homNay.Month;
            var dauThang = new DateTime(namHienThi, thangHienThi, 1);
            var cuoiThang = dauThang.AddMonths(1);

            var danhSach = await _context.LichThis.AsNoTracking()
                .Where(l => l.HoSoDangKyThi!.MaThiSinh == maThiSinh.Value
                    && l.ThoiGianBatDau < cuoiThang && l.ThoiGianKetThuc >= dauThang)
                .OrderBy(l => l.ThoiGianBatDau)
                .Select(l => new LichThiCuaToiItemViewModel
                {
                    MaLichThi = l.MaLichThi,
                    TenKyThi = l.HoSoDangKyThi!.KyThi!.TenKyThi,
                    TenChungChi = l.HoSoDangKyThi.KyThi.ChungChi!.TenChungChi,
                    ThoiGianBatDau = l.ThoiGianBatDau,
                    ThoiGianKetThuc = l.ThoiGianKetThuc,
                    HinhThucThi = l.HinhThucThi,
                    DiaDiem = l.DiaDiemHoacLienKet,
                    PhongThi = l.PhongThi,
                    TrangThai = l.TrangThai
                }).ToListAsync();

            return View(new LichThiCuaToiViewModel { ThangHienThi = dauThang, DanhSach = danhSach });
        }

        public async Task<IActionResult> LichSu()
        {
            var maThiSinh = await LayMaThiSinhAsync();
            if (!maThiSinh.HasValue) return ChuyenDenHoSoCaNhan();
            var model = await _context.HoSoDangKyThis.AsNoTracking()
                .Where(h => h.MaThiSinh == maThiSinh.Value)
                .OrderByDescending(h => h.NgayNop)
                .Select(h => new LichSuDuThiItemViewModel
                {
                    MaHoSo = h.MaHoSo,
                    TenKyThi = h.KyThi!.TenKyThi,
                    TenChungChi = h.KyThi.ChungChi!.TenChungChi,
                    NgayNop = h.NgayNop,
                    TrangThai = h.TrangThai,
                    DiemDanhGia = h.KetQuaThi != null ? h.KetQuaThi.DiemDanhGia : null,
                    KetQua = h.KetQuaThi != null ? h.KetQuaThi.KetQua : null
                }).ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> YeuThich()
        {
            var maThiSinh = await LayMaThiSinhAsync();
            if (!maThiSinh.HasValue) return ChuyenDenHoSoCaNhan();
            var bayGio = DateTime.Now;
            var model = await _context.ChungChiYeuThichs.AsNoTracking()
                .Where(y => y.MaThiSinh == maThiSinh.Value && y.ChungChi!.TrangThai == TrangThaiChungChi.DangHoatDong)
                .OrderByDescending(y => y.NgayThem)
                .Select(y => new ChungChiTheViewModel
                {
                    MaChungChi = y.MaChungChi,
                    TenChungChi = y.ChungChi!.TenChungChi,
                    TenNganhNghe = y.ChungChi.NganhNghe != null ? y.ChungChi.NganhNghe.TenNganhNghe : null,
                    MoTa = y.ChungChi.MoTa,
                    DaYeuThich = true,
                    SoKyThiDangMo = y.ChungChi.KyThis.Count(k => k.TrangThai == TrangThaiKyThi.DangMoDangKy
                        && k.NgayBatDauDangKy <= bayGio && k.HanDangKy >= bayGio),
                    LePhiThapNhat = y.ChungChi.KyThis.Where(k => k.TrangThai == TrangThaiKyThi.DangMoDangKy
                        && k.NgayBatDauDangKy <= bayGio && k.HanDangKy >= bayGio)
                        .Select(k => (decimal?)k.LePhiDuThi).Min()
                }).ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChuyenYeuThich(int maChungChi, string? returnUrl)
        {
            var maThiSinh = await LayMaThiSinhAsync();
            if (!maThiSinh.HasValue) return ChuyenDenHoSoCaNhan();
            if (!await _context.ChungChis.AnyAsync(c => c.MaChungChi == maChungChi
                && c.TrangThai == TrangThaiChungChi.DangHoatDong)) return NotFound();

            var daCo = await _context.ChungChiYeuThichs.FindAsync(maThiSinh.Value, maChungChi);
            if (daCo is null)
            {
                _context.ChungChiYeuThichs.Add(new ChungChiYeuThich
                {
                    MaThiSinh = maThiSinh.Value, MaChungChi = maChungChi, NgayThem = DateTime.Now
                });
                TempData["ThongBao"] = "Đã thêm chứng chỉ vào danh sách yêu thích.";
            }
            else
            {
                _context.ChungChiYeuThichs.Remove(daCo);
                TempData["ThongBao"] = "Đã bỏ chứng chỉ khỏi danh sách yêu thích.";
            }
            await _context.SaveChangesAsync();
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction(nameof(KhamPha));
        }

        private async Task<int?> LayMaThiSinhAsync()
            => await _context.ThiSinhs.Where(t => t.MaTaiKhoan == MaTaiKhoan)
                .Select(t => (int?)t.MaThiSinh).FirstOrDefaultAsync();

        private IActionResult ChuyenDenHoSoCaNhan()
        {
            TempData["LoiThongBao"] = "Vui lòng hoàn thiện thông tin cá nhân trước khi sử dụng chức năng này.";
            return RedirectToAction("Edit", "ThiSinh");
        }
    }
}
