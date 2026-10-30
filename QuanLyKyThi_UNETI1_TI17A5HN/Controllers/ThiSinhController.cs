// Họ và tên: CHƯA CUNG CẤP. Mã sinh viên: CHƯA CUNG CẤP.
// Nội dung thực hiện: Module 3 — hồ sơ cá nhân, tra cứu thí sinh và quyền đổi trạng thái Admin.
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;
using QuanLyKyThi_UNETI1_TI17A5HN.Filters;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.Services;
using QuanLyKyThi_UNETI1_TI17A5HN.Services.Module3;
using QuanLyKyThi_UNETI1_TI17A5HN.ViewModels;
using QuanLyKyThi_UNETI1_TI17A5HN.ViewModels.Module3;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Controllers;

[PhanQuyen]
public class ThiSinhController(ApplicationDbContext context, Module3Service service, ILogger<ThiSinhController> logger) : Controller
{
    private int MaTaiKhoan => HttpContext.Session.LayMaTaiKhoan()!.Value;

    [HttpGet, PhanQuyen(VaiTro.ThiSinh)]
    public async Task<IActionResult> HoSoCaNhan() => View(await service.DocHoSoCaNhanAsync(MaTaiKhoan));

    [HttpGet, PhanQuyen(VaiTro.ThiSinh)]
    public async Task<IActionResult> CapNhatHoSo()
    {
        var caNhan = await service.DocHoSoCaNhanAsync(MaTaiKhoan);
        if (caNhan.DaKhaiBao) return View(caNhan);
        var taiKhoan = await context.TaiKhoans.AsNoTracking().Where(t => t.MaTaiKhoan == MaTaiKhoan)
            .Select(t => new { t.HoTen, t.Email }).SingleAsync();
        return View(new HoSoCaNhanFormViewModel { HoTen = taiKhoan.HoTen, Email = taiKhoan.Email, SoLanThiTruocDay = 0 });
    }

    [HttpPost, ValidateAntiForgeryToken, PhanQuyen(VaiTro.ThiSinh)]
    public async Task<IActionResult> CapNhatHoSo(HoSoCaNhanFormViewModel model)
    {
        foreach (var error in QuyTacHoSoModule3.KiemTraCaNhan(model, service.BayGio))
            foreach (var field in error.MemberNames.DefaultIfEmpty(""))
                if (!ModelState.TryGetValue(field, out var entry) || entry.Errors.Count == 0)
                    ModelState.AddModelError(field, error.ErrorMessage!);
        if (ModelState.IsValid)
        {
            try
            {
                await service.LuuHoSoCaNhanAsync(MaTaiKhoan, model);
                TempData["ThongBao"] = "Đã lưu hồ sơ cá nhân.";
                return RedirectToAction(nameof(HoSoCaNhan));
            }
            catch (NghiepVuModule3Exception e) { ModelState.AddModelError(e.Truong, e.Message); }
            catch (Exception e) when (LoiDuLieuModule3.LaLoiDatabase(e))
            {
                logger.LogWarning(e, "Không lưu được hồ sơ cá nhân Module 3.");
                ModelState.AddModelError("", "Không lưu được hồ sơ. Dữ liệu có thể vừa thay đổi ở tab khác; hãy tải lại và thử lại.");
            }
        }
        return View(model);
    }

    [HttpGet, PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
    public async Task<IActionResult> Index(string? tuKhoa, TrangThaiThiSinh? trangThai, int trang = 1)
    {
        var query = context.ThiSinhs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var key = tuKhoa.Trim();
            query = query.Where(t => t.HoTen.Contains(key) || t.Email.Contains(key) || t.SoDienThoai.Contains(key) || t.TaiKhoan!.TenDangNhap.Contains(key));
        }
        if (trangThai.HasValue) query = query.Where(t => t.TrangThai == trangThai);
        var count = await query.CountAsync();
        var page = PhanTrangViewModel.ChuanHoaTrang(trang, count);
        return View(new QuanLyThiSinhViewModel
        {
            TuKhoa = tuKhoa, TrangThai = trangThai,
            DanhSach = await query.OrderBy(t => t.HoTen).ThenBy(t => t.MaThiSinh).Skip((page - 1) * 10).Take(10)
                .Select(t => new ThiSinhDongViewModel
                { MaThiSinh = t.MaThiSinh, HoTen = t.HoTen, TenDangNhap = t.TaiKhoan!.TenDangNhap, Email = t.Email, SoDienThoai = t.SoDienThoai, TrangThai = t.TrangThai }).ToListAsync(),
            PhanTrang = new() { TrangHienTai = page, TongSoBanGhi = count, ThamSo = new() { ["tuKhoa"] = tuKhoa, ["trangThai"] = trangThai?.ToString() } }
        });
    }

    [HttpGet, PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
    public async Task<IActionResult> Details(int id, int trang = 1)
    {
        var thiSinh = await context.ThiSinhs.AsNoTracking().SingleOrDefaultAsync(t => t.MaThiSinh == id);
        if (thiSinh == null) return NotFound();
        var query = context.HoSoDangKyThis.AsNoTracking().Where(h => h.MaThiSinh == id);
        var count = await query.CountAsync();
        var page = PhanTrangViewModel.ChuanHoaTrang(trang, count);
        return View(new ChiTietThiSinhViewModel
        {
            CaNhan = QuyTacHoSoModule3.CaNhan(thiSinh, service.BayGio),
            TenDangNhap = await context.TaiKhoans.Where(t => t.MaTaiKhoan == thiSinh.MaTaiKhoan).Select(t => t.TenDangNhap).SingleAsync(),
            CoQuyenDoiTrangThai = HttpContext.Session.LaVaiTro(VaiTro.Admin),
            HoSos = await query.OrderByDescending(h => h.NgayNop).ThenBy(h => h.MaHoSo).Skip((page - 1) * 10).Take(10)
                .Select(h => new HoSoThiDongViewModel
                { MaHoSo = h.MaHoSo, TenKyThi = h.KyThi!.TenKyThi, TenChungChi = h.KyThi.ChungChi!.TenChungChi, NgayNop = h.NgayNop, TrangThai = h.TrangThai }).ToListAsync(),
            PhanTrang = new() { Action = "Details", TrangHienTai = page, TongSoBanGhi = count, ThamSo = new() { ["id"] = id.ToString() } }
        });
    }

    [HttpPost, ValidateAntiForgeryToken, PhanQuyen(VaiTro.Admin)]
    public async Task<IActionResult> CapNhatTrangThai(int id, CapNhatTrangThaiThiSinhViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["LoiThongBao"] = "Vui lòng chọn trạng thái thí sinh hợp lệ.";
            return RedirectToAction(nameof(Details), new { id });
        }
        try
        {
            await service.CapNhatTrangThaiAsync(MaTaiKhoan, id, model.TrangThai!.Value);
            TempData["ThongBao"] = "Đã cập nhật trạng thái thí sinh.";
        }
        catch (NghiepVuModule3Exception e)
        {
            if (e.MaLoi == "KHONG_TIM_THAY") return NotFound();
            TempData["LoiThongBao"] = e.Message;
        }
        catch (Exception e) when (LoiDuLieuModule3.LaLoiDatabase(e))
        {
            logger.LogWarning(e, "Không đổi được trạng thái thí sinh Module 3.");
            TempData["LoiThongBao"] = "Dữ liệu vừa thay đổi hoặc không lưu được. Hãy tải lại và thử lại.";
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
