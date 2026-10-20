// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Áp dụng migration và nạp dữ liệu mẫu Module 1
//   (tài khoản Admin / Cán bộ khảo thí / Thí sinh demo, 6 chứng chỉ).
//   Chạy lặp lại an toàn: chỉ thêm bản ghi chưa có (so theo TenDangNhap / TenChungChi).
//   Thành viên module khác bổ sung dữ liệu mẫu của mình vào các hàm riêng bên dưới.
// ============================================================

using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.Services;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Data
{
    public static class DbInitializer
    {
        // Mật khẩu thử nghiệm (chỉ dùng cho dữ liệu demo, được hash trước khi lưu DB).
        public const string MatKhauAdmin = "Admin@123";
        public const string MatKhauCanBo = "CanBo@123";
        public const string MatKhauThiSinh = "ThiSinh@123";

        public static async Task KhoiTaoAsync(IServiceProvider services, bool tuDongMigrate)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var matKhauService = scope.ServiceProvider.GetRequiredService<IMatKhauService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            if (!context.Database.GetMigrations().Any())
            {
                logger.LogWarning("Chưa có migration nào. Hãy chạy: dotnet ef migrations add InitialCreate");
                return;
            }

            if (tuDongMigrate)
                await context.Database.MigrateAsync();

            await NapTaiKhoanAsync(context, matKhauService);
            await NapChungChiAsync(context);
            await NapKyThiAsync(context);
            await NapThiSinhVaHoSoAsync(context, matKhauService);
            await NapLichThiVaKetQuaAsync(context);
        }

        private static async Task NapTaiKhoanAsync(ApplicationDbContext context, IMatKhauService matKhauService)
        {
            var mau = new (string TenDangNhap, string HoTen, string Email, VaiTro VaiTro, TrangThaiTaiKhoan TrangThai, string MatKhau)[]
            {
                ("admin",     "Quản trị viên hệ thống", "admin@quanlykythi.local",   VaiTro.Admin,        TrangThaiTaiKhoan.HoatDong, MatKhauAdmin),
                ("admin2",    "Quản trị viên dự phòng", "admin2@quanlykythi.local",  VaiTro.Admin,        TrangThaiTaiKhoan.HoatDong, MatKhauAdmin),
                ("canbo1",    "Cán bộ khảo thí 1",      "canbo1@quanlykythi.local",  VaiTro.CanBoKhaoThi, TrangThaiTaiKhoan.HoatDong, MatKhauCanBo),
                ("canbo2",    "Cán bộ khảo thí 2",      "canbo2@quanlykythi.local",  VaiTro.CanBoKhaoThi, TrangThaiTaiKhoan.HoatDong, MatKhauCanBo),
                ("canbo3",    "Cán bộ khảo thí 3",      "canbo3@quanlykythi.local",  VaiTro.CanBoKhaoThi, TrangThaiTaiKhoan.HoatDong, MatKhauCanBo),
                ("thisinh01", "Thí sinh demo 01",       "thisinh01@quanlykythi.local", VaiTro.ThiSinh,    TrangThaiTaiKhoan.HoatDong, MatKhauThiSinh),
                ("thisinh02", "Thí sinh demo 02",       "thisinh02@quanlykythi.local", VaiTro.ThiSinh,    TrangThaiTaiKhoan.HoatDong, MatKhauThiSinh),
                ("thisinh_khoa", "Thí sinh bị khóa (demo)", "thisinh.khoa@quanlykythi.local", VaiTro.ThiSinh, TrangThaiTaiKhoan.BiKhoa, MatKhauThiSinh),
            };

            var daCo = await context.TaiKhoans.Select(t => t.TenDangNhap.ToLower()).ToListAsync();

            foreach (var m in mau.Where(m => !daCo.Contains(m.TenDangNhap.ToLower())))
            {
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = m.TenDangNhap,
                    HoTen = m.HoTen,
                    Email = m.Email,
                    VaiTro = m.VaiTro,
                    TrangThai = m.TrangThai
                };
                taiKhoan.MatKhau = matKhauService.BamMatKhau(taiKhoan, m.MatKhau);
                context.TaiKhoans.Add(taiKhoan);
            }

            await context.SaveChangesAsync();
        }

        private static async Task NapChungChiAsync(ApplicationDbContext context)
        {
            var mau = new[]
            {
                new ChungChi { TenChungChi = "Chứng chỉ Ứng dụng CNTT cơ bản",
                    MoTa = "Chuẩn kỹ năng sử dụng công nghệ thông tin cơ bản (Thông tư 03/2014/TT-BTTTT).",
                    EmailLienHe = "cntt@quanlykythi.local", TrangThai = TrangThaiChungChi.DangHoatDong },
                new ChungChi { TenChungChi = "Chứng chỉ Ứng dụng CNTT nâng cao",
                    MoTa = "Chuẩn kỹ năng sử dụng công nghệ thông tin nâng cao.",
                    EmailLienHe = "cntt@quanlykythi.local", TrangThai = TrangThaiChungChi.DangHoatDong },
                new ChungChi { TenChungChi = "Tiếng Anh VSTEP (B1-C1)",
                    MoTa = "Đánh giá năng lực tiếng Anh theo Khung năng lực ngoại ngữ 6 bậc dùng cho Việt Nam.",
                    EmailLienHe = "ngoaingu@quanlykythi.local", TrangThai = TrangThaiChungChi.DangHoatDong },
                new ChungChi { TenChungChi = "TOEIC Listening & Reading",
                    MoTa = "Bài thi đánh giá kỹ năng nghe - đọc tiếng Anh trong môi trường làm việc.",
                    EmailLienHe = "ngoaingu@quanlykythi.local", TrangThai = TrangThaiChungChi.DangHoatDong },
                new ChungChi { TenChungChi = "Kỹ năng lập trình .NET",
                    MoTa = "Chứng chỉ nội bộ đánh giá kỹ năng lập trình C#/ASP.NET Core.",
                    EmailLienHe = null, TrangThai = TrangThaiChungChi.DangHoatDong },
                new ChungChi { TenChungChi = "Tin học văn phòng MOS (cũ)",
                    MoTa = "Chứng chỉ đã ngừng tổ chức - dùng để kiểm thử trạng thái Ngừng hoạt động.",
                    EmailLienHe = "cntt@quanlykythi.local", TrangThai = TrangThaiChungChi.NgungHoatDong },
            };

            var daCo = await context.ChungChis.Select(c => c.TenChungChi.ToLower()).ToListAsync();
            context.ChungChis.AddRange(mau.Where(c => !daCo.Contains(c.TenChungChi.ToLower())));
            await context.SaveChangesAsync();
        }

        // Module 2 - Nguyễn Tiến Quang. Mã sinh viên cần bổ sung theo bảng phân công.
        // Nạp 15 kỳ thi đa dạng trạng thái/thời hạn để kiểm thử tìm kiếm, lọc và phân trang.
        private static async Task NapKyThiAsync(ApplicationDbContext context)
        {
            var chungChi = await context.ChungChis
                .AsNoTracking()
                .ToDictionaryAsync(c => c.TenChungChi, c => c.MaChungChi);

            int Ma(string ten) => chungChi[ten];
            var homNay = DateTime.Today;
            var mau = new[]
            {
                new KyThi { TenKyThi = "Ứng dụng CNTT cơ bản đợt 1", MaChungChi = Ma("Chứng chỉ Ứng dụng CNTT cơ bản"), SoChoThiToiDa = 60, CapDoThi = "Cơ bản", DiemDauVaoToiThieu = 0, NgayBatDauDangKy = homNay.AddDays(-5), HanDangKy = homNay.AddDays(10).AddHours(23).AddMinutes(59), MoTaKyThi = "Kỳ thi cấp chứng chỉ ứng dụng CNTT cơ bản.", DieuKienDuThi = "Hồ sơ cá nhân đầy đủ.", TrangThai = TrangThaiKyThi.DangMoDangKy },
                new KyThi { TenKyThi = "Ứng dụng CNTT cơ bản đợt 2", MaChungChi = Ma("Chứng chỉ Ứng dụng CNTT cơ bản"), SoChoThiToiDa = 50, CapDoThi = "Cơ bản", DiemDauVaoToiThieu = 0, NgayBatDauDangKy = homNay.AddDays(15), HanDangKy = homNay.AddDays(30), MoTaKyThi = "Đợt thi dự kiến tiếp theo.", DieuKienDuThi = "Hồ sơ cá nhân đầy đủ.", TrangThai = TrangThaiKyThi.ChuaMo },
                new KyThi { TenKyThi = "Ứng dụng CNTT cơ bản đợt cũ", MaChungChi = Ma("Chứng chỉ Ứng dụng CNTT cơ bản"), SoChoThiToiDa = 45, CapDoThi = "Cơ bản", DiemDauVaoToiThieu = 0, NgayBatDauDangKy = homNay.AddDays(-60), HanDangKy = homNay.AddDays(-40), MoTaKyThi = "Dữ liệu kiểm thử kỳ thi đã đóng.", DieuKienDuThi = "Hồ sơ cá nhân đầy đủ.", TrangThai = TrangThaiKyThi.DaDong },
                new KyThi { TenKyThi = "Ứng dụng CNTT nâng cao tháng này", MaChungChi = Ma("Chứng chỉ Ứng dụng CNTT nâng cao"), SoChoThiToiDa = 40, CapDoThi = "Nâng cao", DiemDauVaoToiThieu = 5, NgayBatDauDangKy = homNay.AddDays(-3), HanDangKy = homNay.AddDays(12), MoTaKyThi = "Đánh giá kỹ năng ứng dụng CNTT nâng cao.", DieuKienDuThi = "Có kiến thức CNTT cơ bản.", TrangThai = TrangThaiKyThi.DangMoDangKy },
                new KyThi { TenKyThi = "Ứng dụng CNTT nâng cao tạm dừng", MaChungChi = Ma("Chứng chỉ Ứng dụng CNTT nâng cao"), SoChoThiToiDa = 35, CapDoThi = "Nâng cao", DiemDauVaoToiThieu = 5, NgayBatDauDangKy = homNay.AddDays(-7), HanDangKy = homNay.AddDays(7), MoTaKyThi = "Tạm dừng tiếp nhận để điều chỉnh kế hoạch.", DieuKienDuThi = "Có kiến thức CNTT cơ bản.", TrangThai = TrangThaiKyThi.TamDung },
                new KyThi { TenKyThi = "VSTEP B1 tháng này", MaChungChi = Ma("Tiếng Anh VSTEP (B1-C1)"), SoChoThiToiDa = 100, CapDoThi = "B1", DiemDauVaoToiThieu = 3, NgayBatDauDangKy = homNay.AddDays(-10), HanDangKy = homNay.AddDays(5), MoTaKyThi = "Kỳ thi đánh giá năng lực tiếng Anh bậc B1.", DieuKienDuThi = "Hoàn thành hồ sơ và lệ phí theo quy định.", TrangThai = TrangThaiKyThi.DangMoDangKy },
                new KyThi { TenKyThi = "VSTEP B2 tháng sau", MaChungChi = Ma("Tiếng Anh VSTEP (B1-C1)"), SoChoThiToiDa = 90, CapDoThi = "B2", DiemDauVaoToiThieu = 4, NgayBatDauDangKy = homNay.AddDays(10), HanDangKy = homNay.AddDays(25), MoTaKyThi = "Kỳ thi đánh giá năng lực tiếng Anh bậc B2.", DieuKienDuThi = "Hoàn thành hồ sơ và lệ phí theo quy định.", TrangThai = TrangThaiKyThi.ChuaMo },
                new KyThi { TenKyThi = "VSTEP C1 đợt trước", MaChungChi = Ma("Tiếng Anh VSTEP (B1-C1)"), SoChoThiToiDa = 70, CapDoThi = "C1", DiemDauVaoToiThieu = 5, NgayBatDauDangKy = homNay.AddDays(-80), HanDangKy = homNay.AddDays(-55), MoTaKyThi = "Kỳ thi đã kết thúc nhận hồ sơ.", DieuKienDuThi = "Đáp ứng điều kiện hồ sơ theo thông báo.", TrangThai = TrangThaiKyThi.DaDong },
                new KyThi { TenKyThi = "TOEIC 450 tháng này", MaChungChi = Ma("TOEIC Listening & Reading"), SoChoThiToiDa = 120, CapDoThi = "450+", DiemDauVaoToiThieu = 0, NgayBatDauDangKy = homNay.AddDays(-2), HanDangKy = homNay.AddDays(14), MoTaKyThi = "Bài thi TOEIC Listening & Reading.", DieuKienDuThi = "Có giấy tờ tùy thân hợp lệ.", TrangThai = TrangThaiKyThi.DangMoDangKy },
                new KyThi { TenKyThi = "TOEIC 650 đợt tiếp theo", MaChungChi = Ma("TOEIC Listening & Reading"), SoChoThiToiDa = 100, CapDoThi = "650+", DiemDauVaoToiThieu = 3, NgayBatDauDangKy = homNay.AddDays(20), HanDangKy = homNay.AddDays(40), MoTaKyThi = "Đợt thi TOEIC dự kiến.", DieuKienDuThi = "Có giấy tờ tùy thân hợp lệ.", TrangThai = TrangThaiKyThi.ChuaMo },
                new KyThi { TenKyThi = "TOEIC 750 đợt đã đóng", MaChungChi = Ma("TOEIC Listening & Reading"), SoChoThiToiDa = 80, CapDoThi = "750+", DiemDauVaoToiThieu = 4, NgayBatDauDangKy = homNay.AddDays(-50), HanDangKy = homNay.AddDays(-30), MoTaKyThi = "Dữ liệu kiểm thử kỳ thi hết hạn.", DieuKienDuThi = "Có giấy tờ tùy thân hợp lệ.", TrangThai = TrangThaiKyThi.DaDong },
                new KyThi { TenKyThi = "Lập trình .NET cơ bản", MaChungChi = Ma("Kỹ năng lập trình .NET"), SoChoThiToiDa = 40, CapDoThi = "Cơ bản", DiemDauVaoToiThieu = 0, NgayBatDauDangKy = homNay.AddDays(-1), HanDangKy = homNay.AddDays(18), MoTaKyThi = "Đánh giá kiến thức C# và ASP.NET Core cơ bản.", DieuKienDuThi = "Biết lập trình C# cơ bản.", TrangThai = TrangThaiKyThi.DangMoDangKy },
                new KyThi { TenKyThi = "Lập trình ASP.NET Core nâng cao", MaChungChi = Ma("Kỹ năng lập trình .NET"), SoChoThiToiDa = 30, CapDoThi = "Nâng cao", DiemDauVaoToiThieu = 6, NgayBatDauDangKy = homNay.AddDays(7), HanDangKy = homNay.AddDays(28), MoTaKyThi = "Đánh giá MVC, EF Core và xử lý nghiệp vụ.", DieuKienDuThi = "Có kiến thức C# và cơ sở dữ liệu.", TrangThai = TrangThaiKyThi.ChuaMo },
                new KyThi { TenKyThi = "Lập trình .NET đợt tạm dừng", MaChungChi = Ma("Kỹ năng lập trình .NET"), SoChoThiToiDa = 25, CapDoThi = "Trung cấp", DiemDauVaoToiThieu = 4, NgayBatDauDangKy = homNay.AddDays(-4), HanDangKy = homNay.AddDays(9), MoTaKyThi = "Tạm dừng để cập nhật phòng thi.", DieuKienDuThi = "Có kiến thức C# cơ bản.", TrangThai = TrangThaiKyThi.TamDung },
                new KyThi { TenKyThi = "MOS văn phòng đợt lưu trữ", MaChungChi = Ma("Tin học văn phòng MOS (cũ)"), SoChoThiToiDa = 50, CapDoThi = "Cơ bản", DiemDauVaoToiThieu = 0, NgayBatDauDangKy = homNay.AddDays(-120), HanDangKy = homNay.AddDays(-100), MoTaKyThi = "Dữ liệu lịch sử thuộc chứng chỉ đã ngừng hoạt động.", DieuKienDuThi = "Hồ sơ hợp lệ.", TrangThai = TrangThaiKyThi.DaDong }
            };

            var daCo = await context.KyThis.Select(k => k.TenKyThi.ToLower()).ToListAsync();
            context.KyThis.AddRange(mau.Where(k => !daCo.Contains(k.TenKyThi.ToLower())));
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Dữ liệu tích hợp tối thiểu cho Module 3: 30 tài khoản/hồ sơ thí sinh và
        /// 45 hồ sơ đăng ký. Chỉ thêm các bản ghi còn thiếu, không sửa dữ liệu đã có.
        /// </summary>
        private static async Task NapThiSinhVaHoSoAsync(
            ApplicationDbContext context, IMatKhauService matKhauService)
        {
            var hoDem = new[]
            {
                "Nguyễn Văn", "Trần Thị", "Lê Minh", "Phạm Thu", "Hoàng Đức",
                "Vũ Ngọc", "Đỗ Quang", "Bùi Thanh", "Đặng Hải", "Phan Anh"
            };
            var ten = new[] { "An", "Bình", "Chi", "Dũng", "Hà", "Hùng", "Lan", "Minh", "Ngân", "Quân" };

            var tenDangNhapDaCo = await context.TaiKhoans
                .Select(t => t.TenDangNhap.ToLower()).ToListAsync();
            for (var i = 1; i <= 30; i++)
            {
                var tenDangNhap = $"thisinh{i:00}";
                if (tenDangNhapDaCo.Contains(tenDangNhap)) continue;
                var hoTen = $"{hoDem[(i - 1) % hoDem.Length]} {ten[(i * 3 - 1) % ten.Length]}";
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = tenDangNhap,
                    HoTen = hoTen,
                    Email = $"{tenDangNhap}@quanlykythi.local",
                    VaiTro = VaiTro.ThiSinh,
                    TrangThai = TrangThaiTaiKhoan.HoatDong
                };
                taiKhoan.MatKhau = matKhauService.BamMatKhau(taiKhoan, MatKhauThiSinh);
                context.TaiKhoans.Add(taiKhoan);
            }
            await context.SaveChangesAsync();

            var taiKhoanThiSinh = await context.TaiKhoans
                .Where(t => t.VaiTro == VaiTro.ThiSinh && t.TenDangNhap.StartsWith("thisinh"))
                .OrderBy(t => t.TenDangNhap)
                .ToListAsync();
            var maTaiKhoanDaCoHoSo = await context.ThiSinhs.Select(t => t.MaTaiKhoan).ToListAsync();
            var thuTu = 0;
            foreach (var taiKhoan in taiKhoanThiSinh.Where(t => t.TenDangNhap != "thisinh_khoa").Take(30))
            {
                if (maTaiKhoanDaCoHoSo.Contains(taiKhoan.MaTaiKhoan)) { thuTu++; continue; }
                var i = thuTu + 1;
                context.ThiSinhs.Add(new ThiSinh
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    HoTen = taiKhoan.HoTen,
                    NgaySinh = new DateTime(1997 + i % 8, i % 12 + 1, i % 25 + 1),
                    GioiTinh = i % 3 == 0 ? GioiTinh.Nu : GioiTinh.Nam,
                    SoDienThoai = $"09{(10000000 + i):00000000}",
                    Email = taiKhoan.Email,
                    DiaChi = $"Quận/Huyện mẫu {i % 8 + 1}, Hà Nội",
                    TrinhDoHienTai = i % 3 == 0 ? "Đại học" : "Cao đẳng",
                    NgheNghiep = i % 4 == 0 ? "Nhân viên văn phòng" : "Sinh viên",
                    SoLanThiTruocDay = i % 3,
                    ChungChiDaCo = i % 5 == 0 ? "Ứng dụng CNTT cơ bản" : null,
                    TrangThai = TrangThaiThiSinh.HoatDong
                });
                thuTu++;
            }
            await context.SaveChangesAsync();

            var thiSinhs = await context.ThiSinhs.OrderBy(t => t.MaThiSinh).Take(30).ToListAsync();
            var kyThis = await context.KyThis.OrderBy(k => k.MaKyThi).Take(15).ToListAsync();
            if (thiSinhs.Count == 0 || kyThis.Count == 0) return;

            var trangThais = Enumerable.Repeat(TrangThaiHoSo.ChoDuyet, 12)
                .Concat(Enumerable.Repeat(TrangThaiHoSo.DuDieuKien, 8))
                .Concat(Enumerable.Repeat(TrangThaiHoSo.KhongDuDieuKien, 5))
                .Concat(Enumerable.Repeat(TrangThaiHoSo.ChoThi, 6))
                .Concat(Enumerable.Repeat(TrangThaiHoSo.DaThi, 4))
                .Concat(Enumerable.Repeat(TrangThaiHoSo.Dat, 4))
                .Concat(Enumerable.Repeat(TrangThaiHoSo.KhongDat, 3))
                .Concat(Enumerable.Repeat(TrangThaiHoSo.DaHuy, 3))
                .ToArray();
            var canBo = await context.TaiKhoans
                .Where(t => t.VaiTro == VaiTro.CanBoKhaoThi)
                .OrderBy(t => t.MaTaiKhoan).Select(t => t.MaTaiKhoan).ToListAsync();
            var capDaCo = (await context.HoSoDangKyThis
                .Select(h => new { h.MaThiSinh, h.MaKyThi }).ToListAsync())
                .Select(x => (x.MaThiSinh, x.MaKyThi)).ToHashSet();
            var homNay = DateTime.Today;

            for (var i = 0; i < 45; i++)
            {
                var thiSinh = thiSinhs[i % thiSinhs.Count];
                var chiSoKyThi = (i * 7 + i / 30) % kyThis.Count;
                var kyThi = kyThis[chiSoKyThi];
                if (!capDaCo.Add((thiSinh.MaThiSinh, kyThi.MaKyThi))) continue;
                var trangThai = trangThais[i];
                var daXuLy = trangThai != TrangThaiHoSo.ChoDuyet && trangThai != TrangThaiHoSo.DaHuy;
                context.HoSoDangKyThis.Add(new HoSoDangKyThi
                {
                    MaThiSinh = thiSinh.MaThiSinh,
                    MaKyThi = kyThi.MaKyThi,
                    NgayNop = homNay.AddDays(-45 + i).AddHours(8 + i % 8),
                    MucDichDuThi = i % 2 == 0
                        ? "Bổ sung chứng chỉ phục vụ học tập và chuẩn đầu ra."
                        : "Đánh giá năng lực và hoàn thiện hồ sơ nghề nghiệp.",
                    GhiChu = i % 7 == 0 ? "Đề nghị kiểm tra kỹ thông tin liên hệ." : null,
                    TrangThai = trangThai,
                    NgayXuLy = daXuLy ? homNay.AddDays(-30 + i).AddHours(14) : null,
                    NhanXetXetDuyet = daXuLy
                        ? trangThai == TrangThaiHoSo.KhongDuDieuKien
                            ? "Hồ sơ còn thiếu minh chứng theo yêu cầu."
                            : "Hồ sơ đáp ứng yêu cầu kiểm tra ban đầu."
                        : null,
                    MaNguoiXuLy = daXuLy && canBo.Count > 0 ? canBo[i % canBo.Count] : null
                });
            }
            await context.SaveChangesAsync();
        }

        /// <summary>Dữ liệu nền để Module 5 và các màn hình tổng hợp có thể kiểm thử.</summary>
        private static async Task NapLichThiVaKetQuaAsync(ApplicationDbContext context)
        {
            var hoSoHopLe = await context.HoSoDangKyThis
                .Where(h => h.TrangThai == TrangThaiHoSo.DuDieuKien
                    || h.TrangThai == TrangThaiHoSo.ChoThi || h.TrangThai == TrangThaiHoSo.DaThi
                    || h.TrangThai == TrangThaiHoSo.Dat || h.TrangThai == TrangThaiHoSo.KhongDat)
                .OrderBy(h => h.MaHoSo).Take(18).ToListAsync();
            var maHoSoDaCoLich = await context.LichThis.Select(l => l.MaHoSo).ToListAsync();
            var mocThi = DateTime.Today.AddDays(14).AddHours(8);
            for (var i = 0; i < hoSoHopLe.Count; i++)
            {
                var hoSo = hoSoHopLe[i];
                if (maHoSoDaCoLich.Contains(hoSo.MaHoSo)) continue;
                var batDau = mocThi.AddDays(i / 6).AddMinutes((i % 6) * 90);
                context.LichThis.Add(new LichThi
                {
                    MaHoSo = hoSo.MaHoSo, ThoiGianBatDau = batDau,
                    ThoiGianKetThuc = batDau.AddMinutes(75),
                    HinhThucThi = i % 4 == 0 ? HinhThucThi.TrucTuyen : HinhThucThi.TrucTiep,
                    DiaDiemHoacLienKet = i % 4 == 0 ? "https://thi.example.local/phong-demo" : "Cơ sở Minh Khai",
                    PhongThi = i % 4 == 0 ? null : $"P.{201 + i % 6}",
                    GiamThi = $"Giám thị {i % 5 + 1}",
                    TrangThai = TrangThaiLichThi.DaLenLich
                });
            }
            await context.SaveChangesAsync();

            var hoSoCoKetQua = await context.HoSoDangKyThis
                .Where(h => h.TrangThai == TrangThaiHoSo.Dat || h.TrangThai == TrangThaiHoSo.KhongDat)
                .OrderBy(h => h.MaHoSo).ToListAsync();
            var maHoSoDaCoKetQua = await context.KetQuaThis.Select(k => k.MaHoSo).ToListAsync();
            foreach (var hoSo in hoSoCoKetQua.Where(h => !maHoSoDaCoKetQua.Contains(h.MaHoSo)))
            {
                var dat = hoSo.TrangThai == TrangThaiHoSo.Dat;
                context.KetQuaThis.Add(new KetQuaThi
                {
                    MaHoSo = hoSo.MaHoSo,
                    DiemDanhGia = dat ? 75 + hoSo.MaHoSo % 20 : 35 + hoSo.MaHoSo % 15,
                    NhanXet = dat ? "Đạt yêu cầu của kỳ thi." : "Chưa đạt ngưỡng yêu cầu.",
                    KetQua = dat ? KetQuaCuoiCung.Dat : KetQuaCuoiCung.KhongDat,
                    NgayCapNhat = DateTime.Today.AddDays(-hoSo.MaHoSo % 10).AddHours(16)
                });
            }
            await context.SaveChangesAsync();
        }
    }
}
