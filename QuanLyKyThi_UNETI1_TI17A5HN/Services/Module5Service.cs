// Họ và tên: Phạm Quang Minh
// Mã sinh viên: 23103100201
// Nội dung thực hiện: Kiểm tra trùng lịch, chuyển trạng thái lịch/hồ sơ và ghi kết quả bằng EF Core.
using System.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.ViewModels;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Services
{
    public class NghiepVuModule5Exception : Exception
    {
        public NghiepVuModule5Exception(string message) : base(message) { }
    }

    public class Module5Service
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeProvider _timeProvider;

        public Module5Service(ApplicationDbContext context, TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        // Múi giờ nghiệp vụ Việt Nam, không phụ thuộc múi giờ của máy chủ.
        public DateTime BayGio => _timeProvider.GetUtcNow().ToOffset(TimeSpan.FromHours(7)).DateTime;

        private static void KiemTraModel(object model)
        {
            var errors = new List<ValidationResult>();
            if (!Validator.TryValidateObject(model, new ValidationContext(model), errors, true))
                throw new NghiepVuModule5Exception(string.Join(" ", errors.Select(e => e.ErrorMessage)));
        }

        private static string ChuanHoa(string? text) => string.Join(' ', (text ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        public async Task<int> LuuLichAsync(LichThiFormViewModel model, bool sua)
        {
            KiemTraModel(model);
            var batDau = model.ThoiGianBatDau!.Value;
            var ketThuc = model.ThoiGianKetThuc!.Value;
            if (ketThuc <= batDau) throw new NghiepVuModule5Exception("Thời gian kết thúc phải sau thời gian bắt đầu.");
            if (batDau <= BayGio) throw new NghiepVuModule5Exception("Thời gian bắt đầu phải ở tương lai.");
            var giamThi = ChuanHoa(model.GiamThi);
            var phong = model.HinhThucThi == HinhThucThi.TrucTiep ? ChuanHoa(model.PhongThi) : null;
            var diaDiem = ChuanHoa(model.DiaDiemHoacLienKet);
            if (giamThi.Length == 0 || diaDiem.Length == 0)
                throw new NghiepVuModule5Exception("Giám thị và địa điểm/liên kết không được để trống.");
            if (model.HinhThucThi == HinhThucThi.TrucTiep && string.IsNullOrEmpty(phong))
                throw new NghiepVuModule5Exception("Thi trực tiếp phải khai báo phòng thi.");
            if (model.HinhThucThi == HinhThucThi.TrucTuyen
                && (!Uri.TryCreate(diaDiem, UriKind.Absolute, out var uri) || (uri.Scheme != "https" && uri.Scheme != "http")))
                throw new NghiepVuModule5Exception("Liên kết thi trực tuyến phải là URL http hoặc https hợp lệ.");

            // Serializable bảo vệ cả việc đọc kiểm tra và ghi, tránh hai yêu cầu đồng thời cùng vượt kiểm tra.
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            LichThi lich;
            if (sua)
            {
                lich = await _context.LichThis.SingleOrDefaultAsync(l => l.MaLichThi == model.MaLichThi)
                    ?? throw new NghiepVuModule5Exception("Lịch thi không tồn tại.");
                if (lich.MaHoSo != model.MaHoSo) throw new NghiepVuModule5Exception("Không được đổi hồ sơ của lịch thi.");
                if (lich.TrangThai != TrangThaiLichThi.DaLenLich || lich.ThoiGianBatDau <= BayGio)
                    throw new NghiepVuModule5Exception("Chỉ được sửa lịch chưa bắt đầu và đang có hiệu lực.");
            }
            else lich = new LichThi { MaHoSo = model.MaHoSo };

            var hoSo = await _context.HoSoDangKyThis.Include(h => h.ThiSinh).SingleOrDefaultAsync(h => h.MaHoSo == model.MaHoSo)
                ?? throw new NghiepVuModule5Exception("Hồ sơ không tồn tại.");
            var trangThaiCan = sua ? TrangThaiHoSo.ChoThi : TrangThaiHoSo.DuDieuKien;
            if (hoSo.TrangThai != trangThaiCan || hoSo.ThiSinh?.TrangThai != TrangThaiThiSinh.HoatDong)
                throw new NghiepVuModule5Exception("Hồ sơ chưa ở trạng thái cho phép lập/sửa lịch hoặc thí sinh đã ngừng hoạt động.");
            if (await _context.KetQuaThis.AnyAsync(k => k.MaHoSo == hoSo.MaHoSo))
                throw new NghiepVuModule5Exception("Hồ sơ đã có kết quả, không thể lập/sửa lịch.");
            var lichKhac = _context.LichThis.Where(l => l.MaLichThi != lich.MaLichThi && l.TrangThai != TrangThaiLichThi.DaHuy);
            if (await lichKhac.AnyAsync(l => l.MaHoSo == model.MaHoSo))
                throw new NghiepVuModule5Exception("Hồ sơ đã có một lịch thi còn hiệu lực.");
            // Hai khoảng giao nhau khi: bắt đầu cũ < kết thúc mới && bắt đầu mới < kết thúc cũ.
            // Hai lịch nối tiếp nhau chính xác tại một thời điểm được phép.
            var trungGio = lichKhac.Where(l => l.ThoiGianBatDau < ketThuc && batDau < l.ThoiGianKetThuc);
            if (await trungGio.AnyAsync(l => l.HoSoDangKyThi!.MaThiSinh == hoSo.MaThiSinh))
                throw new NghiepVuModule5Exception("Thí sinh bị trùng lịch thi.");
            if (await trungGio.AnyAsync(l => l.GiamThi.Trim().ToUpper() == giamThi.ToUpper()))
                throw new NghiepVuModule5Exception("Giám thị bị trùng lịch thi.");
            // Phòng thi được coi là mã phòng dùng chung trong hệ thống.
            if (phong != null && await trungGio.AnyAsync(l => l.HinhThucThi == HinhThucThi.TrucTiep
                    && l.PhongThi != null && l.PhongThi.Trim().ToUpper() == phong.ToUpper()))
                throw new NghiepVuModule5Exception("Phòng thi bị trùng lịch.");

            lich.ThoiGianBatDau = batDau;
            lich.ThoiGianKetThuc = ketThuc;
            lich.HinhThucThi = model.HinhThucThi;
            lich.DiaDiemHoacLienKet = diaDiem;
            lich.PhongThi = phong;
            lich.GiamThi = giamThi;
            lich.GhiChu = model.GhiChu?.Trim();
            lich.TrangThai = TrangThaiLichThi.DaLenLich;
            hoSo.TrangThai = TrangThaiHoSo.ChoThi;
            if (!sua) _context.LichThis.Add(lich);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return lich.MaLichThi;
        }

        public async Task ChuyenTrangThaiLichAsync(int id, bool hoanThanh)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var lich = await _context.LichThis.Include(l => l.HoSoDangKyThi).SingleOrDefaultAsync(l => l.MaLichThi == id)
                ?? throw new NghiepVuModule5Exception("Lịch thi không tồn tại.");
            var hoSo = lich.HoSoDangKyThi!;
            if (lich.TrangThai != TrangThaiLichThi.DaLenLich || hoSo.TrangThai != TrangThaiHoSo.ChoThi)
                throw new NghiepVuModule5Exception("Lịch hoặc hồ sơ không còn ở trạng thái cho phép xử lý.");
            if (await _context.KetQuaThis.AnyAsync(k => k.MaHoSo == hoSo.MaHoSo))
                throw new NghiepVuModule5Exception("Hồ sơ đã có kết quả cuối cùng.");
            if (hoanThanh && lich.ThoiGianKetThuc > BayGio)
                throw new NghiepVuModule5Exception("Chỉ được hoàn thành lịch sau thời gian kết thúc thi.");
            if (!hoanThanh && lich.ThoiGianBatDau <= BayGio)
                throw new NghiepVuModule5Exception("Chỉ được hủy lịch chưa bắt đầu.");
            if (await _context.LichThis.AnyAsync(l => l.MaHoSo == hoSo.MaHoSo && l.MaLichThi != id && l.TrangThai != TrangThaiLichThi.DaHuy))
                throw new NghiepVuModule5Exception("Hồ sơ có nhiều lịch hiệu lực, cần kiểm tra dữ liệu trước khi xử lý.");
            lich.TrangThai = hoanThanh ? TrangThaiLichThi.DaHoanThanh : TrangThaiLichThi.DaHuy;
            hoSo.TrangThai = hoanThanh ? TrangThaiHoSo.DaThi : TrangThaiHoSo.DuDieuKien;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task<int> LuuKetQuaAsync(KetQuaThiFormViewModel model, bool sua)
        {
            KiemTraModel(model);
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var hoSo = await _context.HoSoDangKyThis.SingleOrDefaultAsync(h => h.MaHoSo == model.MaHoSo)
                ?? throw new NghiepVuModule5Exception("Hồ sơ không tồn tại.");
            if (!await _context.LichThis.AnyAsync(l => l.MaHoSo == model.MaHoSo && l.TrangThai == TrangThaiLichThi.DaHoanThanh
                    && l.ThoiGianKetThuc <= BayGio))
                throw new NghiepVuModule5Exception("Hồ sơ phải có lịch thi đã hoàn thành.");
            var ketQua = await _context.KetQuaThis.SingleOrDefaultAsync(k => k.MaHoSo == model.MaHoSo);
            if (sua)
            {
                if (ketQua == null || ketQua.MaKetQuaThi != model.MaKetQuaThi)
                    throw new NghiepVuModule5Exception("Kết quả không tồn tại hoặc không thuộc hồ sơ này.");
                var trangThaiCu = ketQua.KetQua == KetQuaCuoiCung.Dat ? TrangThaiHoSo.Dat : TrangThaiHoSo.KhongDat;
                if (hoSo.TrangThai != trangThaiCu)
                    throw new NghiepVuModule5Exception("Trạng thái hồ sơ không khớp kết quả hiện tại, không được sửa.");
            }
            else
            {
                if (hoSo.TrangThai != TrangThaiHoSo.DaThi || ketQua != null)
                    throw new NghiepVuModule5Exception("Chỉ nhập kết quả mới cho hồ sơ Đã thi và chưa có kết quả.");
                ketQua = new KetQuaThi { MaHoSo = model.MaHoSo };
                _context.KetQuaThis.Add(ketQua);
            }
            ketQua.DiemDanhGia = model.DiemDanhGia;
            ketQua.NhanXet = model.NhanXet?.Trim();
            ketQua.KetQua = model.KetQua!.Value;
            ketQua.NgayCapNhat = BayGio;
            hoSo.TrangThai = ketQua.KetQua == KetQuaCuoiCung.Dat ? TrangThaiHoSo.Dat : TrangThaiHoSo.KhongDat;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return ketQua.MaKetQuaThi;
        }
    }
}
