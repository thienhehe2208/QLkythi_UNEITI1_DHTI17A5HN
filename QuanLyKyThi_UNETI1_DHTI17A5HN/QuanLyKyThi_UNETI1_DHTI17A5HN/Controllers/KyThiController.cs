// ============================================================
// Họ và tên: Nguyễn Tiến Quang
// Mã sinh viên: 
// Nội dung thực hiện: Quản lý kỳ thi, tìm kiếm, lọc, sắp xếp,
//   phân trang và kiểm soát trạng thái (Module 2).
// ============================================================

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
    public class KyThiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KyThiController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool LaQuanLy => HttpContext.Session.LaVaiTro(VaiTro.Admin, VaiTro.CanBoKhaoThi);

        public async Task<IActionResult> Index(
            string? tuKhoa,
            int? maChungChi,
            string? capDoThi,
            TrangThaiKyThi? trangThai,
            decimal? diemToiDa,
            string? thoiHan,
            string? sapXep,
            int? trang)
        {
            var bayGio = DateTime.Now;
            IQueryable<KyThi> query = _context.KyThis.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim();
                query = query.Where(k => k.TenKyThi.Contains(tk)
                                      || k.ChungChi!.TenChungChi.Contains(tk));
            }

            if (maChungChi.HasValue)
                query = query.Where(k => k.MaChungChi == maChungChi.Value);
            if (!string.IsNullOrWhiteSpace(capDoThi))
                query = query.Where(k => k.CapDoThi == capDoThi);
            if (trangThai.HasValue)
                query = query.Where(k => k.TrangThai == trangThai.Value);
            if (diemToiDa.HasValue)
                query = query.Where(k => k.DiemDauVaoToiThieu <= diemToiDa.Value);

            query = thoiHan switch
            {
                "con_han" => query.Where(k => k.HanDangKy >= bayGio),
                "het_han" => query.Where(k => k.HanDangKy < bayGio),
                _ => query
            };

            query = sapXep switch
            {
                "ten_desc" => query.OrderByDescending(k => k.TenKyThi),
                "han_asc" => query.OrderBy(k => k.HanDangKy),
                "han_desc" => query.OrderByDescending(k => k.HanDangKy),
                "socho_asc" => query.OrderBy(k => k.SoChoThiToiDa),
                "socho_desc" => query.OrderByDescending(k => k.SoChoThiToiDa),
                _ => query.OrderBy(k => k.TenKyThi)
            };

            var tongSo = await query.CountAsync();
            var trangHienTai = PhanTrangViewModel.ChuanHoaTrang(trang, tongSo);
            var danhSach = await query
                .Skip((trangHienTai - 1) * PhanTrangViewModel.KichThuocMacDinh)
                .Take(PhanTrangViewModel.KichThuocMacDinh)
                .Select(k => new KyThiDongViewModel
                {
                    MaKyThi = k.MaKyThi,
                    TenKyThi = k.TenKyThi,
                    TenChungChi = k.ChungChi!.TenChungChi,
                    CapDoThi = k.CapDoThi,
                    SoChoThiToiDa = k.SoChoThiToiDa,
                    DiemDauVaoToiThieu = k.DiemDauVaoToiThieu,
                    NgayBatDauDangKy = k.NgayBatDauDangKy,
                    HanDangKy = k.HanDangKy,
                    TrangThai = k.TrangThai,
                    SoHoSo = k.HoSoDangKyThis.Count(),
                    DangNhanDangKy = k.TrangThai == TrangThaiKyThi.DangMoDangKy
                        && k.NgayBatDauDangKy <= bayGio
                        && k.HanDangKy >= bayGio
                })
                .ToListAsync();

            await NapBoLocAsync(maChungChi, capDoThi);
            return View(new KyThiIndexViewModel
            {
                DanhSach = danhSach,
                TuKhoa = tuKhoa,
                MaChungChi = maChungChi,
                CapDoThi = capDoThi,
                TrangThai = trangThai,
                DiemToiDa = diemToiDa,
                ThoiHan = thoiHan,
                SapXep = sapXep,
                LaQuanLy = LaQuanLy,
                PhanTrang = new PhanTrangViewModel
                {
                    TrangHienTai = trangHienTai,
                    TongSoBanGhi = tongSo,
                    ThamSo = new()
                    {
                        ["tuKhoa"] = tuKhoa,
                        ["maChungChi"] = maChungChi?.ToString(),
                        ["capDoThi"] = capDoThi,
                        ["trangThai"] = trangThai?.ToString(),
                        ["diemToiDa"] = diemToiDa?.ToString(),
                        ["thoiHan"] = thoiHan,
                        ["sapXep"] = sapXep
                    }
                }
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var kyThi = await _context.KyThis
                .AsNoTracking()
                .Include(k => k.ChungChi)
                .FirstOrDefaultAsync(k => k.MaKyThi == id);
            if (kyThi is null) return NotFound();

            ViewBag.LaQuanLy = LaQuanLy;
            ViewBag.SoHoSo = await _context.HoSoDangKyThis.CountAsync(h => h.MaKyThi == id);
            ViewBag.SoGiuCho = await DemSoHoSoGiuChoAsync(id);
            ViewBag.DangNhanDangKy = DangNhanDangKy(kyThi, DateTime.Now);
            return View(kyThi);
        }

        [HttpGet]
        [PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public async Task<IActionResult> Create()
        {
            await NapDanhSachChungChiAsync();
            var ngayMai = DateTime.Today.AddDays(1);
            return View(new KyThiFormViewModel
            {
                NgayBatDauDangKy = ngayMai,
                HanDangKy = ngayMai.AddDays(14),
                SoChoThiToiDa = 1
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public async Task<IActionResult> Create(KyThiFormViewModel model)
        {
            ChuanHoa(model);
            await KiemTraFormAsync(model);
            if (!ModelState.IsValid)
            {
                await NapDanhSachChungChiAsync(model.MaChungChi);
                return View(model);
            }

            var kyThi = new KyThi
            {
                TenKyThi = model.TenKyThi,
                MaChungChi = model.MaChungChi,
                SoChoThiToiDa = model.SoChoThiToiDa,
                CapDoThi = model.CapDoThi,
                DiemDauVaoToiThieu = model.DiemDauVaoToiThieu,
                NgayBatDauDangKy = model.NgayBatDauDangKy,
                HanDangKy = model.HanDangKy,
                MoTaKyThi = model.MoTaKyThi,
                DieuKienDuThi = model.DieuKienDuThi,
                TrangThai = TrangThaiKyThi.ChuaMo
            };
            _context.KyThis.Add(kyThi);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã tạo kỳ thi \"{kyThi.TenKyThi}\" ở trạng thái Chưa mở.";
            return RedirectToAction(nameof(Details), new { id = kyThi.MaKyThi });
        }

        [HttpGet]
        [PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public async Task<IActionResult> Edit(int id)
        {
            var kyThi = await _context.KyThis.AsNoTracking().FirstOrDefaultAsync(k => k.MaKyThi == id);
            if (kyThi is null) return NotFound();
            await NapDanhSachChungChiAsync(kyThi.MaChungChi);
            return View(TaoForm(kyThi));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public async Task<IActionResult> Edit(int id, KyThiFormViewModel model)
        {
            if (id != model.MaKyThi) return BadRequest();
            var kyThi = await _context.KyThis.FirstOrDefaultAsync(k => k.MaKyThi == id);
            if (kyThi is null) return NotFound();

            ChuanHoa(model);
            await KiemTraFormAsync(model);
            var soGiuCho = await DemSoHoSoGiuChoAsync(id);
            if (model.SoChoThiToiDa < soGiuCho)
                ModelState.AddModelError(nameof(model.SoChoThiToiDa), $"Không thể nhỏ hơn {soGiuCho} hồ sơ đang giữ chỗ.");

            var daCoHoSo = await _context.HoSoDangKyThis.AnyAsync(h => h.MaKyThi == id);
            if (daCoHoSo && model.MaChungChi != kyThi.MaChungChi)
                ModelState.AddModelError(nameof(model.MaChungChi), "Kỳ thi đã có hồ sơ nên không thể đổi chứng chỉ.");

            if (!ModelState.IsValid)
            {
                await NapDanhSachChungChiAsync(model.MaChungChi);
                return View(model);
            }

            kyThi.TenKyThi = model.TenKyThi;
            kyThi.MaChungChi = model.MaChungChi;
            kyThi.SoChoThiToiDa = model.SoChoThiToiDa;
            kyThi.CapDoThi = model.CapDoThi;
            kyThi.DiemDauVaoToiThieu = model.DiemDauVaoToiThieu;
            kyThi.NgayBatDauDangKy = model.NgayBatDauDangKy;
            kyThi.HanDangKy = model.HanDangKy;
            kyThi.MoTaKyThi = model.MoTaKyThi;
            kyThi.DieuKienDuThi = model.DieuKienDuThi;
            await _context.SaveChangesAsync();

            TempData["ThongBao"] = $"Đã cập nhật kỳ thi \"{kyThi.TenKyThi}\".";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public async Task<IActionResult> CapNhatTrangThai(int id, TrangThaiKyThi trangThai)
        {
            if (!Enum.IsDefined(trangThai)) return BadRequest();
            var kyThi = await _context.KyThis.Include(k => k.ChungChi).FirstOrDefaultAsync(k => k.MaKyThi == id);
            if (kyThi is null) return NotFound();

            if (trangThai == TrangThaiKyThi.DangMoDangKy)
            {
                if (!DuLieuHopLe(kyThi))
                {
                    TempData["LoiThongBao"] = "Không thể mở đăng ký vì dữ liệu kỳ thi chưa hợp lệ.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                if (kyThi.ChungChi?.TrangThai != TrangThaiChungChi.DangHoatDong)
                {
                    TempData["LoiThongBao"] = "Không thể mở kỳ thi thuộc chứng chỉ đã ngừng hoạt động.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                if (kyThi.HanDangKy < DateTime.Now)
                {
                    TempData["LoiThongBao"] = "Không thể mở đăng ký vì kỳ thi đã hết hạn nhận hồ sơ.";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }

            kyThi.TrangThai = trangThai;
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã chuyển kỳ thi sang trạng thái {trangThai.HienThi()}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        [PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public async Task<IActionResult> Delete(int id)
        {
            var kyThi = await _context.KyThis.AsNoTracking().Include(k => k.ChungChi)
                .FirstOrDefaultAsync(k => k.MaKyThi == id);
            if (kyThi is null) return NotFound();
            ViewBag.SoHoSo = await _context.HoSoDangKyThis.CountAsync(h => h.MaKyThi == id);
            return View(kyThi);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kyThi = await _context.KyThis.FindAsync(id);
            if (kyThi is null) return NotFound();
            if (await _context.HoSoDangKyThis.AnyAsync(h => h.MaKyThi == id))
            {
                TempData["LoiThongBao"] = "Không thể xóa kỳ thi đã có hồ sơ đăng ký. Hãy đóng kỳ thi để giữ dữ liệu lịch sử.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.KyThis.Remove(kyThi);
            await _context.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã xóa kỳ thi \"{kyThi.TenKyThi}\".";
            return RedirectToAction(nameof(Index));
        }

        private async Task NapBoLocAsync(int? maChungChi, string? capDoThi)
        {
            var chungChis = await _context.ChungChis.AsNoTracking().OrderBy(c => c.TenChungChi).ToListAsync();
            ViewBag.DanhSachChungChi = new SelectList(chungChis, "MaChungChi", "TenChungChi", maChungChi);
            var capDos = await _context.KyThis.AsNoTracking().Select(k => k.CapDoThi).Distinct().OrderBy(x => x).ToListAsync();
            ViewBag.DanhSachCapDo = new SelectList(capDos, capDoThi);
        }

        private async Task NapDanhSachChungChiAsync(int? maChungChi = null)
        {
            var chungChis = await _context.ChungChis.AsNoTracking()
                .Where(c => c.TrangThai == TrangThaiChungChi.DangHoatDong || c.MaChungChi == maChungChi)
                .OrderBy(c => c.TenChungChi).ToListAsync();
            ViewBag.DanhSachChungChi = new SelectList(chungChis, "MaChungChi", "TenChungChi", maChungChi);
        }

        private async Task KiemTraFormAsync(KyThiFormViewModel model)
        {
            if (model.HanDangKy < model.NgayBatDauDangKy)
                ModelState.AddModelError(nameof(model.HanDangKy), "Hạn đăng ký phải sau hoặc bằng ngày bắt đầu đăng ký.");
            if (!await _context.ChungChis.AnyAsync(c => c.MaChungChi == model.MaChungChi))
                ModelState.AddModelError(nameof(model.MaChungChi), "Chứng chỉ không tồn tại.");
        }

        private async Task<int> DemSoHoSoGiuChoAsync(int maKyThi)
        {
            return await _context.HoSoDangKyThis.CountAsync(h => h.MaKyThi == maKyThi
                && (h.TrangThai == TrangThaiHoSo.DuDieuKien
                 || h.TrangThai == TrangThaiHoSo.ChoThi
                 || h.TrangThai == TrangThaiHoSo.DaThi
                 || h.TrangThai == TrangThaiHoSo.Dat
                 || h.TrangThai == TrangThaiHoSo.KhongDat));
        }

        private static bool DangNhanDangKy(KyThi kyThi, DateTime ngay)
            => kyThi.TrangThai == TrangThaiKyThi.DangMoDangKy
               && kyThi.NgayBatDauDangKy <= ngay && kyThi.HanDangKy >= ngay;

        private static bool DuLieuHopLe(KyThi kyThi)
            => !string.IsNullOrWhiteSpace(kyThi.TenKyThi)
               && !string.IsNullOrWhiteSpace(kyThi.CapDoThi)
               && kyThi.MaChungChi > 0 && kyThi.SoChoThiToiDa > 0
               && kyThi.DiemDauVaoToiThieu >= 0
               && kyThi.HanDangKy >= kyThi.NgayBatDauDangKy;

        private static void ChuanHoa(KyThiFormViewModel model)
        {
            model.TenKyThi = string.Join(' ', (model.TenKyThi ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries));
            model.CapDoThi = model.CapDoThi?.Trim() ?? string.Empty;
            model.MoTaKyThi = string.IsNullOrWhiteSpace(model.MoTaKyThi) ? null : model.MoTaKyThi.Trim();
            model.DieuKienDuThi = string.IsNullOrWhiteSpace(model.DieuKienDuThi) ? null : model.DieuKienDuThi.Trim();
        }

        private static KyThiFormViewModel TaoForm(KyThi k) => new()
        {
            MaKyThi = k.MaKyThi,
            TenKyThi = k.TenKyThi,
            MaChungChi = k.MaChungChi,
            SoChoThiToiDa = k.SoChoThiToiDa,
            CapDoThi = k.CapDoThi,
            DiemDauVaoToiThieu = k.DiemDauVaoToiThieu,
            NgayBatDauDangKy = k.NgayBatDauDangKy,
            HanDangKy = k.HanDangKy,
            MoTaKyThi = k.MoTaKyThi,
            DieuKienDuThi = k.DieuKienDuThi
        };
    }
}
