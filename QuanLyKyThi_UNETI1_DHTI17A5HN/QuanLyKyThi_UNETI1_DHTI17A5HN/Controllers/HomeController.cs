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

        public async Task<IActionResult> Index()
        {
            // Khách chưa đăng nhập -> vào thẳng trang đăng nhập.
            if (!HttpContext.Session.DaDangNhap())
                return RedirectToAction("DangNhap", "Auth");

            var model = new TrangChuViewModel
            {
                SoChungChiHoatDong = await _context.ChungChis.CountAsync(c => c.TrangThai == TrangThaiChungChi.DangHoatDong),
                SoKyThi = await _context.KyThis.CountAsync()
            };

            if (HttpContext.Session.LaVaiTro(VaiTro.Admin))
            {
                model.SoTaiKhoan = await _context.TaiKhoans.CountAsync();
                model.SoTaiKhoanBiKhoa = await _context.TaiKhoans.CountAsync(t => t.TrangThai == TrangThaiTaiKhoan.BiKhoa);
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
