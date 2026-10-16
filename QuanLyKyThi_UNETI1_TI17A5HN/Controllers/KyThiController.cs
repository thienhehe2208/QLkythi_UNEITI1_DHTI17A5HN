using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Controllers
{
    // ============================================================
    // Họ và tên: Nguyễn Tiến Quang
    // Module 2: Quản lý kỳ thi
    // Nội dung: Controller quản lý kỳ thi
    // ============================================================

    public class KyThiController : Controller
    {
        private readonly AppDbContext _context;

        public KyThiController(AppDbContext context)
        {
            _context = context;
        }

        // Nạp danh sách chứng chỉ từ cơ sở dữ liệu
        private async Task NapDanhSachChungChiAsync(int? maChungChi = null)
        {
            var danhSach = await _context.ChungChis
                .AsNoTracking()
                .OrderBy(c => c.TenChungChi)
                .ToListAsync();

            ViewBag.MaChungChi = new SelectList(
                danhSach,
                "MaChungChi",
                "TenChungChi",
                maChungChi);
        }

        // Danh sách kỳ thi
        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.KyThis
                .AsNoTracking()
                .OrderBy(k => k.TenKyThi)
                .ToListAsync();

            return View(danhSach);
        }

        // Chi tiết kỳ thi
        public async Task<IActionResult> Details(int id)
        {
            var kyThi = await _context.KyThis
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.MaKyThi == id);

            if (kyThi == null)
                return NotFound();

            return View(kyThi);
        }

        // Thêm kỳ thi
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await NapDanhSachChungChiAsync();

            return View(new KyThi
            {
                NgayBatDauDangKy = DateTime.Today,
                HanDangKy = DateTime.Today,
                TrangThai = KyThiTrangThai.ChuaMo,
                SoChoThiToiDa = 1
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KyThi kyThi)
        {
            if (kyThi.HanDangKy.Date < kyThi.NgayBatDauDangKy.Date)
            {
                ModelState.AddModelError(
                    nameof(kyThi.HanDangKy),
                    "Hạn đăng ký phải sau hoặc bằng ngày bắt đầu đăng ký.");
            }

            if (!kyThi.DuLieuHopLe())
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Dữ liệu kỳ thi không hợp lệ. Hãy kiểm tra các thông tin bắt buộc.");
            }

            if (!ModelState.IsValid)
            {
                await NapDanhSachChungChiAsync(kyThi.MaChungChi);
                return View(kyThi);
            }

            _context.KyThis.Add(kyThi);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Sửa kỳ thi
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var kyThi = await _context.KyThis.FindAsync(id);

            if (kyThi == null)
                return NotFound();

            await NapDanhSachChungChiAsync(kyThi.MaChungChi);

            return View(kyThi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KyThi kyThi)
        {
            if (id != kyThi.MaKyThi)
                return BadRequest();

            if (kyThi.HanDangKy.Date < kyThi.NgayBatDauDangKy.Date)
            {
                ModelState.AddModelError(
                    nameof(kyThi.HanDangKy),
                    "Hạn đăng ký phải sau hoặc bằng ngày bắt đầu đăng ký.");
            }

            if (!kyThi.DuLieuHopLe())
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Dữ liệu kỳ thi không hợp lệ. Hãy kiểm tra các thông tin bắt buộc.");
            }

            if (!ModelState.IsValid)
            {
                await NapDanhSachChungChiAsync(kyThi.MaChungChi);
                return View(kyThi);
            }

            var kyThiTrongDb = await _context.KyThis.FindAsync(id);

            if (kyThiTrongDb == null)
                return NotFound();

            kyThiTrongDb.TenKyThi = kyThi.TenKyThi;
            kyThiTrongDb.MaChungChi = kyThi.MaChungChi;
            kyThiTrongDb.SoChoThiToiDa = kyThi.SoChoThiToiDa;
            kyThiTrongDb.CapDoThi = kyThi.CapDoThi;
            kyThiTrongDb.DiemDauVaoToiThieu = kyThi.DiemDauVaoToiThieu;
            kyThiTrongDb.NgayBatDauDangKy = kyThi.NgayBatDauDangKy;
            kyThiTrongDb.HanDangKy = kyThi.HanDangKy;
            kyThiTrongDb.MoTaKyThi = kyThi.MoTaKyThi;
            kyThiTrongDb.DieuKienDuThi = kyThi.DieuKienDuThi;
            kyThiTrongDb.TrangThai = kyThi.TrangThai;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Cập nhật riêng trạng thái kỳ thi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int id, string trangThai)
        {
            if (!KyThiTrangThai.TatCa.Contains(trangThai))
            {
                return BadRequest("Trạng thái kỳ thi không hợp lệ.");
            }

            var kyThi = await _context.KyThis.FindAsync(id);

            if (kyThi == null)
                return NotFound();

            kyThi.TrangThai = trangThai;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Xác nhận xóa kỳ thi
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var kyThi = await _context.KyThis
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.MaKyThi == id);

            if (kyThi == null)
                return NotFound();

            return View(kyThi);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kyThi = await _context.KyThis.FindAsync(id);

            if (kyThi == null)
                return NotFound();

            _context.KyThis.Remove(kyThi);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}