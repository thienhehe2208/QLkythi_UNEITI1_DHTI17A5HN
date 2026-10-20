// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Quản lý tài khoản (chỉ Admin): danh sách có tìm kiếm/lọc/phân trang,
//   chi tiết, thêm, sửa, đặt lại mật khẩu, khóa/mở khóa, xóa có kiểm tra ràng buộc.
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
    [PhanQuyen(VaiTro.Admin)]
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMatKhauService _matKhauService;

        public TaiKhoanController(ApplicationDbContext context, IMatKhauService matKhauService)
        {
            _context = context;
            _matKhauService = matKhauService;
        }

        private int MaTaiKhoanDangNhap => HttpContext.Session.LayMaTaiKhoan()!.Value;

        // GET: /TaiKhoan?tuKhoa=&vaiTro=&trangThai=&trang=
        public async Task<IActionResult> Index(string? tuKhoa, VaiTro? vaiTro, TrangThaiTaiKhoan? trangThai, int? trang)
        {
            IQueryable<TaiKhoan> query = _context.TaiKhoans.AsNoTracking();

            // Tìm kiếm theo tên đăng nhập / họ tên / email
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim();
                query = query.Where(t => t.TenDangNhap.Contains(tk)
                                      || t.HoTen.Contains(tk)
                                      || t.Email.Contains(tk));
            }

            // Lọc
            if (vaiTro.HasValue)
                query = query.Where(t => t.VaiTro == vaiTro.Value);
            if (trangThai.HasValue)
                query = query.Where(t => t.TrangThai == trangThai.Value);

            // Sắp xếp -> đếm -> phân trang trên server
            query = query.OrderBy(t => t.VaiTro).ThenBy(t => t.TenDangNhap);

            var tongSo = await query.CountAsync();
            var trangHienTai = PhanTrangViewModel.ChuanHoaTrang(trang, tongSo);

            var danhSach = await query
                .Skip((trangHienTai - 1) * PhanTrangViewModel.KichThuocMacDinh)
                .Take(PhanTrangViewModel.KichThuocMacDinh)
                .Select(t => new TaiKhoanDongViewModel
                {
                    MaTaiKhoan = t.MaTaiKhoan,
                    TenDangNhap = t.TenDangNhap,
                    HoTen = t.HoTen,
                    Email = t.Email,
                    VaiTro = t.VaiTro,
                    TrangThai = t.TrangThai,
                    CoHoSoThiSinh = t.ThiSinh != null
                })
                .ToListAsync();

            var model = new TaiKhoanIndexViewModel
            {
                DanhSach = danhSach,
                TuKhoa = tuKhoa,
                VaiTro = vaiTro,
                TrangThai = trangThai,
                PhanTrang = new PhanTrangViewModel
                {
                    TrangHienTai = trangHienTai,
                    TongSoBanGhi = tongSo,
                    ThamSo = new()
                    {
                        ["tuKhoa"] = tuKhoa,
                        ["vaiTro"] = vaiTro?.ToString(),
                        ["trangThai"] = trangThai?.ToString()
                    }
                }
            };
            return View(model);
        }

        // GET: /TaiKhoan/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var taiKhoan = await _context.TaiKhoans
                .AsNoTracking()
                .Include(t => t.ThiSinh)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan is null) return NotFound();

            ViewBag.LaChinhMinh = taiKhoan.MaTaiKhoan == MaTaiKhoanDangNhap;
            return View(taiKhoan);
        }

        // GET: /TaiKhoan/Create
        public IActionResult Create() => View(new TaiKhoanTaoViewModel());

        // POST: /TaiKhoan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoanTaoViewModel model)
        {
            model.TenDangNhap = model.TenDangNhap?.Trim() ?? string.Empty;
            model.HoTen = model.HoTen?.Trim() ?? string.Empty;
            model.Email = model.Email?.Trim() ?? string.Empty;

            if (await TenDangNhapDaTonTaiAsync(model.TenDangNhap))
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại.");

            if (!ModelState.IsValid)
                return View(model);

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = model.TenDangNhap,
                HoTen = model.HoTen,
                Email = model.Email,
                VaiTro = model.VaiTro!.Value,
                TrangThai = TrangThaiTaiKhoan.HoatDong
            };
            taiKhoan.MatKhau = _matKhauService.BamMatKhau(taiKhoan, model.MatKhau);

            _context.TaiKhoans.Add(taiKhoan);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Phòng trường hợp 2 người tạo cùng lúc vượt qua bước kiểm tra -> unique index chặn.
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại.");
                return View(model);
            }

            TempData["ThongBao"] = $"Đã tạo tài khoản \"{taiKhoan.TenDangNhap}\".";
            return RedirectToAction(nameof(Index));
        }

        // GET: /TaiKhoan/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var taiKhoan = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan is null) return NotFound();

            return View(new TaiKhoanSuaViewModel
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TenDangNhap = taiKhoan.TenDangNhap,
                HoTen = taiKhoan.HoTen,
                Email = taiKhoan.Email,
                VaiTro = taiKhoan.VaiTro,
                TrangThai = taiKhoan.TrangThai
            });
        }

        // POST: /TaiKhoan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaiKhoanSuaViewModel model)
        {
            if (id != model.MaTaiKhoan) return BadRequest();

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.ThiSinh)
                .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan is null) return NotFound();

            // Các trường chỉ hiển thị lấy lại từ DB, không tin dữ liệu form.
            model.TenDangNhap = taiKhoan.TenDangNhap;
            model.TrangThai = taiKhoan.TrangThai;
            model.HoTen = model.HoTen?.Trim() ?? string.Empty;
            model.Email = model.Email?.Trim() ?? string.Empty;

            if (model.VaiTro.HasValue && model.VaiTro != taiKhoan.VaiTro)
            {
                if (taiKhoan.MaTaiKhoan == MaTaiKhoanDangNhap)
                    ModelState.AddModelError(nameof(model.VaiTro), "Không thể tự thay đổi vai trò của chính mình.");
                else if (taiKhoan.ThiSinh != null)
                    ModelState.AddModelError(nameof(model.VaiTro),
                        "Tài khoản đã có hồ sơ thí sinh nên không thể đổi sang vai trò khác.");
                else if (taiKhoan.VaiTro == VaiTro.Admin && await LaAdminHoatDongCuoiCungAsync(taiKhoan.MaTaiKhoan))
                    ModelState.AddModelError(nameof(model.VaiTro), "Hệ thống phải còn ít nhất một Admin đang hoạt động.");
            }

            if (!ModelState.IsValid)
                return View(model);

            taiKhoan.HoTen = model.HoTen;
            taiKhoan.Email = model.Email;
            taiKhoan.VaiTro = model.VaiTro!.Value;
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = $"Đã cập nhật tài khoản \"{taiKhoan.TenDangNhap}\".";
            return RedirectToAction(nameof(Index));
        }

        // GET: /TaiKhoan/DatLaiMatKhau/5
        public async Task<IActionResult> DatLaiMatKhau(int id)
        {
            var taiKhoan = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan is null) return NotFound();

            return View(new DatLaiMatKhauViewModel { MaTaiKhoan = id, TenDangNhap = taiKhoan.TenDangNhap });
        }

        // POST: /TaiKhoan/DatLaiMatKhau/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatLaiMatKhau(int id, DatLaiMatKhauViewModel model)
        {
            if (id != model.MaTaiKhoan) return BadRequest();

            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan is null) return NotFound();
            model.TenDangNhap = taiKhoan.TenDangNhap;

            if (!ModelState.IsValid)
                return View(model);

            taiKhoan.MatKhau = _matKhauService.BamMatKhau(taiKhoan, model.MatKhauMoi);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = $"Đã đặt lại mật khẩu cho \"{taiKhoan.TenDangNhap}\".";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /TaiKhoan/KhoaMoKhoa/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KhoaMoKhoa(int id)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan is null) return NotFound();

            if (taiKhoan.TrangThai == TrangThaiTaiKhoan.HoatDong)
            {
                if (taiKhoan.MaTaiKhoan == MaTaiKhoanDangNhap)
                {
                    TempData["LoiThongBao"] = "Không thể tự khóa tài khoản đang đăng nhập.";
                    return RedirectToAction(nameof(Index));
                }
                if (taiKhoan.VaiTro == VaiTro.Admin && await LaAdminHoatDongCuoiCungAsync(taiKhoan.MaTaiKhoan))
                {
                    TempData["LoiThongBao"] = "Không thể khóa Admin đang hoạt động cuối cùng.";
                    return RedirectToAction(nameof(Index));
                }
                taiKhoan.TrangThai = TrangThaiTaiKhoan.BiKhoa;
                TempData["ThongBao"] = $"Đã khóa tài khoản \"{taiKhoan.TenDangNhap}\". Phiên đăng nhập hiện tại của tài khoản này sẽ bị vô hiệu.";
            }
            else
            {
                taiKhoan.TrangThai = TrangThaiTaiKhoan.HoatDong;
                TempData["ThongBao"] = $"Đã mở khóa tài khoản \"{taiKhoan.TenDangNhap}\".";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /TaiKhoan/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var taiKhoan = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(t => t.MaTaiKhoan == id);
            if (taiKhoan is null) return NotFound();

            ViewBag.LyDoKhongXoa = await LyDoKhongTheXoaAsync(taiKhoan);
            return View(taiKhoan);
        }

        // POST: /TaiKhoan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan is null) return NotFound();

            var lyDo = await LyDoKhongTheXoaAsync(taiKhoan);
            if (lyDo != null)
            {
                TempData["LoiThongBao"] = lyDo;
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.TaiKhoans.Remove(taiKhoan);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = $"Đã xóa tài khoản \"{taiKhoan.TenDangNhap}\".";
            return RedirectToAction(nameof(Index));
        }

        // ================= Hàm kiểm tra nghiệp vụ (LINQ) =================

        private Task<bool> TenDangNhapDaTonTaiAsync(string tenDangNhap, int? boQuaMa = null)
        {
            var ten = tenDangNhap.Trim().ToLower();
            return _context.TaiKhoans.AnyAsync(t => t.TenDangNhap.ToLower() == ten
                                                 && (boQuaMa == null || t.MaTaiKhoan != boQuaMa));
        }

        /// <summary>True nếu tài khoản này là Admin hoạt động duy nhất còn lại.</summary>
        private async Task<bool> LaAdminHoatDongCuoiCungAsync(int maTaiKhoan)
        {
            return !await _context.TaiKhoans.AnyAsync(t => t.MaTaiKhoan != maTaiKhoan
                                                         && t.VaiTro == VaiTro.Admin
                                                         && t.TrangThai == TrangThaiTaiKhoan.HoatDong);
        }

        /// <summary>Trả về lý do không được xóa (null nếu được xóa).</summary>
        private async Task<string?> LyDoKhongTheXoaAsync(TaiKhoan taiKhoan)
        {
            if (taiKhoan.MaTaiKhoan == MaTaiKhoanDangNhap)
                return "Không thể xóa tài khoản đang đăng nhập.";

            if (taiKhoan.VaiTro == VaiTro.Admin
                && taiKhoan.TrangThai == TrangThaiTaiKhoan.HoatDong
                && await LaAdminHoatDongCuoiCungAsync(taiKhoan.MaTaiKhoan))
                return "Không thể xóa Admin đang hoạt động cuối cùng.";

            if (await _context.ThiSinhs.AnyAsync(t => t.MaTaiKhoan == taiKhoan.MaTaiKhoan))
                return "Tài khoản đã có hồ sơ thí sinh. Hãy khóa tài khoản thay vì xóa để giữ lịch sử.";

            if (await _context.HoSoDangKyThis.AnyAsync(h => h.MaNguoiXuLy == taiKhoan.MaTaiKhoan))
                return "Tài khoản đã xử lý hồ sơ đăng ký. Hãy khóa tài khoản thay vì xóa để giữ lịch sử.";

            return null;
        }
    }
}
