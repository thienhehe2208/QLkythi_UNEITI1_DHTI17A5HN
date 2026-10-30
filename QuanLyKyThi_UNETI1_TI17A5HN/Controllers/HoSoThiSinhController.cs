// Họ và tên: CHƯA CUNG CẤP. Mã sinh viên: CHƯA CUNG CẤP.
// Nội dung thực hiện: Module 3 — nộp, danh sách, chi tiết và hủy hồ sơ chỉ của tài khoản hiện tại.
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

[PhanQuyen(VaiTro.ThiSinh)]
public class HoSoThiSinhController(ApplicationDbContext context, Module3Service service, ILogger<HoSoThiSinhController> logger) : Controller
{
    private int MaTaiKhoan => HttpContext.Session.LayMaTaiKhoan()!.Value;

    [HttpGet]
    public async Task<IActionResult> Index(string? tuKhoa, TrangThaiHoSo? trangThai, int trang = 1)
    {
        var ma = MaTaiKhoan;
        var query = context.HoSoDangKyThis.AsNoTracking().Where(h => h.ThiSinh!.MaTaiKhoan == ma);
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var key = tuKhoa.Trim();
            query = query.Where(h => h.KyThi!.TenKyThi.Contains(key) || h.KyThi.ChungChi!.TenChungChi.Contains(key));
        }
        if (trangThai.HasValue) query = query.Where(h => h.TrangThai == trangThai);
        var count = await query.CountAsync();
        var page = PhanTrangViewModel.ChuanHoaTrang(trang, count);
        return View(new DanhSachHoSoThiViewModel
        {
            TuKhoa = tuKhoa, TrangThai = trangThai,
            DanhSach = await query.OrderByDescending(h => h.NgayNop).ThenByDescending(h => h.MaHoSo).Skip((page - 1) * 10).Take(10)
                .Select(h => new HoSoThiDongViewModel
                {
                    MaHoSo = h.MaHoSo, TenKyThi = h.KyThi!.TenKyThi, TenChungChi = h.KyThi.ChungChi!.TenChungChi,
                    NgayNop = h.NgayNop, TrangThai = h.TrangThai,
                    CoTheHuy = (h.TrangThai == TrangThaiHoSo.ChoDuyet || h.TrangThai == TrangThaiHoSo.DuDieuKien)
                        && !h.LichThis.Any(l => l.TrangThai != TrangThaiLichThi.DaHuy) && h.KetQuaThi == null
                }).ToListAsync(),
            PhanTrang = new() { TrangHienTai = page, TongSoBanGhi = count, ThamSo = new() { ["tuKhoa"] = tuKhoa, ["trangThai"] = trangThai?.ToString() } }
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var model = await service.DocChiTietHoSoAsync(MaTaiKhoan, id);
        return model == null ? NotFound() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? maKyThi)
    {
        var model = new NopHoSoThiViewModel { MaKyThi = maKyThi };
        await service.NapFormNopAsync(MaTaiKhoan, model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NopHoSoThiViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var id = await service.NopHoSoAsync(MaTaiKhoan, model);
                TempData["ThongBao"] = "Đã nộp hồ sơ. Hồ sơ đang Chờ duyệt.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (NghiepVuModule3Exception e) { ModelState.AddModelError(e.Truong, e.Message); }
            catch (Exception e) when (LoiDuLieuModule3.LaLoiDatabase(e))
            {
                logger.LogWarning(e, "Không nộp được hồ sơ Module 3.");
                ModelState.AddModelError("", "Không lưu được hồ sơ. Dữ liệu có thể vừa thay đổi; hãy tải lại và kiểm tra hồ sơ đã nộp trước khi thử lại.");
            }
        }
        await service.NapFormNopAsync(MaTaiKhoan, model);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Huy(int id)
    {
        var model = await service.DocChiTietHoSoAsync(MaTaiKhoan, id);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> HuyXacNhan(int id)
    {
        try
        {
            await service.HuyHoSoAsync(MaTaiKhoan, id);
            TempData["ThongBao"] = "Đã hủy đăng ký. Hồ sơ được giữ trong lịch sử.";
        }
        catch (NghiepVuModule3Exception e)
        {
            if (e.MaLoi == "KHONG_TIM_THAY") return NotFound();
            TempData["LoiThongBao"] = e.Message;
        }
        catch (Exception e) when (LoiDuLieuModule3.LaLoiDatabase(e))
        {
            logger.LogWarning(e, "Không hủy được hồ sơ Module 3.");
            TempData["LoiThongBao"] = "Không lưu được thay đổi. Hãy tải lại hồ sơ và thử lại.";
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
