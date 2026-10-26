// Họ và tên: Phạm Quang Minh
// Mã sinh viên: 23103100201
// Nội dung thực hiện: Kiểm thử tích hợp nghiệp vụ Module 5 trên SQL Server riêng, không dùng database nhóm.
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Controllers;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.Services;
using QuanLyKyThi_UNETI1_TI17A5HN.ViewModels;

var name = "QuanLyKyThi_Module5Checks_" + Guid.NewGuid().ToString("N");
var connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("MODULE5_TEST_SQL")
    ?? "Server=.\\SQLEXPRESS;Trusted_Connection=True;TrustServerCertificate=True") { InitialCatalog = name };
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options;
var clock = new TestClock(new DateTimeOffset(2026, 11, 1, 1, 0, 0, TimeSpan.Zero));
int passed = 0;
void Check(bool condition, string title)
{
    if (!condition) throw new Exception("FAIL: " + title);
    Console.WriteLine("PASS: " + title); passed++;
}
async Task Reject(Func<Module5Service, Task> action, string expected, string title)
{
    await using var context = new ApplicationDbContext(options);
    try { await action(new Module5Service(context, clock)); }
    catch (NghiepVuModule5Exception e) when (e.Message.Contains(expected, StringComparison.OrdinalIgnoreCase))
    { Check(true, title); return; }
    throw new Exception("FAIL: " + title + " (không chặn đúng quy tắc)");
}
async Task<int> Save(LichThiFormViewModel model, bool edit = false)
{
    await using var context = new ApplicationDbContext(options);
    return await new Module5Service(context, clock).LuuLichAsync(model, edit);
}
async Task Change(int id, bool complete)
{
    await using var context = new ApplicationDbContext(options);
    await new Module5Service(context, clock).ChuyenTrangThaiLichAsync(id, complete);
}
DateTime start = new(2026, 11, 2, 9, 0, 0);
LichThiFormViewModel Form(int hoSo, string invigilator = "Giám thị A", string room = "A-101") => new()
{
    MaHoSo = hoSo, ThoiGianBatDau = start, ThoiGianKetThuc = start.AddHours(1),
    HinhThucThi = HinhThucThi.TrucTiep, DiaDiemHoacLienKet = "Cơ sở A", PhongThi = room, GiamThi = invigilator
};

await using var setup = new ApplicationDbContext(options);
try
{
    await setup.Database.MigrateAsync();
    var cc = new ChungChi { TenChungChi = "Test", TrangThai = TrangThaiChungChi.DangHoatDong };
    var ky = new KyThi { TenKyThi = "Test", ChungChi = cc, CapDoThi = "A", SoChoThiToiDa = 30 };
    ThiSinh Candidate(string code) => new()
    {
        HoTen = code, Email = code + "@example.com", SoDienThoai = "0900000000", NgaySinh = new DateTime(2004, 1, 1),
        TaiKhoan = new TaiKhoan { TenDangNhap = code, HoTen = code, Email = code + "@example.com", MatKhau = "test", VaiTro = VaiTro.ThiSinh }
    };
    var ts1 = Candidate("ts1"); var ts2 = Candidate("ts2");
    HoSoDangKyThi HoSo(ThiSinh ts, TrangThaiHoSo status = TrangThaiHoSo.DuDieuKien) => new()
        { ThiSinh = ts, KyThi = ky, MucDichDuThi = "Test", NgayNop = new DateTime(2026, 10, 1), TrangThai = status };
    var a = HoSo(ts1); var b = HoSo(ts2); var c = HoSo(ts1); var pending = HoSo(ts2, TrangThaiHoSo.ChoDuyet);
    setup.AddRange(a, b, c, pending);
    await setup.SaveChangesAsync();
    setup.ChangeTracker.Clear();
    await Reject(s => s.LuuLichAsync(Form(pending.MaHoSo), false), "trạng thái", "Không lập lịch hồ sơ Chờ duyệt");
    var bad = Form(a.MaHoSo); bad.ThoiGianKetThuc = start;
    await Reject(s => s.LuuLichAsync(bad, false), "sau", "Chặn khoảng thời gian rỗng");
    bad = Form(a.MaHoSo); bad.ThoiGianBatDau = start.AddDays(-5); bad.ThoiGianKetThuc = start.AddDays(-5).AddHours(1);
    await Reject(s => s.LuuLichAsync(bad, false), "tương lai", "Chặn lịch trong quá khứ");
    bad = Form(a.MaHoSo); bad.PhongThi = " ";
    await Reject(s => s.LuuLichAsync(bad, false), "phòng", "Thi trực tiếp bắt buộc có phòng");
    bad = Form(a.MaHoSo); bad.HinhThucThi = HinhThucThi.TrucTuyen; bad.DiaDiemHoacLienKet = "javascript:alert(1)";
    await Reject(s => s.LuuLichAsync(bad, false), "URL", "Chặn liên kết thi không hợp lệ");
    var id = await Save(Form(a.MaHoSo));
    Check(await setup.HoSoDangKyThis.AsNoTracking().Where(h => h.MaHoSo == a.MaHoSo).Select(h => h.TrangThai).SingleAsync() == TrangThaiHoSo.ChoThi, "Lập lịch chuyển hồ sơ sang Chờ thi");
    await Reject(s => s.LuuLichAsync(Form(a.MaHoSo), false), "trạng thái", "Chặn lập thêm lịch cùng hồ sơ");
    await Reject(s => s.LuuLichAsync(Form(c.MaHoSo, "B", "B"), false), "Thí sinh bị trùng", "Chặn trùng thí sinh ở hồ sơ khác");
    await Reject(s => s.LuuLichAsync(Form(b.MaHoSo, " giám thị a ", "B"), false), "Giám thị bị trùng", "Chặn giám thị trùng không phân biệt hoa/thường");
    await Reject(s => s.LuuLichAsync(Form(b.MaHoSo, "B", " a-101 "), false), "Phòng thi bị trùng", "Chặn trùng phòng");
    var adjacent = Form(b.MaHoSo); adjacent.ThoiGianBatDau = start.AddHours(1); adjacent.ThoiGianKetThuc = start.AddHours(2);
    var adjacentId = await Save(adjacent);
    Check(adjacentId > 0, "Cho phép hai lịch nối tiếp tại mốc kết thúc");
    var edit = Form(a.MaHoSo); edit.MaLichThi = id;
    Check(await Save(edit, true) == id, "Sửa lịch loại chính lịch đó khỏi kiểm tra trùng");
    edit.MaHoSo = c.MaHoSo;
    await Reject(s => s.LuuLichAsync(edit, true), "đổi hồ sơ", "Chặn thay mã hồ sơ khi sửa lịch");
    await Reject(s => s.ChuyenTrangThaiLichAsync(id, true), "sau thời gian", "Chặn hoàn thành lịch chưa kết thúc");
    await Change(adjacentId, false);
    Check(await setup.HoSoDangKyThis.AsNoTracking().Where(h => h.MaHoSo == b.MaHoSo).Select(h => h.TrangThai).SingleAsync() == TrangThaiHoSo.DuDieuKien, "Hủy lịch trả hồ sơ về Đủ điều kiện");
    Check(await Save(adjacent) > 0, "Cho phép lập lại lịch sau hủy");
    await Reject(s => s.ChuyenTrangThaiLichAsync(adjacentId, true), "trạng thái", "Không hoàn thành lịch đã hủy");
    var result = new KetQuaThiFormViewModel { MaHoSo = a.MaHoSo, KetQua = KetQuaCuoiCung.Dat, DiemDanhGia = 80 };
    await Reject(s => s.LuuKetQuaAsync(result, false), "hoàn thành", "Không nhập kết quả khi chưa thi");
    clock.UtcNow = new DateTimeOffset(2026, 11, 3, 1, 0, 0, TimeSpan.Zero);
    await Reject(s => s.ChuyenTrangThaiLichAsync(id, false), "chưa bắt đầu", "Không hủy lịch đã diễn ra");
    await Change(id, true);
    Check(await setup.HoSoDangKyThis.AsNoTracking().Where(h => h.MaHoSo == a.MaHoSo).Select(h => h.TrangThai).SingleAsync() == TrangThaiHoSo.DaThi, "Hoàn thành lịch chuyển hồ sơ sang Đã thi");
    result.DiemDanhGia = 101;
    await Reject(s => s.LuuKetQuaAsync(result, false), "Điểm", "Chặn điểm trên 100");
    result.DiemDanhGia = -1;
    await Reject(s => s.LuuKetQuaAsync(result, false), "Điểm", "Chặn điểm âm");
    result.DiemDanhGia = 80; result.KetQua = null;
    await Reject(s => s.LuuKetQuaAsync(result, false), "xác nhận", "Bắt buộc cán bộ xác nhận kết quả");
    result.KetQua = KetQuaCuoiCung.Dat;
    await using (var db = new ApplicationDbContext(options)) result.MaKetQuaThi = await new Module5Service(db, clock).LuuKetQuaAsync(result, false);
    Check(await setup.HoSoDangKyThis.AsNoTracking().Where(h => h.MaHoSo == a.MaHoSo).Select(h => h.TrangThai).SingleAsync() == TrangThaiHoSo.Dat, "Ghi kết quả đồng bộ trạng thái Đạt");
    await Reject(s => s.LuuKetQuaAsync(result, false), "chưa có kết quả", "Chặn nhập kết quả trùng");
    result.KetQua = KetQuaCuoiCung.KhongDat; result.DiemDanhGia = 35;
    await using (var db = new ApplicationDbContext(options)) await new Module5Service(db, clock).LuuKetQuaAsync(result, true);
    Check(await setup.HoSoDangKyThis.AsNoTracking().Where(h => h.MaHoSo == a.MaHoSo).Select(h => h.TrangThai).SingleAsync() == TrangThaiHoSo.KhongDat, "Sửa kết quả đồng bộ trạng thái Không đạt");
    var stored = await setup.KetQuaThis.AsNoTracking().SingleAsync(k => k.MaKetQuaThi == result.MaKetQuaThi);
    Check(stored.NgayCapNhat == new DateTime(2026, 11, 3, 8, 0, 0), "Ngày cập nhật lấy từ hệ thống theo UTC+7");

    // Kiểm tra truy vấn quyền dữ liệu bằng tài khoản thí sinh thứ hai.
    var http = new DefaultHttpContext { Session = new MemorySession() };
    http.Session.GhiDangNhap(ts2.TaiKhoan!);
    var controllerContext = new ControllerContext { HttpContext = http };
    var svc = new Module5Service(setup, clock);
    var lichController = new LichThiController(setup, svc) { ControllerContext = controllerContext };
    var kqController = new KetQuaThiController(setup, svc) { ControllerContext = controllerContext };
    Check(await lichController.Details(id) is NotFoundResult, "Đổi URL không xem được lịch người khác");
    Check(await kqController.Details(result.MaKetQuaThi) is NotFoundResult, "Đổi URL không xem được kết quả người khác");
    var list = (LichThiIndexViewModel)((ViewResult)await lichController.Index(null, null)).Model!;
    Check(list.DanhSach.All(l => l.MaThiSinh == ts2.MaThiSinh), "Danh sách lịch chỉ chứa dữ liệu của mình");
    http.Session.GhiDangNhap(ts1.TaiKhoan!);
    Check(await lichController.Details(id) is ViewResult && await kqController.Details(result.MaKetQuaThi) is ViewResult, "Thí sinh xem được lịch/kết quả của mình");

    // Dữ liệu không nhất quán phải bị từ chối khi sửa kết quả cuối cùng.
    await setup.HoSoDangKyThis.Where(h => h.MaHoSo == a.MaHoSo).ExecuteUpdateAsync(s => s.SetProperty(h => h.TrangThai, TrangThaiHoSo.DaHuy));
    await Reject(s => s.LuuKetQuaAsync(result, true), "không khớp", "Chặn sửa kết quả khi trạng thái hồ sơ không khớp");
    Console.WriteLine($"SUCCESS: {passed} checks passed on SQL Server.");
}
finally
{
    // Chỉ xóa database ngẫu nhiên vừa được chính tiến trình kiểm thử tạo ra.
    if (setup.Database.GetDbConnection().Database == name && name.StartsWith("QuanLyKyThi_Module5Checks_"))
        await setup.Database.EnsureDeletedAsync();
}

sealed class TestClock(DateTimeOffset initial) : TimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = initial;
    public override DateTimeOffset GetUtcNow() => UtcNow;
}

sealed class MemorySession : ISession
{
    private readonly Dictionary<string, byte[]> values = [];
    public bool IsAvailable => true;
    public string Id => "test-session";
    public IEnumerable<string> Keys => values.Keys;
    public void Clear() => values.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Remove(string key) => values.Remove(key);
    public void Set(string key, byte[] value) => values[key] = value;
    public bool TryGetValue(string key, out byte[] value) => values.TryGetValue(key, out value!);
}
