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
    [PhanQuyen]
    public class ThiSinhController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ThiSinhController(ApplicationDbContext context) => _context = context;

        private int MaTaiKhoan => HttpContext.Session.LayMaTaiKhoan()!.Value;
        private bool LaThiSinh => HttpContext.Session.LaVaiTro(VaiTro.ThiSinh);

        [PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public async Task<IActionResult> Index(string? tuKhoa, TrangThaiThiSinh? trangThai, int? trang)
        {
            var query = _context.ThiSinhs.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim();
                query = query.Where(t => t.HoTen.Contains(tk) || t.Email.Contains(tk)
                    || t.SoDienThoai.Contains(tk) || t.TaiKhoan!.TenDangNhap.Contains(tk));
            }
            if (trangThai.HasValue) query = query.Where(t => t.TrangThai == trangThai.Value);

            var tongSo = await query.CountAsync();
            var trangHienTai = PhanTrangViewModel.ChuanHoaTrang(trang, tongSo);
            var danhSach = await query.OrderBy(t => t.HoTen)
                .Skip((trangHienTai - 1) * PhanTrangViewModel.KichThuocMacDinh)
                .Take(PhanTrangViewModel.KichThuocMacDinh)
                .Select(t => new ThiSinhDongViewModel
                {
                    MaThiSinh = t.MaThiSinh,
                    HoTen = t.HoTen,
                    TenDangNhap = t.TaiKhoan!.TenDangNhap,
                    Email = t.Email,
                    SoDienThoai = t.SoDienThoai,
                    TrangThai = t.TrangThai,
                    SoHoSo = t.HoSoDangKyThis.Count
                }).ToListAsync();

            return View(new ThiSinhIndexViewModel
            {
                DanhSach = danhSach,
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                PhanTrang = new PhanTrangViewModel
                {
                    TrangHienTai = trangHienTai,
                    TongSoBanGhi = tongSo,
                    ThamSo = new() { ["tuKhoa"] = tuKhoa, ["trangThai"] = trangThai?.ToString() }
                }
            });
        }

        public async Task<IActionResult> HoSoCuaToi()
        {
            if (!LaThiSinh) return RedirectToAction(nameof(Index));
            var maThiSinh = await _context.ThiSinhs.Where(t => t.MaTaiKhoan == MaTaiKhoan)
                .Select(t => (int?)t.MaThiSinh).FirstOrDefaultAsync();
            return maThiSinh.HasValue
                ? RedirectToAction(nameof(Details), new { id = maThiSinh.Value })
                : RedirectToAction(nameof(Edit));
        }

        public async Task<IActionResult> Details(int id)
        {
            var thiSinh = await _context.ThiSinhs.AsNoTracking().Include(t => t.TaiKhoan)
                .FirstOrDefaultAsync(t => t.MaThiSinh == id);
            if (thiSinh is null) return NotFound();
            if (LaThiSinh && thiSinh.MaTaiKhoan != MaTaiKhoan)
                return RedirectToAction("TuChoiTruyCap", "Auth");
            ViewBag.SoHoSo = await _context.HoSoDangKyThis.CountAsync(h => h.MaThiSinh == id);
            ViewBag.CoTheSua = LaThiSinh;
            return View(thiSinh);
        }

        [HttpGet]
        [PhanQuyen(VaiTro.ThiSinh)]
        public async Task<IActionResult> Edit()
        {
            var thiSinh = await _context.ThiSinhs.AsNoTracking()
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == MaTaiKhoan);

            if (thiSinh is null)
            {
                var taiKhoan = await _context.TaiKhoans.AsNoTracking().FirstAsync(t => t.MaTaiKhoan == MaTaiKhoan);
                return View(new ThiSinhFormViewModel
                {
                    HoTen = taiKhoan.HoTen, Email = taiKhoan.Email,
                    NgaySinh = DateTime.Today.AddYears(-18), TrangThai = TrangThaiThiSinh.HoatDong
                });
            }

            return View(new ThiSinhFormViewModel
            {
                MaThiSinh = thiSinh.MaThiSinh, HoTen = thiSinh.HoTen, NgaySinh = thiSinh.NgaySinh,
                GioiTinh = thiSinh.GioiTinh, SoDienThoai = thiSinh.SoDienThoai, Email = thiSinh.Email,
                DiaChi = thiSinh.DiaChi, TrinhDoHienTai = thiSinh.TrinhDoHienTai,
                NgheNghiep = thiSinh.NgheNghiep, SoLanThiTruocDay = thiSinh.SoLanThiTruocDay,
                ChungChiDaCo = thiSinh.ChungChiDaCo, TrangThai = thiSinh.TrangThai
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.ThiSinh)]
        public async Task<IActionResult> Edit(ThiSinhFormViewModel model)
        {
            var taiKhoan = await _context.TaiKhoans.FirstAsync(t => t.MaTaiKhoan == MaTaiKhoan);
            var thiSinh = await _context.ThiSinhs.FirstOrDefaultAsync(t => t.MaTaiKhoan == MaTaiKhoan);
            if (model.NgaySinh > DateTime.Today || model.NgaySinh < DateTime.Today.AddYears(-100))
                ModelState.AddModelError(nameof(model.NgaySinh), "Ngày sinh không hợp lệ.");
            if (!ModelState.IsValid) return View(model);

            if (thiSinh is null)
            {
                thiSinh = new ThiSinh { MaTaiKhoan = MaTaiKhoan };
                _context.ThiSinhs.Add(thiSinh);
            }
            thiSinh.HoTen = model.HoTen.Trim();
            thiSinh.NgaySinh = model.NgaySinh.Date;
            thiSinh.GioiTinh = model.GioiTinh;
            thiSinh.SoDienThoai = model.SoDienThoai.Trim();
            thiSinh.Email = model.Email.Trim();
            thiSinh.DiaChi = model.DiaChi?.Trim();
            thiSinh.TrinhDoHienTai = model.TrinhDoHienTai?.Trim();
            thiSinh.NgheNghiep = model.NgheNghiep?.Trim();
            thiSinh.SoLanThiTruocDay = model.SoLanThiTruocDay;
            thiSinh.ChungChiDaCo = model.ChungChiDaCo?.Trim();
            taiKhoan.HoTen = thiSinh.HoTen;
            taiKhoan.Email = thiSinh.Email;

            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Đã cập nhật thông tin thí sinh.";
            return RedirectToAction(nameof(Details), new { id = thiSinh.MaThiSinh });
        }
    }
}
