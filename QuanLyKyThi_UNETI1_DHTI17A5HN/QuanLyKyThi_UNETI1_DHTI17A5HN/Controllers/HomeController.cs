// Phạm Quang Minh - 23103100201: bổ sung Dashboard đủ chỉ số mục 9.5.
// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Trang chủ (số liệu nhanh + lối tắt theo vai trò) và trang lỗi chung.
// ============================================================

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.Services;
using QuanLyKyThi_UNETI1_TI17A5HN.ViewModels;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [QuanLyKyThi_UNETI1_TI17A5HN.Filters.PhanQuyen]
        public async Task<IActionResult> Index()
        {
            // Khách chưa đăng nhập -> vào thẳng trang đăng nhập.
            if (!HttpContext.Session.DaDangNhap())
                return RedirectToAction("DangNhap", "Auth");

            var hienTai = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime;
            var model = new TrangChuViewModel
            {
                SoChungChiHoatDong = await _context.ChungChis.CountAsync(c => c.TrangThai == TrangThaiChungChi.DangHoatDong),
                SoKyThi = await _context.KyThis.CountAsync(),
                TongChungChi = await _context.ChungChis.CountAsync(),
                TongThiSinh = await _context.ThiSinhs.CountAsync(),
                SoKyThiDangMo = await _context.KyThis.CountAsync(k => k.TrangThai == TrangThaiKyThi.DangMoDangKy),
                TongHoSo = await _context.HoSoDangKyThis.CountAsync(),
                SoHoSoChoDuyet = await _context.HoSoDangKyThis.CountAsync(h => h.TrangThai == TrangThaiHoSo.ChoDuyet),
                SoHoSoDuDieuKien = await _context.HoSoDangKyThis.CountAsync(h => h.TrangThai == TrangThaiHoSo.DuDieuKien),
                SoHoSoKhongDuDieuKien = await _context.HoSoDangKyThis.CountAsync(h => h.TrangThai == TrangThaiHoSo.KhongDuDieuKien),
                SoLichThiSapToi = await _context.LichThis.CountAsync(l =>
                    l.TrangThai == TrangThaiLichThi.DaLenLich && l.ThoiGianBatDau > hienTai),
                SoKetQuaDat = await _context.KetQuaThis.CountAsync(k => k.KetQua == KetQuaCuoiCung.Dat),
                SoKetQuaKhongDat = await _context.KetQuaThis.CountAsync(k => k.KetQua == KetQuaCuoiCung.KhongDat),
                LichThiSapToi = await _context.LichThis
                    .AsNoTracking()
                    .Where(l => l.TrangThai == TrangThaiLichThi.DaLenLich && l.ThoiGianBatDau > hienTai)
                    .OrderBy(l => l.ThoiGianBatDau)
                    .Select(l => new LichThiSapToiViewModel
                    {
                        MaLichThi = l.MaLichThi,
                        TenKyThi = l.HoSoDangKyThi!.KyThi!.TenKyThi,
                        HoTenThiSinh = l.HoSoDangKyThi.ThiSinh!.HoTen,
                        ThoiGianBatDau = l.ThoiGianBatDau,
                        DiaDiem = l.PhongThi ?? l.DiaDiemHoacLienKet
                    })
                    .Take(5)
                    .ToListAsync()
            };

            if (HttpContext.Session.LaVaiTro(VaiTro.Admin))
            {
                model.SoTaiKhoan = await _context.TaiKhoans.CountAsync();
                model.SoTaiKhoanBiKhoa = await _context.TaiKhoans.CountAsync(t => t.TrangThai == TrangThaiTaiKhoan.BiKhoa);
            }

            if (HttpContext.Session.LaVaiTro(VaiTro.ThiSinh))
            {
                var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan()!.Value;
                var maThiSinh = await _context.ThiSinhs.Where(t => t.MaTaiKhoan == maTaiKhoan)
                    .Select(t => (int?)t.MaThiSinh).FirstOrDefaultAsync();
                model.NganhNghes = await _context.NganhNghes.AsNoTracking()
                    .Where(n => n.DangHienThi)
                    .OrderBy(n => n.TenNganhNghe)
                    .Select(n => new NganhNgheTheViewModel
                    {
                        MaNganhNghe = n.MaNganhNghe,
                        TenNganhNghe = n.TenNganhNghe,
                        MoTa = n.MoTa,
                        BieuTuong = n.BieuTuong ?? "bi-grid",
                        SoChungChi = n.ChungChis.Count(c => c.TrangThai == TrangThaiChungChi.DangHoatDong)
                    }).ToListAsync();

                model.KyThiDangMo = await _context.KyThis.AsNoTracking()
                    .Where(k => k.TrangThai == TrangThaiKyThi.DangMoDangKy
                        && k.NgayBatDauDangKy <= hienTai && k.HanDangKy >= hienTai)
                    .OrderBy(k => k.HanDangKy).Take(4)
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
                        DaDangKy = maThiSinh.HasValue && k.HoSoDangKyThis.Any(h => h.MaThiSinh == maThiSinh.Value
                            && h.TrangThai != TrangThaiHoSo.DaHuy && h.TrangThai != TrangThaiHoSo.KhongDuDieuKien)
                    }).ToListAsync();
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            ViewBag.StatusCode = statusCode;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
