// Họ và tên: Phạm Quang Minh
// Mã sinh viên: 23103100201
// Nội dung thực hiện: Nhập, sửa có kiểm soát và xem kết quả thi theo quyền dữ liệu.
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
    public class KetQuaThiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Module5Service _module5Service;

        public KetQuaThiController(ApplicationDbContext context, Module5Service module5Service)
        {
            _context = context;
            _module5Service = module5Service;
        }

        private bool CoQuyenQuanLy => HttpContext.Session.LaVaiTro(VaiTro.Admin, VaiTro.CanBoKhaoThi);
        private IQueryable<KetQuaThi> DuLieuDuocXem()
        {
            var query = _context.KetQuaThis.AsNoTracking();
            if (!CoQuyenQuanLy)
            {
                var ma = HttpContext.Session.LayMaTaiKhoan();
                query = query.Where(k => k.HoSoDangKyThi!.ThiSinh!.MaTaiKhoan == ma);
            }
            return query;
        }

        public async Task<IActionResult> Index(string? tuKhoa, KetQuaCuoiCung? ketQua, int trang = 1)
        {
            var query = DuLieuDuocXem();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tuKhoaTimKiem = tuKhoa.Trim();
                query = query.Where(k => k.HoSoDangKyThi!.ThiSinh!.HoTen.Contains(tuKhoaTimKiem) || k.HoSoDangKyThi.KyThi!.TenKyThi.Contains(tuKhoaTimKiem));
            }
            if (ketQua.HasValue) query = query.Where(k => k.KetQua == ketQua);
            var tongSo = await query.CountAsync();
            var trangHienTai = PhanTrangViewModel.ChuanHoaTrang(trang, tongSo);
            return View(new KetQuaThiIndexViewModel
            {
                DanhSach = await query.OrderByDescending(k => k.NgayCapNhat).ThenBy(k => k.MaKetQuaThi)
                    .Skip((trangHienTai - 1) * PhanTrangViewModel.KichThuocMacDinh)
                    .Take(PhanTrangViewModel.KichThuocMacDinh)
                    .Select(k => new KetQuaThiDongViewModel
                    {
                        MaKetQuaThi = k.MaKetQuaThi, MaHoSo = k.MaHoSo, HoTenThiSinh = k.HoSoDangKyThi!.ThiSinh!.HoTen,
                        TenKyThi = k.HoSoDangKyThi.KyThi!.TenKyThi, DiemDanhGia = k.DiemDanhGia, KetQua = k.KetQua, NgayCapNhat = k.NgayCapNhat
                    }).ToListAsync(),
                TuKhoa = tuKhoa, KetQua = ketQua, CoQuyenQuanLy = CoQuyenQuanLy,
                PhanTrang = new PhanTrangViewModel
                {
                    TrangHienTai = trangHienTai, TongSoBanGhi = tongSo,
                    ThamSo = new() { ["tuKhoa"] = tuKhoa, ["ketQua"] = ketQua?.ToString() }
                }
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var model = await DuLieuDuocXem().Where(k => k.MaKetQuaThi == id).Select(k => new KetQuaThiChiTietViewModel
            {
                MaKetQuaThi = k.MaKetQuaThi, MaHoSo = k.MaHoSo, HoTenThiSinh = k.HoSoDangKyThi!.ThiSinh!.HoTen,
                TenKyThi = k.HoSoDangKyThi.KyThi!.TenKyThi, DiemDanhGia = k.DiemDanhGia, KetQua = k.KetQua,
                NgayCapNhat = k.NgayCapNhat, NhanXet = k.NhanXet, TrangThaiHoSo = k.HoSoDangKyThi.TrangThai
            }).SingleOrDefaultAsync();
            if (model == null) return NotFound();
            var trangThaiKetQua = model.KetQua == KetQuaCuoiCung.Dat ? TrangThaiHoSo.Dat : TrangThaiHoSo.KhongDat;
            model.CoTheSua = CoQuyenQuanLy && model.TrangThaiHoSo == trangThaiKetQua;
            return View(model);
        }

        [PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public async Task<IActionResult> Create(int? maHoSo)
        {
            var model = new KetQuaThiFormViewModel { MaHoSo = maHoSo ?? 0 };
            await NapDanhSachHoSoAsync(model, false);
            return View("Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken, PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public Task<IActionResult> Create(KetQuaThiFormViewModel model) => LuuAsync(model, false);

        [PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public async Task<IActionResult> Edit(int id)
        {
            var ketQua = await _context.KetQuaThis.AsNoTracking().SingleOrDefaultAsync(k => k.MaKetQuaThi == id);
            if (ketQua == null) return NotFound();
            var model = new KetQuaThiFormViewModel
            {
                MaKetQuaThi = id, MaHoSo = ketQua.MaHoSo, DiemDanhGia = ketQua.DiemDanhGia, KetQua = ketQua.KetQua, NhanXet = ketQua.NhanXet
            };
            await NapDanhSachHoSoAsync(model, true);
            return View("Form", model);
        }

        [HttpPost, ValidateAntiForgeryToken, PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
        public Task<IActionResult> Edit(int id, KetQuaThiFormViewModel model)
            => id != model.MaKetQuaThi ? Task.FromResult<IActionResult>(BadRequest()) : LuuAsync(model, true);

        private async Task<IActionResult> LuuAsync(KetQuaThiFormViewModel model, bool sua)
        {
            if (!sua) model.MaKetQuaThi = 0;
            if (ModelState.IsValid)
            {
                try
                {
                    var id = await _module5Service.LuuKetQuaAsync(model, sua);
                    TempData["ThongBao"] = "Đã lưu kết quả và cập nhật trạng thái hồ sơ. Thí sinh có thể xem kết quả ngay.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (NghiepVuModule5Exception e) { ModelState.AddModelError("", e.Message); }
                catch (Exception e) when (e is DbUpdateException or DbException)
                { ModelState.AddModelError("", "Không lưu được kết quả. Có thể dữ liệu vừa được cập nhật; hãy tải lại và thử lại."); }
            }
            await NapDanhSachHoSoAsync(model, sua);
            return View("Form", model);
        }

        private async Task NapDanhSachHoSoAsync(KetQuaThiFormViewModel model, bool sua)
        {
            var query = _context.HoSoDangKyThis.AsNoTracking();
            if (sua) query = query.Where(h => h.KetQuaThi != null && h.KetQuaThi.MaKetQuaThi == model.MaKetQuaThi);
            else
            {
                var bayGio = _module5Service.BayGio;
                query = query.Where(h => h.TrangThai == TrangThaiHoSo.DaThi && h.KetQuaThi == null
                    && h.LichThis.Any(l => l.TrangThai == TrangThaiLichThi.DaHoanThanh && l.ThoiGianKetThuc <= bayGio));
            }
            model.HoSos = await query.OrderBy(h => h.MaHoSo).Select(h => new SelectListItem
            {
                Value = h.MaHoSo.ToString(), Text = "#" + h.MaHoSo + " — " + h.ThiSinh!.HoTen + " — " + h.KyThi!.TenKyThi
            }).ToListAsync();
        }
    }
}
