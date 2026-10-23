using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class HoSoDangKyController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HoSoDangKyController(ApplicationDbContext context) => _context = context;
        private int MaTaiKhoan => HttpContext.Session.LayMaTaiKhoan()!.Value;
        private bool LaThiSinh => HttpContext.Session.LaVaiTro(VaiTro.ThiSinh);

        public async Task<IActionResult> Index(string? tuKhoa, TrangThaiHoSo? trangThai, int? trang)
        {
            var query = _context.HoSoDangKyThis.AsNoTracking().AsQueryable();
            if (LaThiSinh) query = query.Where(h => h.ThiSinh!.MaTaiKhoan == MaTaiKhoan);
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim();
                query = query.Where(h => h.ThiSinh!.HoTen.Contains(tk) || h.KyThi!.TenKyThi.Contains(tk)
                    || h.KyThi.ChungChi!.TenChungChi.Contains(tk));
            }
            if (trangThai.HasValue) query = query.Where(h => h.TrangThai == trangThai.Value);

            var tongSo = await query.CountAsync();
            var trangHienTai = PhanTrangViewModel.ChuanHoaTrang(trang, tongSo);
            var danhSach = await query.OrderByDescending(h => h.NgayNop)
                .Skip((trangHienTai - 1) * PhanTrangViewModel.KichThuocMacDinh)
                .Take(PhanTrangViewModel.KichThuocMacDinh)
                .Select(h => new HoSoDangKyDongViewModel
                {
                    MaHoSo = h.MaHoSo, HoTenThiSinh = h.ThiSinh!.HoTen,
                    TenKyThi = h.KyThi!.TenKyThi, TenChungChi = h.KyThi.ChungChi!.TenChungChi,
                    NgayNop = h.NgayNop, TrangThai = h.TrangThai
                }).ToListAsync();

            return View(new HoSoDangKyIndexViewModel
            {
                DanhSach = danhSach, TuKhoa = tuKhoa, TrangThai = trangThai, LaThiSinh = LaThiSinh,
                PhanTrang = new PhanTrangViewModel
                {
                    TrangHienTai = trangHienTai, TongSoBanGhi = tongSo,
                    ThamSo = new() { ["tuKhoa"] = tuKhoa, ["trangThai"] = trangThai?.ToString() }
                }
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var hoSo = await _context.HoSoDangKyThis.AsNoTracking()
                .Include(h => h.ThiSinh).ThenInclude(t => t!.TaiKhoan)
                .Include(h => h.KyThi).ThenInclude(k => k!.ChungChi)
                .Include(h => h.NguoiXuLy)
                .FirstOrDefaultAsync(h => h.MaHoSo == id);
            if (hoSo is null) return NotFound();
            if (LaThiSinh && hoSo.ThiSinh!.MaTaiKhoan != MaTaiKhoan)
                return RedirectToAction("TuChoiTruyCap", "Auth");
            ViewBag.CoTheHuy = LaThiSinh && await CoTheHuyAsync(hoSo);
            return View(hoSo);
        }

        [HttpGet]
        [PhanQuyen(VaiTro.ThiSinh)]
        public async Task<IActionResult> Create(int? maKyThi)
        {
            var thiSinh = await _context.ThiSinhs.AsNoTracking().FirstOrDefaultAsync(t => t.MaTaiKhoan == MaTaiKhoan);
            if (thiSinh is null)
            {
                TempData["LoiThongBao"] = "Vui lòng hoàn thiện thông tin cá nhân trước khi đăng ký dự thi.";
                return RedirectToAction("Edit", "ThiSinh");
            }
            if (thiSinh.TrangThai != TrangThaiThiSinh.HoatDong)
            {
                TempData["LoiThongBao"] = "Hồ sơ thí sinh đang ngừng hoạt động.";
                return RedirectToAction(nameof(Index));
            }
            await NapKyThiDangMoAsync(maKyThi);
            return View(new HoSoDangKyFormViewModel { MaKyThi = maKyThi ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.ThiSinh)]
        public async Task<IActionResult> Create(HoSoDangKyFormViewModel model)
        {
            var thiSinh = await _context.ThiSinhs.FirstOrDefaultAsync(t => t.MaTaiKhoan == MaTaiKhoan);
            if (thiSinh is null || thiSinh.TrangThai != TrangThaiThiSinh.HoatDong)
                return RedirectToAction("Edit", "ThiSinh");

            var bayGio = DateTime.Now;
            var kyThi = await _context.KyThis.FirstOrDefaultAsync(k => k.MaKyThi == model.MaKyThi);
            if (kyThi is null || kyThi.TrangThai != TrangThaiKyThi.DangMoDangKy
                || kyThi.NgayBatDauDangKy > bayGio || kyThi.HanDangKy < bayGio)
                ModelState.AddModelError(nameof(model.MaKyThi), "Kỳ thi không tồn tại hoặc hiện không nhận hồ sơ.");

            if (await _context.HoSoDangKyThis.AnyAsync(h => h.MaThiSinh == thiSinh.MaThiSinh
                && h.MaKyThi == model.MaKyThi
                && h.TrangThai != TrangThaiHoSo.KhongDuDieuKien
                && h.TrangThai != TrangThaiHoSo.DaHuy))
                ModelState.AddModelError(nameof(model.MaKyThi), "Bạn đã có hồ sơ cho kỳ thi này.");

            if (!ModelState.IsValid)
            {
                await NapKyThiDangMoAsync(model.MaKyThi);
                return View(model);
            }

            var hoSo = new HoSoDangKyThi
            {
                MaThiSinh = thiSinh.MaThiSinh, MaKyThi = model.MaKyThi, NgayNop = bayGio,
                MucDichDuThi = model.MucDichDuThi.Trim(), GhiChu = model.GhiChu?.Trim(),
                TrangThai = TrangThaiHoSo.ChoDuyet
            };
            _context.HoSoDangKyThis.Add(hoSo);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Đã nộp hồ sơ đăng ký dự thi.";
            return RedirectToAction(nameof(Details), new { id = hoSo.MaHoSo });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.ThiSinh)]
        public async Task<IActionResult> Huy(int id)
        {
            var hoSo = await _context.HoSoDangKyThis
                .Include(h => h.ThiSinh)
                .Include(h => h.LichThis)
                .FirstOrDefaultAsync(h => h.MaHoSo == id);
            if (hoSo is null) return NotFound();
            if (hoSo.ThiSinh!.MaTaiKhoan != MaTaiKhoan)
                return RedirectToAction("TuChoiTruyCap", "Auth");
            if (!CoTheHuy(hoSo))
            {
                TempData["LoiThongBao"] = "Chỉ có thể hủy hồ sơ đang chờ duyệt, hoặc hồ sơ đủ điều kiện nhưng chưa có lịch thi hiệu lực.";
                return RedirectToAction(nameof(Details), new { id });
            }
            hoSo.TrangThai = TrangThaiHoSo.DaHuy;
            hoSo.GhiChu = string.IsNullOrWhiteSpace(hoSo.GhiChu)
                ? "Thí sinh tự hủy hồ sơ." : hoSo.GhiChu + "\nThí sinh tự hủy hồ sơ.";
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = "Đã hủy hồ sơ đăng ký.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<bool> CoTheHuyAsync(HoSoDangKyThi hoSo)
        {
            if (hoSo.TrangThai == TrangThaiHoSo.ChoDuyet) return true;
            if (hoSo.TrangThai != TrangThaiHoSo.DuDieuKien) return false;
            return !await _context.LichThis.AnyAsync(l => l.MaHoSo == hoSo.MaHoSo
                && l.TrangThai != TrangThaiLichThi.DaHuy);
        }

        private static bool CoTheHuy(HoSoDangKyThi hoSo)
            => hoSo.TrangThai == TrangThaiHoSo.ChoDuyet
                || (hoSo.TrangThai == TrangThaiHoSo.DuDieuKien
                    && hoSo.LichThis.All(l => l.TrangThai == TrangThaiLichThi.DaHuy));

        private async Task NapKyThiDangMoAsync(int? giaTri)
        {
            var bayGio = DateTime.Now;
            var danhSach = await _context.KyThis.AsNoTracking()
                .Where(k => k.TrangThai == TrangThaiKyThi.DangMoDangKy
                    && k.NgayBatDauDangKy <= bayGio && k.HanDangKy >= bayGio)
                .OrderBy(k => k.TenKyThi)
                .Select(k => new { k.MaKyThi, Ten = k.TenKyThi + " - " + k.ChungChi!.TenChungChi })
                .ToListAsync();
            ViewBag.DanhSachKyThi = new SelectList(danhSach, "MaKyThi", "Ten", giaTri);
        }
    }
}
