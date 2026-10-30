using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;
using QuanLyKyThi_UNETI1_TI17A5HN.Filters;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.ViewModels;
using QuanLyKyThi_UNETI1_TI17A5HN.Services;
using System.Data;
namespace QuanLyKyThi_UNETI1_TI17A5HN.Controllers
{
    [PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]
    public class XetDuyetHoSoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public XetDuyetHoSoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? tuKhoa,
            TrangThaiHoSo? trangThai,
            int? trang)
        {
            // Nếu chưa chọn trạng thái thì mặc định xem hồ sơ chờ duyệt.
            var trangThaiLoc = trangThai ?? TrangThaiHoSo.ChoDuyet;

            var query = _context.HoSoDangKyThis
                .AsNoTracking()
                .Where(h => h.TrangThai == trangThaiLoc);

            // Tìm theo tên thí sinh, kỳ thi hoặc chứng chỉ.
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tuKhoaChuanHoa = tuKhoa.Trim();

                query = query.Where(h =>
                    h.ThiSinh!.HoTen.Contains(tuKhoaChuanHoa) ||
                    h.KyThi!.TenKyThi.Contains(tuKhoaChuanHoa) ||
                    h.KyThi.ChungChi!.TenChungChi.Contains(tuKhoaChuanHoa));
            }

            // Đếm trước khi thực hiện Skip và Take.
            var tongSoBanGhi = await query.CountAsync();

            var trangHienTai = PhanTrangViewModel.ChuanHoaTrang(
                trang,
                tongSoBanGhi);

            var danhSach = await query
                .OrderBy(h => h.NgayNop)
                .Skip((trangHienTai - 1) *
                      PhanTrangViewModel.KichThuocMacDinh)
                .Take(PhanTrangViewModel.KichThuocMacDinh)
                .Select(h => new XetDuyetHoSoDongViewModel
                {
                    MaHoSo = h.MaHoSo,
                    HoTenThiSinh = h.ThiSinh!.HoTen,
                    TenKyThi = h.KyThi!.TenKyThi,
                    TenChungChi = h.KyThi.ChungChi!.TenChungChi,
                    NgayNop = h.NgayNop,
                    TrangThai = h.TrangThai
                })
                .ToListAsync();

            var model = new XetDuyetHoSoIndexViewModel
            {
                DanhSach = danhSach,
                TuKhoa = tuKhoa,
                TrangThai = trangThaiLoc,

                PhanTrang = new PhanTrangViewModel
                {
                    TrangHienTai = trangHienTai,
                    TongSoBanGhi = tongSoBanGhi,
                    Action = nameof(Index),

                    ThamSo = new Dictionary<string, string?>
                    {
                        ["tuKhoa"] = tuKhoa,
                        ["trangThai"] = trangThaiLoc.ToString()
                    }
                }
            };

            return View(model);
        }
        public async Task<IActionResult> Details(int id)
        {
            var model = await _context.HoSoDangKyThis
                .AsNoTracking()
                .Where(h => h.MaHoSo == id)
                .Select(h => new XetDuyetHoSoChiTietViewModel
                {
                    // Hồ sơ đăng ký
                    MaHoSo = h.MaHoSo,
                    NgayNop = h.NgayNop,
                    MucDichDuThi = h.MucDichDuThi,
                    GhiChu = h.GhiChu,
                    TrangThai = h.TrangThai,

                    // Thí sinh
                    MaThiSinh = h.MaThiSinh,
                    HoTenThiSinh = h.ThiSinh!.HoTen,
                    NgaySinh = h.ThiSinh.NgaySinh,
                    GioiTinh = h.ThiSinh.GioiTinh,
                    SoDienThoai = h.ThiSinh.SoDienThoai,
                    Email = h.ThiSinh.Email,
                    DiaChi = h.ThiSinh.DiaChi,
                    TrinhDoHienTai = h.ThiSinh.TrinhDoHienTai,
                    NgheNghiep = h.ThiSinh.NgheNghiep,
                    SoLanThiTruocDay = h.ThiSinh.SoLanThiTruocDay,
                    ChungChiDaCo = h.ThiSinh.ChungChiDaCo,
                    TrangThaiThiSinh = h.ThiSinh.TrangThai,

                    // Kỳ thi
                    MaKyThi = h.MaKyThi,
                    TenKyThi = h.KyThi!.TenKyThi,
                    TenChungChi = h.KyThi.ChungChi!.TenChungChi,
                    CapDoThi = h.KyThi.CapDoThi,
                    DiemDauVaoToiThieu = h.KyThi.DiemDauVaoToiThieu,
                    DieuKienDuThi = h.KyThi.DieuKienDuThi,
                    NgayBatDauDangKy = h.KyThi.NgayBatDauDangKy,
                    HanDangKy = h.KyThi.HanDangKy,
                    SoChoThiToiDa = h.KyThi.SoChoThiToiDa,

                    // Thông tin xử lý trước đó
                    NgayXuLy = h.NgayXuLy,
                    NhanXetXetDuyet = h.NhanXetXetDuyet,

                    TenNguoiXuLy = h.NguoiXuLy == null
                        ? null
                        : h.NguoiXuLy.HoTen
                })
                .FirstOrDefaultAsync();

            if (model == null)
            {
                return NotFound();
            }

            // Những trạng thái được tính là đã sử dụng một suất dự thi.
            model.SoSuatDaSuDung = await _context.HoSoDangKyThis
                .AsNoTracking()
                .CountAsync(h =>
                    h.MaKyThi == model.MaKyThi &&
                    (
                        h.TrangThai == TrangThaiHoSo.DuDieuKien ||
                        h.TrangThai == TrangThaiHoSo.ChoThi ||
                        h.TrangThai == TrangThaiHoSo.DaThi ||
                        h.TrangThai == TrangThaiHoSo.Dat ||
                        h.TrangThai == TrangThaiHoSo.KhongDat
                    ));

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoi(
    XetDuyetHoSoFormViewModel model)
        {
            // Từ chối bắt buộc phải ghi rõ lý do.
            if (string.IsNullOrWhiteSpace(model.NhanXet))
            {
                TempData["LoiThongBao"] =
                    "Vui lòng nhập lý do từ chối hồ sơ.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.MaHoSo });
            }

            if (!ModelState.IsValid)
            {
                TempData["LoiThongBao"] =
                    "Thông tin xử lý hồ sơ không hợp lệ.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.MaHoSo });
            }

            var maNguoiXuLy = HttpContext.Session.LayMaTaiKhoan();

            if (!maNguoiXuLy.HasValue)
            {
                return RedirectToAction("DangNhap", "Auth");
            }

            var ngayXuLy = DateTime.Now;
            var nhanXet = model.NhanXet.Trim();

            /*
             * Chỉ cập nhật bản ghi nếu trạng thái trong database
             * vẫn là ChoDuyet.
             *
             * ExecuteUpdateAsync trả về số bản ghi đã cập nhật.
             */
            var soBanGhiDaCapNhat = await _context.HoSoDangKyThis
                .Where(h =>
                    h.MaHoSo == model.MaHoSo &&
                    h.TrangThai == TrangThaiHoSo.ChoDuyet)
                .ExecuteUpdateAsync(capNhat => capNhat
                    .SetProperty(
                        h => h.TrangThai,
                        TrangThaiHoSo.KhongDuDieuKien)
                    .SetProperty(
                        h => h.NgayXuLy,
                        (DateTime?)ngayXuLy)
                    .SetProperty(
                        h => h.MaNguoiXuLy,
                        (int?)maNguoiXuLy.Value)
                    .SetProperty(
                        h => h.NhanXetXetDuyet,
                        nhanXet));

            if (soBanGhiDaCapNhat == 0)
            {
                var hoSoConTonTai = await _context.HoSoDangKyThis
                    .AsNoTracking()
                    .AnyAsync(h => h.MaHoSo == model.MaHoSo);

                if (!hoSoConTonTai)
                {
                    return NotFound();
                }

                TempData["LoiThongBao"] =
                    "Hồ sơ đã được người khác xử lý hoặc không còn ở trạng thái chờ duyệt.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.MaHoSo });
            }

            TempData["ThongBao"] =
                "Đã từ chối hồ sơ đăng ký dự thi.";

            return RedirectToAction(
                nameof(Details),
                new { id = model.MaHoSo });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duyet(
    XetDuyetHoSoFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["LoiThongBao"] =
                    "Thông tin xử lý hồ sơ không hợp lệ.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.MaHoSo });
            }

            var maNguoiXuLy = HttpContext.Session.LayMaTaiKhoan();

            if (!maNguoiXuLy.HasValue)
            {
                return RedirectToAction("DangNhap", "Auth");
            }

            /*
             * Serializable bảo đảm trong lúc kiểm tra chỉ tiêu và duyệt,
             * một yêu cầu khác không thể chen vào làm vượt số chỗ.
             */
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var hoSo = await _context.HoSoDangKyThis
                .Include(h => h.KyThi)
                .Include(h => h.ThiSinh)
                .FirstOrDefaultAsync(h => h.MaHoSo == model.MaHoSo);

            if (hoSo == null)
            {
                return NotFound();
            }

            // Chỉ hồ sơ chờ duyệt mới được xử lý.
            if (hoSo.TrangThai != TrangThaiHoSo.ChoDuyet)
            {
                TempData["LoiThongBao"] =
                    "Hồ sơ đã được xử lý hoặc không còn ở trạng thái chờ duyệt.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.MaHoSo });
            }

            // Không duyệt hồ sơ của thí sinh đã ngừng hoạt động.
            if (hoSo.ThiSinh == null ||
                hoSo.ThiSinh.TrangThai != TrangThaiThiSinh.HoatDong)
            {
                TempData["LoiThongBao"] =
                    "Hồ sơ thí sinh đang ngừng hoạt động nên không thể duyệt.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.MaHoSo });
            }

            if (hoSo.KyThi == null)
            {
                TempData["LoiThongBao"] =
                    "Không tìm thấy kỳ thi của hồ sơ.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.MaHoSo });
            }

            // Các trạng thái được tính là đã sử dụng một suất.
            var soSuatDaSuDung = await _context.HoSoDangKyThis
                .AsNoTracking()
                .CountAsync(h =>
                    h.MaKyThi == hoSo.MaKyThi &&
                    (
                        h.TrangThai == TrangThaiHoSo.DuDieuKien ||
                        h.TrangThai == TrangThaiHoSo.ChoThi ||
                        h.TrangThai == TrangThaiHoSo.DaThi ||
                        h.TrangThai == TrangThaiHoSo.Dat ||
                        h.TrangThai == TrangThaiHoSo.KhongDat
                    ));

            if (soSuatDaSuDung >= hoSo.KyThi.SoChoThiToiDa)
            {
                TempData["LoiThongBao"] =
                    "Kỳ thi đã đủ chỉ tiêu, không thể duyệt thêm hồ sơ.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.MaHoSo });
            }

            // Cập nhật kết quả xét duyệt.
            hoSo.TrangThai = TrangThaiHoSo.DuDieuKien;
            hoSo.NgayXuLy = DateTime.Now;
            hoSo.MaNguoiXuLy = maNguoiXuLy.Value;

            hoSo.NhanXetXetDuyet =
                string.IsNullOrWhiteSpace(model.NhanXet)
                    ? "Hồ sơ đủ điều kiện dự thi."
                    : model.NhanXet.Trim();

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["ThongBao"] =
                "Đã duyệt hồ sơ đủ điều kiện dự thi.";

            return RedirectToAction(
                nameof(Details),
                new { id = model.MaHoSo });
        }
    }
}