// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Đăng nhập (EF Core + LINQ), lưu Session, đăng xuất,
//   đổi mật khẩu và trang từ chối truy cập.
// ============================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;
using QuanLyKyThi_UNETI1_TI17A5HN.Filters;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.Services;
using QuanLyKyThi_UNETI1_TI17A5HN.ViewModels;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Controllers
{
    public class AuthController : Controller
    {
        public const string TenCookieSession = ".QuanLyKyThi.Session";

        private readonly ApplicationDbContext _context;
        private readonly IMatKhauService _matKhauService;

        public AuthController(ApplicationDbContext context, IMatKhauService matKhauService)
        {
            _context = context;
            _matKhauService = matKhauService;
        }

        // GET: /Auth/DangNhap
        [HttpGet]
        public IActionResult DangNhap(string? returnUrl = null)
        {
            if (HttpContext.Session.DaDangNhap())
                return RedirectToAction("Index", "Home");

            return View(new DangNhapViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Auth/DangNhap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tenDangNhap = model.TenDangNhap.Trim();

            // LINQ: tìm tài khoản theo tên đăng nhập (SQL Server so sánh không phân biệt hoa/thường).
            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);

            if (taiKhoan is null)
            {
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tài khoản không tồn tại.");
                return View(model);
            }

            var ketQua = _matKhauService.KiemTra(taiKhoan, model.MatKhau);
            if (ketQua == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(nameof(model.MatKhau), "Mật khẩu không đúng.");
                return View(model);
            }

            if (taiKhoan.TrangThai == TrangThaiTaiKhoan.BiKhoa)
            {
                ModelState.AddModelError(string.Empty,
                    "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            // Hash cũ cần nâng cấp thuật toán -> băm lại và lưu.
            if (ketQua == PasswordVerificationResult.SuccessRehashNeeded)
            {
                taiKhoan.MatKhau = _matKhauService.BamMatKhau(taiKhoan, model.MatKhau);
                await _context.SaveChangesAsync();
            }

            // Xóa Session cũ (nếu có) rồi ghi thông tin đăng nhập mới.
            HttpContext.Session.Clear();
            HttpContext.Session.GhiDangNhap(taiKhoan);

            TempData["ThongBao"] = $"Xin chào {taiKhoan.HoTen}!";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        // POST: /Auth/DangXuat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete(TenCookieSession);
            TempData["ThongBao"] = "Bạn đã đăng xuất.";
            return RedirectToAction(nameof(DangNhap));
        }

        // GET: /Auth/DoiMatKhau
        [HttpGet]
        [PhanQuyen]
        public IActionResult DoiMatKhau() => View(new DoiMatKhauViewModel());

        // POST: /Auth/DoiMatKhau
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen]
        public async Task<IActionResult> DoiMatKhau(DoiMatKhauViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var maTaiKhoan = HttpContext.Session.LayMaTaiKhoan()!.Value;
            var taiKhoan = await _context.TaiKhoans.FindAsync(maTaiKhoan);
            if (taiKhoan is null)
                return RedirectToAction(nameof(DangNhap));

            if (_matKhauService.KiemTra(taiKhoan, model.MatKhauHienTai) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(nameof(model.MatKhauHienTai), "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            taiKhoan.MatKhau = _matKhauService.BamMatKhau(taiKhoan, model.MatKhauMoi);
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = "Đổi mật khẩu thành công.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/TuChoiTruyCap
        [HttpGet]
        public IActionResult TuChoiTruyCap()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }
    }
}
