// Phạm Quang Minh - 23103100201: 10 thống kê LINQ theo mục 9.6.
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;
using QuanLyKyThi_UNETI1_TI17A5HN.Filters;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.Services;
using QuanLyKyThi_UNETI1_TI17A5HN.ViewModels;
namespace QuanLyKyThi_UNETI1_TI17A5HN.Controllers
{
    [PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Module5Service _module5Service;
        public ThongKeController(ApplicationDbContext context, Module5Service module5Service)
        {
            _context = context;
            _module5Service = module5Service;
        }
        public async Task<IActionResult> Index(int? nam)
        {
            var namHienTai = _module5Service.BayGio.Year;
            if (!ModelState.IsValid || nam is < 1900 or > 9998)
            {
                ModelState.AddModelError(nameof(nam), "Năm phải từ 1900 đến 9998.");
                nam = namHienTai;
            }
            var model = new ThongKeViewModel { Nam = nam ?? namHienTai };
            model.ChungChis = await _context.ChungChis.AsNoTracking().OrderBy(c => c.TenChungChi)
                .Select(c => new ThongKeChungChiViewModel
                {
                    Ten = c.TenChungChi,
                    SoKyThi = c.KyThis.Count(),
                    SoDat = c.KyThis.SelectMany(k => k.HoSoDangKyThis).Count(h => h.TrangThai == TrangThaiHoSo.Dat)
                }).ToListAsync();
            model.KyThis = await _context.KyThis.AsNoTracking()
                .Select(k => new ThongKeKyThiViewModel
                {
                    MaKyThi = k.MaKyThi, Ten = k.TenKyThi, TongHoSo = k.HoSoDangKyThis.Count(),
                    SoDuDieuKien = k.HoSoDangKyThis.Count(h => h.TrangThai == TrangThaiHoSo.DuDieuKien),
                    SoQuaKiemTra = k.HoSoDangKyThis.Count(h => h.TrangThai == TrangThaiHoSo.DuDieuKien
                        || h.TrangThai == TrangThaiHoSo.ChoThi || h.TrangThai == TrangThaiHoSo.DaThi
                        || h.TrangThai == TrangThaiHoSo.Dat || h.TrangThai == TrangThaiHoSo.KhongDat),
                    SoKhongDuDieuKien = k.HoSoDangKyThis.Count(h => h.TrangThai == TrangThaiHoSo.KhongDuDieuKien),
                    SoDaThi = k.HoSoDangKyThis.Count(h => h.TrangThai == TrangThaiHoSo.DaThi
                        || h.TrangThai == TrangThaiHoSo.Dat || h.TrangThai == TrangThaiHoSo.KhongDat),
                    SoDat = k.HoSoDangKyThis.Count(h => h.TrangThai == TrangThaiHoSo.Dat)
                }).OrderByDescending(k => k.TongHoSo).ThenBy(k => k.MaKyThi).ToListAsync();
            var trangThais = await _context.HoSoDangKyThis.AsNoTracking().GroupBy(h => h.TrangThai)
                .Select(g => new ThongKeTrangThaiViewModel { TrangThai = g.Key, SoLuong = g.Count() }).ToListAsync();
            model.TrangThais = Enum.GetValues<TrangThaiHoSo>().Select(t => new ThongKeTrangThaiViewModel
                { TrangThai = t, SoLuong = trangThais.FirstOrDefault(x => x.TrangThai == t)?.SoLuong ?? 0 }).ToList();
            var dauNam = new DateTime(model.Nam, 1, 1);
            var cuoiNam = dauNam.AddYears(1);
            var hoSos = await _context.HoSoDangKyThis.AsNoTracking().Where(h => h.NgayNop >= dauNam && h.NgayNop < cuoiNam)
                .GroupBy(h => h.NgayNop.Month).Select(g => new { Thang = g.Key, SoLuong = g.Count() }).ToListAsync();
            var lichThis = await _context.LichThis.AsNoTracking().Where(l => l.ThoiGianBatDau >= dauNam && l.ThoiGianBatDau < cuoiNam)
                .GroupBy(l => l.ThoiGianBatDau.Month).Select(g => new { Thang = g.Key, SoLuong = g.Count() }).ToListAsync();
            model.Thangs = Enumerable.Range(1, 12).Select(t => new ThongKeThangViewModel
            {
                Thang = t, SoHoSo = hoSos.FirstOrDefault(h => h.Thang == t)?.SoLuong ?? 0,
                SoLichThi = lichThis.FirstOrDefault(l => l.Thang == t)?.SoLuong ?? 0
            }).ToList();
            return View(model);
        }
    }
}
