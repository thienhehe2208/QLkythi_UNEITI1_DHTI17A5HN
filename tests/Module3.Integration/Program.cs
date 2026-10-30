// Họ và tên/Mã sinh viên: CHƯA CUNG CẤP.
// Nội dung: kiểm thử Module 3 trên SQL Server thật; fixture nằm riêng, không tạo endpoint giả trong ứng dụng.
using System.Data.Common;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuanLyKyThi_UNETI1_TI17A5HN.Controllers;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.Services;
using QuanLyKyThi_UNETI1_TI17A5HN.Services.Module3;
using QuanLyKyThi_UNETI1_TI17A5HN.ViewModels;
using QuanLyKyThi_UNETI1_TI17A5HN.ViewModels.Module3;

var input = Environment.GetEnvironmentVariable("MODULE3_TEST_CONNECTION")
    ?? throw new InvalidOperationException("Cần MODULE3_TEST_CONNECTION trỏ SQL Server kiểm thử; không dùng Database của nhóm.");
// Luôn tạo một Database fixture mới; không xóa/ghi đè Database nhận trong connection string.
var connection = new SqlConnectionStringBuilder(input) { InitialCatalog = "Module3_Test_" + Guid.NewGuid().ToString("N") };
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection.ConnectionString).Options;
ApplicationDbContext Db() => new(options);
var clock = new TestClock(new DateTimeOffset(2026, 10, 30, 3, 0, 0, TimeSpan.Zero));
Module3Service Service(ApplicationDbContext db) => new(db, clock, NullLogger<Module3Service>.Instance);
var passed = new List<string>();
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    passed.Add(name); Console.WriteLine("PASS: " + name);
}
async Task Reject(Func<Task> action, string name, string? code = null)
{
    try { await action(); }
    catch (NghiepVuModule3Exception e) { Check(code == null || e.MaLoi == code, name); return; }
    throw new Exception("FAIL (không bị chặn): " + name);
}
HoSoCaNhanFormViewModel Profile(string name = "Thí sinh kiểm thử") => new()
{
    HoTen = name, NgaySinh = new DateTime(2002, 4, 15), GioiTinh = GioiTinh.Nu,
    Email = "module3@example.test", SoDienThoai = "0912345678", SoLanThiTruocDay = 0
};
NopHoSoThiViewModel Form(int id) => new() { MaKyThi = id, MucDichDuThi = "Bổ sung chứng chỉ phục vụ học tập.", GhiChu = "Kiểm thử Module 3" };

int a, b, admin, staff, fresh, locked, cc;
await using (var db = Db())
{
    await db.Database.MigrateAsync();
    var hasher = new MatKhauService();
    TaiKhoan Account(string name, VaiTro role, TrangThaiTaiKhoan state = TrangThaiTaiKhoan.HoatDong)
    {
        var t = new TaiKhoan { TenDangNhap = "m3_" + name, HoTen = "Module 3 " + name, Email = name + "@example.test", VaiTro = role, TrangThai = state };
        t.MatKhau = hasher.BamMatKhau(t, "Module3@Test123"); return t;
    }
    var ta = Account("a", VaiTro.ThiSinh); var tb = Account("b", VaiTro.ThiSinh);
    var ad = Account("admin", VaiTro.Admin); var cb = Account("staff", VaiTro.CanBoKhaoThi);
    var fr = Account("fresh", VaiTro.ThiSinh); var lk = Account("locked", VaiTro.ThiSinh, TrangThaiTaiKhoan.BiKhoa);
    db.TaiKhoans.AddRange(ta, tb, ad, cb, fr, lk);
    var certificate = new ChungChi { TenChungChi = "Chứng chỉ kiểm thử Module 3" }; db.ChungChis.Add(certificate);
    await db.SaveChangesAsync();
    a = ta.MaTaiKhoan; b = tb.MaTaiKhoan; admin = ad.MaTaiKhoan; staff = cb.MaTaiKhoan; fresh = fr.MaTaiKhoan; locked = lk.MaTaiKhoan; cc = certificate.MaChungChi;
}
async Task<int> Exam(TrangThaiKyThi state = TrangThaiKyThi.DangMoDangKy, DateTime? start = null, DateTime? end = null)
{
    await using var db = Db(); var now = Service(db).BayGio;
    var k = new KyThi { TenKyThi = "M3 " + Guid.NewGuid().ToString("N"), MaChungChi = cc, SoChoThiToiDa = 1,
        CapDoThi = "Cơ bản", NgayBatDauDangKy = start ?? now.AddDays(-1), HanDangKy = end ?? now.AddDays(5), TrangThai = state };
    db.KyThis.Add(k); await db.SaveChangesAsync(); return k.MaKyThi;
}
async Task<int> Submit(int account, int exam)
{
    await using var db = Db(); return await Service(db).NopHoSoAsync(account, Form(exam));
}
async Task SetStatus(int id, TrangThaiHoSo state)
{
    await using var db = Db(); var h = await db.HoSoDangKyThis.FindAsync(id); h!.TrangThai = state; await db.SaveChangesAsync();
}
async Task<int> Fixture(int account, int exam, TrangThaiHoSo state)
{
    await using var db = Db();
    var candidate = await db.ThiSinhs.SingleAsync(t => t.MaTaiKhoan == account);
    var h = new HoSoDangKyThi { MaThiSinh = candidate.MaThiSinh, MaKyThi = exam, NgayNop = Service(db).BayGio.AddDays(-1), MucDichDuThi = "Fixture riêng", TrangThai = state };
    db.HoSoDangKyThis.Add(h); await db.SaveChangesAsync(); return h.MaHoSo;
}

await using (var db = Db())
{
    var s = Service(db);
    Check(!(await s.DocHoSoCaNhanAsync(fresh)).DaKhaiBao && !await db.ThiSinhs.AnyAsync(), "TC02: GET hồ sơ chưa khai báo không ghi dữ liệu");
    await s.LuuHoSoCaNhanAsync(a, Profile("Thí sinh A"));
    await s.LuuHoSoCaNhanAsync(b, Profile("Thí sinh B"));
    var original = await s.DocHoSoCaNhanAsync(a);
    await s.LuuHoSoCaNhanAsync(a, Profile("A cập nhật"));
    Check((await s.DocHoSoCaNhanAsync(a)).MaThiSinh == original.MaThiSinh && await db.ThiSinhs.CountAsync() == 2, "TC01/02: tạo và cập nhật giữ đúng liên kết, không nhân bản");
    Check((await db.TaiKhoans.FindAsync(a))!.HoTen == "Module 3 a", "TC10: cập nhật cá nhân không sửa TaiKhoan");
    foreach (var (name, change) in new (string, Action<HoSoCaNhanFormViewModel>)[]
    {
        ("Họ tên trắng", m => m.HoTen = "   "), ("Email sai", m => m.Email = "bad"), ("Email thiếu", m => m.Email = ""),
        ("Điện thoại sai", m => m.SoDienThoai = "abcdef"), ("Điện thoại thiếu", m => m.SoDienThoai = ""),
        ("Ngày sinh thiếu", m => m.NgaySinh = null), ("Ngày sinh mặc định", m => m.NgaySinh = DateTime.MinValue),
        ("Ngày sinh tương lai", m => m.NgaySinh = s.BayGio.AddDays(1)), ("Giới tính sai enum", m => m.GioiTinh = (GioiTinh)99),
        ("Giới tính thiếu", m => m.GioiTinh = null), ("Số lần âm", m => m.SoLanThiTruocDay = -1), ("Số lần thiếu", m => m.SoLanThiTruocDay = null),
        ("Họ tên quá dài", m => m.HoTen = new string('a',101)), ("Điện thoại quá dài", m => m.SoDienThoai = new string('1',21)),
        ("Email quá dài", m => m.Email = new string('a',145)+"@ex.test"), ("Địa chỉ quá dài", m => m.DiaChi = new string('a',501)),
        ("Trình độ quá dài", m => m.TrinhDoHienTai = new string('a',101)), ("Nghề nghiệp quá dài", m => m.NgheNghiep = new string('a',151)),
        ("Chứng chỉ quá dài", m => m.ChungChiDaCo = new string('a',501))
    })
    { var m = Profile(); change(m); await Reject(() => s.LuuHoSoCaNhanAsync(a,m), "TC03–09: " + name); }
    await Reject(() => s.LuuHoSoCaNhanAsync(locked, Profile()), "TC15: tài khoản khóa bị chặn tại service", "TAI_KHOAN");
    await Reject(() => s.LuuHoSoCaNhanAsync(staff, Profile()), "TC54: cán bộ không tự khai báo thành thí sinh", "TAI_KHOAN");
}
// Hai request độc lập cùng khai báo lần đầu: unique FK và transaction giữ đúng một hồ sơ.
int profileRaceAccount;
await using (var db = Db())
{
    var account = new TaiKhoan { TenDangNhap = "m3_profile_race", HoTen = "Profile race", Email = "race@example.test", MatKhau = "fixture-only", VaiTro = VaiTro.ThiSinh };
    db.TaiKhoans.Add(account); await db.SaveChangesAsync(); profileRaceAccount = account.MaTaiKhoan;
}
async Task ProfileRace()
{
    await using var db = Db();
    try { await Service(db).LuuHoSoCaNhanAsync(profileRaceAccount, Profile()); }
    catch (Exception e) when (LoiDuLieuModule3.LaLoiDatabase(e)) { /* Có thể là deadlock victim. */ }
}
await Task.WhenAll(Task.Run(ProfileRace), Task.Run(ProfileRace));
await using (var db = Db()) Check(await db.ThiSinhs.CountAsync(t => t.MaTaiKhoan == profileRaceAccount) == 1, "TC12: hai request khai báo chỉ có một hồ sơ cá nhân");
var valid = await Exam();
await using (var db = Db())
{
    await Reject(() => Service(db).NopHoSoAsync(fresh, Form(valid)), "TC16: thiếu hồ sơ cá nhân", "CHUA_CO_CA_NHAN");
    foreach (var form in new[] { new NopHoSoThiViewModel(), new NopHoSoThiViewModel { MaKyThi = valid, MucDichDuThi = " " }, new NopHoSoThiViewModel { MaKyThi = valid, MucDichDuThi = new string('x',1001) }, new NopHoSoThiViewModel { MaKyThi = valid, MucDichDuThi = "OK", GhiChu = new string('x',1001) } })
        await Reject(() => Service(db).NopHoSoAsync(a, form), "TC19/31: form nộp không hợp lệ", "FORM_KHONG_HOP_LE");
}
var submitted = await Submit(a, valid);
await using (var db = Db())
{
    var h = await db.HoSoDangKyThis.SingleAsync(h => h.MaHoSo == submitted);
    Check(h.TrangThai == TrangThaiHoSo.ChoDuyet && h.NgayNop == Service(db).BayGio && h.NgayXuLy == null && h.NhanXetXetDuyet == null && h.MaNguoiXuLy == null, "TC13: hồ sơ Chờ duyệt, ngày hệ thống, xét duyệt null");
    Check(h.MaThiSinh == (await db.ThiSinhs.SingleAsync(t=>t.MaTaiKhoan==a)).MaThiSinh, "TC13: hồ sơ đúng chủ sở hữu");
    Check(await Service(db).DocChiTietHoSoAsync(b, submitted) == null, "TC34: B không đọc được chi tiết A");
    await Reject(() => Service(db).HuyHoSoAsync(b, submitted), "TC46: B không hủy được A", "KHONG_TIM_THAY");
}
await Submit(b, valid); Check(true, "TC27: thí sinh khác cùng kỳ thi được xét độc lập");
await Submit(a, await Exam()); Check(true, "TC26: cùng thí sinh kỳ thi khác được xét độc lập");
await using (var db = Db()) { var t = await db.ThiSinhs.SingleAsync(t => t.MaTaiKhoan == b); t.SoDienThoai = ""; await db.SaveChangesAsync(); }
await using (var db = Db()) await Reject(() => Service(db).NopHoSoAsync(b, Form(valid)), "TC18: dữ liệu cá nhân cũ thiếu thông tin bị chặn", "CA_NHAN_CHUA_DU");
await using (var db = Db()) await Service(db).LuuHoSoCaNhanAsync(b, Profile("Thí sinh B"));
foreach (var state in QuyTacHoSoModule3.DangXuLy)
{
    await SetStatus(submitted,state);
    await using var db = Db(); await Reject(() => Service(db).NopHoSoAsync(a,Form(valid)), "TC25: chống trùng " + state, "TRUNG_HO_SO");
}
foreach (var state in new[] { TrangThaiHoSo.DaHuy, TrangThaiHoSo.KhongDuDieuKien, TrangThaiHoSo.Dat, TrangThaiHoSo.KhongDat })
{
    var exam = await Exam(); await Fixture(a,exam,state); await Submit(a,exam);
    Check(true,"TC28/29: nộp mới sau " + state + " giữ lịch sử theo TXT");
}
foreach (var state in new[] { TrangThaiKyThi.ChuaMo, TrangThaiKyThi.TamDung, TrangThaiKyThi.DaDong })
{
    var exam = await Exam(state); await using var db = Db();
    await Reject(() => Service(db).NopHoSoAsync(a,Form(exam)), "TC20: chặn kỳ thi " + state,"KY_THI_KHONG_NHAN");
}
var now = clock.GetUtcNow().ToOffset(TimeSpan.FromHours(7)).DateTime;
foreach (var (start,end,name) in new[] { (now.AddHours(1),now.AddDays(2),"TC21: chưa tới hạn nhận"), (now.AddDays(-2),now.AddTicks(-1),"TC22: vừa quá hạn") })
{
    var exam = await Exam(start:start,end:end); await using var db = Db();
    await Reject(() => Service(db).NopHoSoAsync(a,Form(exam)), name,"KY_THI_KHONG_NHAN");
}
await Submit(a,await Exam(start:now,end:now)); Check(true,"TC23: đúng lúc bắt đầu và hạn cuối được nộp");
var stale = await Exam(end:now.AddMinutes(1));
await using (var db = Db()) { var m=Form(stale); await Service(db).NapFormNopAsync(a,m); Check(m.KyThi?.ConNhanHoSo == true,"TC24: GET còn hạn"); }
clock.Now = clock.Now.AddMinutes(2);
await using (var db = Db()) await Reject(()=>Service(db).NopHoSoAsync(a,Form(stale)),"TC24: POST kiểm tra lại hạn","KY_THI_KHONG_NHAN");
clock.Now = clock.Now.AddMinutes(-2);
await using (var db = Db())
{
    await Service(db).CapNhatTrangThaiAsync(admin,(await db.ThiSinhs.SingleAsync(t=>t.MaTaiKhoan==a)).MaThiSinh,TrangThaiThiSinh.NgungHoatDong);
}
await using (var db = Db())
{
    await Reject(()=>Service(db).NopHoSoAsync(a,Form(valid)),"TC17: ngừng hoạt động chặn nộp","THI_SINH_NGUNG");
    await Service(db).LuuHoSoCaNhanAsync(a,Profile("A đang ngừng"));
    Check((await Service(db).DocHoSoCaNhanAsync(a)).TrangThai==TrangThaiThiSinh.NgungHoatDong,"TC11: sửa cá nhân không tự kích hoạt trạng thái");
    Check((await db.TaiKhoans.FindAsync(a))!.TrangThai==TrangThaiTaiKhoan.HoatDong,"TC53: ngừng thí sinh không khóa tài khoản");
    var candidateId = (await Service(db).DocHoSoCaNhanAsync(a)).MaThiSinh;
    await Reject(()=>Service(db).CapNhatTrangThaiAsync(staff,candidateId,TrangThaiThiSinh.HoatDong),"TC54: cán bộ không đổi trạng thái Admin","TAI_KHOAN");
}
await using (var db=Db()) await Service(db).CapNhatTrangThaiAsync(admin,(await Service(db).DocHoSoCaNhanAsync(a)).MaThiSinh,TrangThaiThiSinh.HoatDong);
var cancel = await Submit(a,await Exam());
await using(var db=Db()) await Service(db).HuyHoSoAsync(a,cancel);
await using(var db=Db()) Check((await db.HoSoDangKyThis.FindAsync(cancel))!.TrangThai==TrangThaiHoSo.DaHuy,"TC42: hủy giữ bản ghi");
var approved=await Fixture(a,await Exam(),TrangThaiHoSo.DuDieuKien);
await using(var db=Db()) { var h=await db.HoSoDangKyThis.FindAsync(approved);h!.NgayXuLy=now;h.NhanXetXetDuyet="Đã xét";h.MaNguoiXuLy=staff;await db.SaveChangesAsync(); }
await using(var db=Db()) await Service(db).HuyHoSoAsync(a,approved);
await using(var db=Db()) { var h=await db.HoSoDangKyThis.FindAsync(approved);Check(h!.NgayXuLy==now&&h.NhanXetXetDuyet=="Đã xét"&&h.MaNguoiXuLy==staff,"TC43: hủy giữ ngày, người và nhận xét xét duyệt"); }
foreach(var state in new[]{TrangThaiHoSo.ChoThi,TrangThaiHoSo.DaThi,TrangThaiHoSo.Dat,TrangThaiHoSo.KhongDat,TrangThaiHoSo.DaHuy,TrangThaiHoSo.KhongDuDieuKien})
{
    var id=await Fixture(a,await Exam(),state);await using var db=Db();
    await Reject(()=>Service(db).HuyHoSoAsync(a,id),"TC45/51: chặn hủy " + state,"KHONG_THE_HUY");
}
var withSchedule=await Fixture(a,await Exam(),TrangThaiHoSo.DuDieuKien);
await using(var db=Db()) { db.LichThis.Add(new LichThi{MaHoSo=withSchedule,ThoiGianBatDau=now.AddDays(2),ThoiGianKetThuc=now.AddDays(2).AddHours(1),GiamThi="Test",DiaDiemHoacLienKet="Test",PhongThi="TEST",TrangThai=TrangThaiLichThi.DaLenLich});await db.SaveChangesAsync(); }
await using(var db=Db()) await Reject(()=>Service(db).HuyHoSoAsync(a,withSchedule),"TC44/48: có lịch hiệu lực dù trạng thái lệch","KHONG_THE_HUY");
await using(var db=Db()) { var l=await db.LichThis.SingleAsync(l=>l.MaHoSo==withSchedule);l.TrangThai=TrangThaiLichThi.DaHuy;await db.SaveChangesAsync(); }
await using(var db=Db()) { var m=await Service(db).DocChiTietHoSoAsync(a,withSchedule);Check(m!.LichThis.Count==1&&m.CoTheHuy,"TC38/50: lịch hủy là lịch sử, hồ sơ được hủy");await Service(db).HuyHoSoAsync(a,withSchedule); }
var result=await Fixture(a,await Exam(),TrangThaiHoSo.Dat);
await using(var db=Db()) { db.KetQuaThis.Add(new KetQuaThi{MaHoSo=result,KetQua=KetQuaCuoiCung.Dat,DiemDanhGia=85,NgayCapNhat=now,NhanXet="Kết quả kiểm thử"});await db.SaveChangesAsync(); }
await using(var db=Db()) Check((await Service(db).DocChiTietHoSoAsync(a,result))!.KetQua?.DiemDanhGia==85,"TC40: đọc kết quả khớp trạng thái");
await SetStatus(result,TrangThaiHoSo.DuDieuKien);
await using(var db=Db()) { Check((await Service(db).DocChiTietHoSoAsync(a,result))!.KetQua==null,"TC41: không lộ kết quả chưa khớp/công bố");await Reject(()=>Service(db).HuyHoSoAsync(a,result),"TC44: có kết quả thì chặn hủy","KHONG_THE_HUY"); }

var duplicateExam=await Exam();
async Task<bool> RaceSubmit() { await using var db=Db();try { await Service(db).NopHoSoAsync(a,Form(duplicateExam));return true; }catch(Exception e) when(e is NghiepVuModule3Exception || LoiDuLieuModule3.LaLoiDatabase(e)){return false;} }
var twins=await Task.WhenAll(Task.Run(RaceSubmit),Task.Run(RaceSubmit));
await using(var db=Db()) Check(twins.Count(x=>x)==1&&await db.HoSoDangKyThis.CountAsync(h=>h.MaKyThi==duplicateExam)==1,"TC32: hai DbContext nộp đồng thời chỉ commit một hồ sơ");
var raceId=await Fixture(a,await Exam(),TrangThaiHoSo.DuDieuKien);
async Task<bool> CancelRace(){await using var db=Db();try{await Service(db).HuyHoSoAsync(a,raceId);return true;}catch(Exception e)when(e is NghiepVuModule3Exception || LoiDuLieuModule3.LaLoiDatabase(e)){return false;}}
async Task<bool> ScheduleRace(){await using var db=Db();try{await new Module5Service(db,clock).LuuLichAsync(new LichThiFormViewModel{MaHoSo=raceId,ThoiGianBatDau=now.AddDays(3),ThoiGianKetThuc=now.AddDays(3).AddHours(1),HinhThucThi=HinhThucThi.TrucTiep,PhongThi="RACE",GiamThi="RACE",DiaDiemHoacLienKet="RACE"},false);return true;}catch(Exception e)when(e is NghiepVuModule3Exception or NghiepVuModule5Exception || LoiDuLieuModule3.LaLoiDatabase(e)){return false;}}
var racing=await Task.WhenAll(Task.Run(CancelRace),Task.Run(ScheduleRace));
await using(var db=Db()) { var h=await db.HoSoDangKyThis.FindAsync(raceId);var live=await db.LichThis.AnyAsync(l=>l.MaHoSo==raceId&&l.TrangThai!=TrangThaiLichThi.DaHuy);Check(racing.Count(x=>x)==1&&!(h!.TrangThai==TrangThaiHoSo.DaHuy&&live),"TC49: hủy đồng thời Module 5 lập lịch giữ nhất quán"); }

// Fixture cho kiểm thử HTTP, không xuất connection string/mật khẩu trong báo cáo.
var httpExam = await Exam(start:DateTime.Now.AddDays(-10),end:DateTime.Now.AddDays(10));
var metadataPath=Environment.GetEnvironmentVariable("MODULE3_TEST_METADATA");
if(metadataPath!=null) await File.WriteAllTextAsync(metadataPath,JsonSerializer.Serialize(new{database=connection.InitialCatalog,a,b,admin,staff,fresh,locked,httpExam,result,submitted,passed=passed.Count}));
Console.WriteLine($"RESULT: {passed.Count} assertions passed; SQL Server Database {connection.InitialCatalog}");

sealed class TestClock(DateTimeOffset now):TimeProvider
{
    public DateTimeOffset Now {get;set;}=now;
    public override DateTimeOffset GetUtcNow()=>Now;
}
