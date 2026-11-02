// Họ và tên: Phạm Quang Minh
// Mã sinh viên: 23103100201
// Nội dung thực hiện: Quản lý lịch thi, phân quyền và giới hạn quyền xem theo thí sinh.
using System.Data.Common;
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
    public class LichThiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Module5Service _module5Service;

        public LichThiController(ApplicationDbContext context, Module5Service module5Service)
        {
            _context = context;
            _module5Service = module5Service;
        }

        private bool CoQuyenQuanLy => HttpContext.Session.LaVaiTro(VaiTro.Admin, VaiTro.CanBoKhaoThi);
        private IQueryable<LichThi> DuLieuDuocXem()
        {
            var query = _context.LichThis.AsNoTracking();
            if (!CoQuyenQuanLy)
            {
                var ma = HttpContext.Session.LayMaTaiKhoan();
                query = query.Where(l => l.HoSoDangKyThi!.ThiSinh!.MaTaiKhoan == ma);
            }
            return query;
        }

        public async Task<IActionResult> Index(string? tuKhoa, TrangThaiLichThi? trangThai, int trang = 1)
        {
            var query = DuLieuDuocXem();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tuKhoaTimKiem = tuKhoa.Trim();
                query = query.Where(l => l.HoSoDangKyThi!.ThiSinh!.HoTen.Contains(tuKhoaTimKiem)
                    || l.HoSoDangKyThi.KyThi!.TenKyThi.Contains(tuKhoaTimKiem) || l.GiamThi.Contains(tuKhoaTimKiem));
            }
            if (trangThai.HasValue) query = query.Where(l => l.TrangThai == trangThai);
            var tongSo = await query.CountAsync();
            var trangHienTai = PhanTrangViewModel.ChuanHoaTrang(trang, tongSo);
            return View(new LichThiIndexViewModel
            {
                DanhSach = await query.OrderByDescending(l => l.ThoiGianBatDau).ThenBy(l => l.MaLichThi)
                    .Skip((trangHienTai - 1) * PhanTrangViewModel.KichThuocMacDinh)
                    .Take(PhanTrangViewModel.KichThuocMacDinh)
                    .Select(l => new LichThiDongViewModel
                    {
                        MaLichThi = l.MaLichThi, MaHoSo = l.MaHoSo, MaThiSinh = l.HoSoDangKyThi!.MaThiSinh,
                        HoTenThiSinh = l.HoSoDangKyThi.ThiSinh!.HoTen, TenKyThi = l.HoSoDangKyThi.KyThi!.TenKyThi,
                        ThoiGianBatDau = l.ThoiGianBatDau, ThoiGianKetThuc = l.ThoiGianKetThuc,
                        DiaDiemHoacLienKet = l.DiaDiemHoacLienKet, PhongThi = l.PhongThi, GiamThi = l.GiamThi, TrangThai = l.TrangThai
                    }).ToListAsync(),
                TuKhoa = tuKhoa, TrangThai = trangThai, CoQuyenQuanLy = CoQuyenQuanLy,
                PhanTrang = new PhanTrangViewModel
                {
                    TrangHienTai = trangHienTai, TongSoBanGhi = tongSo,
                    ThamSo = new() { ["tuKhoa"] = tuKhoa, ["trangThai"] = trangThai?.ToString() }
                }
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var model = await DuLieuDuocXem().Where(l => l.MaLichThi == id).Select(l => new LichThiChiTietViewModel
            {
                MaLichThi = l.MaLichThi, MaHoSo = l.MaHoSo, MaThiSinh = l.HoSoDangKyThi!.MaThiSinh,
                HoTenThiSinh = l.HoSoDangKyThi.ThiSinh!.HoTen, TenKyThi = l.HoSoDangKyThi.KyThi!.TenKyThi,
                ThoiGianBatDau = l.ThoiGianBatDau, ThoiGianKetThuc = l.ThoiGianKetThuc,
                DiaDiemHoacLienKet = l.DiaDiemHoacLienKet, PhongThi = l.PhongThi, GiamThi = l.GiamThi,
                TrangThai = l.TrangThai, TrangThaiHoSo = l.HoSoDangKyThi.TrangThai, HinhThucThi = l.HinhThucThi, GhiChu = l.GhiChu
            }).SingleOrDefaultAsync();
            if (model == null) return NotFound();
            var dangChoThi = CoQuyenQuanLy && model.TrangThai == TrangThaiLichThi.DaLenLich && model.TrangThaiHoSo == TrangThaiHoSo.ChoThi;
            model.CoTheSua = model.CoTheHuy = dangChoThi && model.ThoiGianBatDau > _module5Service.BayGio;
            model.CoTheHoanThanh = dangChoThi && model.ThoiGianKetThuc <= _module5Service.BayGio;
            model.CoTheNhapKetQua = CoQuyenQuanLy && model.TrangThai == TrangThaiLichThi.DaHoanThanh && model.TrangThaiHoSo == TrangThaiHoSo.DaThi;
            return View(model);
        }

        [PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public async Task<IActionResult> Create(int? maHoSo)
        {
            var model = new LichThiFormViewModel { MaHoSo = maHoSo ?? 0 };
            await NapDanhSachHoSoAsync(model, false);
            return View("Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken, PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public Task<IActionResult> Create(LichThiFormViewModel model) => LuuAsync(model, false);

        [PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public async Task<IActionResult> Edit(int id)
        {
            var lich = await _context.LichThis.AsNoTracking().SingleOrDefaultAsync(l => l.MaLichThi == id);
            if (lich == null) return NotFound();
            if (lich.TrangThai != TrangThaiLichThi.DaLenLich || lich.ThoiGianBatDau <= _module5Service.BayGio)
            {
                TempData["LoiThongBao"] = "Chỉ sửa lịch đang có hiệu lực và chưa bắt đầu.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var model = new LichThiFormViewModel
            {
                MaLichThi = id, MaHoSo = lich.MaHoSo, ThoiGianBatDau = lich.ThoiGianBatDau,
                ThoiGianKetThuc = lich.ThoiGianKetThuc, HinhThucThi = lich.HinhThucThi,
                DiaDiemHoacLienKet = lich.DiaDiemHoacLienKet, PhongThi = lich.PhongThi, GiamThi = lich.GiamThi, GhiChu = lich.GhiChu
            };
            await NapDanhSachHoSoAsync(model, true);
            return View("Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken, PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public Task<IActionResult> Edit(int id, LichThiFormViewModel model)
            => id != model.MaLichThi ? Task.FromResult<IActionResult>(BadRequest()) : LuuAsync(model, true);

        private async Task<IActionResult> LuuAsync(LichThiFormViewModel model, bool sua)
        {
            if (!sua) model.MaLichThi = 0;
            if (ModelState.IsValid)
            {
                try
                {
                    var id = await _module5Service.LuuLichAsync(model, sua);
                    TempData["ThongBao"] = sua ? "Đã cập nhật lịch thi." : "Đã lập lịch thi và chuyển hồ sơ sang Chờ thi.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (NghiepVuModule5Exception e) { ModelState.AddModelError("", e.Message); }
                catch (Exception e) when (e is DbUpdateException or DbException)
                { ModelState.AddModelError("", "Không lưu được dữ liệu. Có thể có thao tác đồng thời; hãy tải lại và thử lại."); }
            }
            await NapDanhSachHoSoAsync(model, sua);
            return View("Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken, PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public Task<IActionResult> Huy(int id) => ChuyenTrangThaiAsync(id, false);

        [HttpPost, ValidateAntiForgeryToken, PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public Task<IActionResult> HoanThanh(int id) => ChuyenTrangThaiAsync(id, true);

        private async Task<IActionResult> ChuyenTrangThaiAsync(int id, bool hoanThanh)
        {
            try
            {
                await _module5Service.ChuyenTrangThaiLichAsync(id, hoanThanh);
                TempData["ThongBao"] = hoanThanh ? "Đã hoàn thành lịch; hồ sơ chuyển sang Đã thi." : "Đã hủy lịch; hồ sơ trở về Đủ điều kiện.";
            }
            catch (NghiepVuModule5Exception e) { TempData["LoiThongBao"] = e.Message; }
            catch (Exception e) when (e is DbUpdateException or DbException)
            { TempData["LoiThongBao"] = "Không lưu được thay đổi. Hãy tải lại và thử lại."; }
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task NapDanhSachHoSoAsync(LichThiFormViewModel model, bool sua)
        {
            var query = _context.HoSoDangKyThis.AsNoTracking();
            if (sua) query = query.Where(h => h.LichThis.Any(l => l.MaLichThi == model.MaLichThi));
            else query = query.Where(h => h.TrangThai == TrangThaiHoSo.DuDieuKien
                && h.ThiSinh!.TrangThai == TrangThaiThiSinh.HoatDong
                && !h.LichThis.Any(l => l.TrangThai != TrangThaiLichThi.DaHuy) && h.KetQuaThi == null);
            model.HoSos = await query.OrderBy(h => h.MaHoSo).Select(h => new SelectListItem
            {
                Value = h.MaHoSo.ToString(), Text = "#" + h.MaHoSo + " — " + h.ThiSinh!.HoTen + " — " + h.KyThi!.TenKyThi
            }).ToListAsync();
        }
    }
}

