// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Quản lý chứng chỉ.
//   - Mọi người dùng đã đăng nhập: xem danh sách/chi tiết (không phải Admin chỉ thấy chứng chỉ đang hoạt động).
//   - Admin: thêm, sửa, xóa, cập nhật trạng thái.
//   - Tên chứng chỉ không trùng; không xóa chứng chỉ đã có kỳ thi tham chiếu.
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
    [PhanQuyen]
    public class ChungChiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChungChiController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool LaAdmin => HttpContext.Session.LaVaiTro(VaiTro.Admin);

        // GET: /ChungChi?tuKhoa=&trangThai=&sapXep=&trang=
        public async Task<IActionResult> Index(string? tuKhoa, TrangThaiChungChi? trangThai, string? sapXep, int? trang)
        {
            IQueryable<ChungChi> query = _context.ChungChis.AsNoTracking();

            // Người không phải Admin chỉ xem chứng chỉ đang hoạt động (thông tin công khai).
            if (!LaAdmin)
                trangThai = TrangThaiChungChi.DangHoatDong;

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim();
                query = query.Where(c => c.TenChungChi.Contains(tk)
                                      || (c.MoTa != null && c.MoTa.Contains(tk)));
            }

            if (trangThai.HasValue)
                query = query.Where(c => c.TrangThai == trangThai.Value);

            query = sapXep switch
            {
                "ten_desc" => query.OrderByDescending(c => c.TenChungChi),
                "sokythi_desc" => query.OrderByDescending(c => c.KyThis.Count()).ThenBy(c => c.TenChungChi),
                _ => query.OrderBy(c => c.TenChungChi)
            };

            var tongSo = await query.CountAsync();
            var trangHienTai = PhanTrangViewModel.ChuanHoaTrang(trang, tongSo);

            var danhSach = await query
                .Skip((trangHienTai - 1) * PhanTrangViewModel.KichThuocMacDinh)
                .Take(PhanTrangViewModel.KichThuocMacDinh)
                .Select(c => new ChungChiDongViewModel
                {
                    MaChungChi = c.MaChungChi,
                    TenChungChi = c.TenChungChi,
                    EmailLienHe = c.EmailLienHe,
                    TrangThai = c.TrangThai,
                    SoKyThi = c.KyThis.Count()
                })
                .ToListAsync();

            return View(new ChungChiIndexViewModel
            {
                DanhSach = danhSach,
                TuKhoa = tuKhoa,
                TrangThai = LaAdmin ? trangThai : null,
                SapXep = sapXep,
                LaAdmin = LaAdmin,
                PhanTrang = new PhanTrangViewModel
                {
                    TrangHienTai = trangHienTai,
                    TongSoBanGhi = tongSo,
                    ThamSo = new()
                    {
                        ["tuKhoa"] = tuKhoa,
                        ["trangThai"] = LaAdmin ? trangThai?.ToString() : null,
                        ["sapXep"] = sapXep
                    }
                }
            });
        }

        // GET: /ChungChi/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var model = await _context.ChungChis
                .AsNoTracking()
                .Where(c => c.MaChungChi == id)
                .Select(c => new ChungChiChiTietViewModel
                {
                    MaChungChi = c.MaChungChi,
                    TenChungChi = c.TenChungChi,
                    MoTa = c.MoTa,
                    EmailLienHe = c.EmailLienHe,
                    TrangThai = c.TrangThai,
                    TongSoKyThi = c.KyThis.Count(),
                    SoKyThiDangMo = c.KyThis.Count(k => k.TrangThai == TrangThaiKyThi.DangMoDangKy),
                    KyThiGanDay = c.KyThis
                        .OrderByDescending(k => k.HanDangKy)
                        .Take(10)
                        .Select(k => new KyThiLienQuanViewModel
                        {
                            MaKyThi = k.MaKyThi,
                            TenKyThi = k.TenKyThi,
                            CapDoThi = k.CapDoThi,
                            HanDangKy = k.HanDangKy,
                            TrangThai = k.TrangThai
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (model is null) return NotFound();
            if (!LaAdmin && model.TrangThai != TrangThaiChungChi.DangHoatDong) return NotFound();

            model.LaAdmin = LaAdmin;
            return View(model);
        }

        // GET: /ChungChi/Create
        [PhanQuyen(VaiTro.Admin)]
        public IActionResult Create() => View(new ChungChiFormViewModel());

        // POST: /ChungChi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> Create(ChungChiFormViewModel model)
        {
            ChuanHoa(model);
            await KiemTraTrungTenAsync(model);

            if (!ModelState.IsValid)
                return View(model);

            var chungChi = new ChungChi
            {
                TenChungChi = model.TenChungChi,
                MoTa = model.MoTa,
                EmailLienHe = model.EmailLienHe,
                TrangThai = TrangThaiChungChi.DangHoatDong // chứng chỉ mới luôn ở trạng thái hoạt động
            };
            _context.ChungChis.Add(chungChi);

            if (!await LuuAnToanAsync(model))
                return View(model);

            TempData["ThongBao"] = $"Đã thêm chứng chỉ \"{chungChi.TenChungChi}\".";
            return RedirectToAction(nameof(Index));
        }

        // GET: /ChungChi/Edit/5
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> Edit(int id)
        {
            var chungChi = await _context.ChungChis.AsNoTracking().FirstOrDefaultAsync(c => c.MaChungChi == id);
            if (chungChi is null) return NotFound();

            return View(new ChungChiFormViewModel
            {
                MaChungChi = chungChi.MaChungChi,
                TenChungChi = chungChi.TenChungChi,
                MoTa = chungChi.MoTa,
                EmailLienHe = chungChi.EmailLienHe,
                TrangThai = chungChi.TrangThai
            });
        }

        // POST: /ChungChi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> Edit(int id, ChungChiFormViewModel model)
        {
            if (id != model.MaChungChi) return BadRequest();

            var chungChi = await _context.ChungChis.FindAsync(id);
            if (chungChi is null) return NotFound();

            model.TrangThai = chungChi.TrangThai; // trạng thái không đổi qua form sửa
            ChuanHoa(model);
            await KiemTraTrungTenAsync(model);

            if (!ModelState.IsValid)
                return View(model);

            chungChi.TenChungChi = model.TenChungChi;
            chungChi.MoTa = model.MoTa;
            chungChi.EmailLienHe = model.EmailLienHe;

            if (!await LuuAnToanAsync(model))
                return View(model);

            TempData["ThongBao"] = $"Đã cập nhật chứng chỉ \"{chungChi.TenChungChi}\".";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /ChungChi/CapNhatTrangThai/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> CapNhatTrangThai(int id, TrangThaiChungChi trangThai)
        {
            if (!Enum.IsDefined(trangThai)) return BadRequest();

            var chungChi = await _context.ChungChis.FindAsync(id);
            if (chungChi is null) return NotFound();

            if (chungChi.TrangThai == trangThai)
                return RedirectToAction(nameof(Details), new { id });

            if (trangThai == TrangThaiChungChi.NgungHoatDong)
            {
                // Không ngừng chứng chỉ khi còn kỳ thi đang mở nhận hồ sơ.
                var soKyThiDangMo = await _context.KyThis
                    .CountAsync(k => k.MaChungChi == id && k.TrangThai == TrangThaiKyThi.DangMoDangKy);
                if (soKyThiDangMo > 0)
                {
                    TempData["LoiThongBao"] =
                        $"Chứng chỉ đang có {soKyThiDangMo} kỳ thi ở trạng thái \"Đang mở đăng ký\". " +
                        "Hãy tạm dừng hoặc đóng các kỳ thi đó trước khi ngừng hoạt động chứng chỉ.";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }

            chungChi.TrangThai = trangThai;
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = $"Đã chuyển chứng chỉ \"{chungChi.TenChungChi}\" sang trạng thái \"{trangThai.HienThi()}\".";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /ChungChi/Delete/5
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var chungChi = await _context.ChungChis.AsNoTracking().FirstOrDefaultAsync(c => c.MaChungChi == id);
            if (chungChi is null) return NotFound();

            ViewBag.SoKyThi = await _context.KyThis.CountAsync(k => k.MaChungChi == id);
            return View(chungChi);
        }

        // POST: /ChungChi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chungChi = await _context.ChungChis.FindAsync(id);
            if (chungChi is null) return NotFound();

            // LINQ: kiểm tra còn kỳ thi tham chiếu -> không cho xóa để kỳ thi không mất chứng chỉ.
            var soKyThi = await _context.KyThis.CountAsync(k => k.MaChungChi == id);
            if (soKyThi > 0)
            {
                TempData["LoiThongBao"] =
                    $"Không thể xóa: chứng chỉ đang được {soKyThi} kỳ thi sử dụng. " +
                    "Hãy chuyển sang \"Ngừng hoạt động\" thay vì xóa.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.ChungChis.Remove(chungChi);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // FK Restrict chặn nếu có kỳ thi vừa được tạo trong lúc xóa.
                TempData["LoiThongBao"] = "Không thể xóa chứng chỉ vì đang có dữ liệu liên quan.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["ThongBao"] = $"Đã xóa chứng chỉ \"{chungChi.TenChungChi}\".";
            return RedirectToAction(nameof(Index));
        }

        // ================= Hàm hỗ trợ =================

        private static void ChuanHoa(ChungChiFormViewModel model)
        {
            model.TenChungChi = string.Join(' ',
                (model.TenChungChi ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries));
            model.MoTa = string.IsNullOrWhiteSpace(model.MoTa) ? null : model.MoTa.Trim();
            model.EmailLienHe = string.IsNullOrWhiteSpace(model.EmailLienHe) ? null : model.EmailLienHe.Trim();
        }

        /// <summary>LINQ: tên chứng chỉ không trùng (không phân biệt hoa/thường, bỏ qua chính nó khi sửa).</summary>
        private async Task KiemTraTrungTenAsync(ChungChiFormViewModel model)
        {
            if (string.IsNullOrEmpty(model.TenChungChi)) return;

            var ten = model.TenChungChi.ToLower();
            var biTrung = await _context.ChungChis.AnyAsync(c => c.TenChungChi.ToLower() == ten
                                                              && c.MaChungChi != model.MaChungChi);
            if (biTrung)
                ModelState.AddModelError(nameof(model.TenChungChi), "Tên chứng chỉ đã tồn tại.");
        }

        private async Task<bool> LuuAnToanAsync(ChungChiFormViewModel model)
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(nameof(model.TenChungChi), "Tên chứng chỉ đã tồn tại.");
                return false;
            }
        }
    }
}
