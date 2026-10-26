// ============================================================
// Há» vÃ  tÃªn: Nguyá»…n Äáº¯c Tiáº¿n
// MÃ£ sinh viÃªn: 23103100232
// Ná»™i dung thá»±c hiá»‡n: Ãp dá»¥ng migration vÃ  náº¡p dá»¯ liá»‡u máº«u Module 1
//   (tÃ i khoáº£n Admin / CÃ¡n bá»™ kháº£o thÃ­ / ThÃ­ sinh demo, 6 chá»©ng chá»‰).
//   Cháº¡y láº·p láº¡i an toÃ n: chá»‰ thÃªm báº£n ghi chÆ°a cÃ³ (so theo TenDangNhap / TenChungChi).
//   ThÃ nh viÃªn module khÃ¡c bá»• sung dá»¯ liá»‡u máº«u cá»§a mÃ¬nh vÃ o cÃ¡c hÃ m riÃªng bÃªn dÆ°á»›i.
// ============================================================

using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.Services;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Data
{
    public static class DbInitializer
    {
        // Máº­t kháº©u thá»­ nghiá»‡m (chá»‰ dÃ¹ng cho dá»¯ liá»‡u demo, Ä‘Æ°á»£c hash trÆ°á»›c khi lÆ°u DB).
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
                logger.LogWarning("ChÆ°a cÃ³ migration nÃ o. HÃ£y cháº¡y: dotnet ef migrations add InitialCreate");
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
                ("admin",     "Quáº£n trá»‹ viÃªn há»‡ thá»‘ng", "admin@quanlykythi.local",   VaiTro.Admin,        TrangThaiTaiKhoan.HoatDong, MatKhauAdmin),
                ("admin2",    "Quáº£n trá»‹ viÃªn dá»± phÃ²ng", "admin2@quanlykythi.local",  VaiTro.Admin,        TrangThaiTaiKhoan.HoatDong, MatKhauAdmin),
                ("canbo1",    "CÃ¡n bá»™ kháº£o thÃ­ 1",      "canbo1@quanlykythi.local",  VaiTro.CanBoKhaoThi, TrangThaiTaiKhoan.HoatDong, MatKhauCanBo),
                ("canbo2",    "CÃ¡n bá»™ kháº£o thÃ­ 2",      "canbo2@quanlykythi.local",  VaiTro.CanBoKhaoThi, TrangThaiTaiKhoan.HoatDong, MatKhauCanBo),
                ("canbo3",    "CÃ¡n bá»™ kháº£o thÃ­ 3",      "canbo3@quanlykythi.local",  VaiTro.CanBoKhaoThi, TrangThaiTaiKhoan.HoatDong, MatKhauCanBo),
                ("thisinh01", "ThÃ­ sinh demo 01",       "thisinh01@quanlykythi.local", VaiTro.ThiSinh,    TrangThaiTaiKhoan.HoatDong, MatKhauThiSinh),
                ("thisinh02", "ThÃ­ sinh demo 02",       "thisinh02@quanlykythi.local", VaiTro.ThiSinh,    TrangThaiTaiKhoan.HoatDong, MatKhauThiSinh),
                ("thisinh_khoa", "ThÃ­ sinh bá»‹ khÃ³a (demo)", "thisinh.khoa@quanlykythi.local", VaiTro.ThiSinh, TrangThaiTaiKhoan.BiKhoa, MatKhauThiSinh),
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
                new ChungChi { TenChungChi = "Chá»©ng chá»‰ á»¨ng dá»¥ng CNTT cÆ¡ báº£n",
                    MoTa = "Chuáº©n ká»¹ nÄƒng sá»­ dá»¥ng cÃ´ng nghá»‡ thÃ´ng tin cÆ¡ báº£n (ThÃ´ng tÆ° 03/2014/TT-BTTTT).",
                    EmailLienHe = "cntt@quanlykythi.local", TrangThai = TrangThaiChungChi.DangHoatDong },
                new ChungChi { TenChungChi = "Chá»©ng chá»‰ á»¨ng dá»¥ng CNTT nÃ¢ng cao",
                    MoTa = "Chuáº©n ká»¹ nÄƒng sá»­ dá»¥ng cÃ´ng nghá»‡ thÃ´ng tin nÃ¢ng cao.",
                    EmailLienHe = "cntt@quanlykythi.local", TrangThai = TrangThaiChungChi.DangHoatDong },
                new ChungChi { TenChungChi = "Tiáº¿ng Anh VSTEP (B1-C1)",
                    MoTa = "ÄÃ¡nh giÃ¡ nÄƒng lá»±c tiáº¿ng Anh theo Khung nÄƒng lá»±c ngoáº¡i ngá»¯ 6 báº­c dÃ¹ng cho Viá»‡t Nam.",
                    EmailLienHe = "ngoaingu@quanlykythi.local", TrangThai = TrangThaiChungChi.DangHoatDong },
                new ChungChi { TenChungChi = "TOEIC Listening & Reading",
                    MoTa = "BÃ i thi Ä‘Ã¡nh giÃ¡ ká»¹ nÄƒng nghe - Ä‘á»c tiáº¿ng Anh trong mÃ´i trÆ°á»ng lÃ m viá»‡c.",
                    EmailLienHe = "ngoaingu@quanlykythi.local", TrangThai = TrangThaiChungChi.DangHoatDong },
                new ChungChi { TenChungChi = "Ká»¹ nÄƒng láº­p trÃ¬nh .NET",
                    MoTa = "Chá»©ng chá»‰ ná»™i bá»™ Ä‘Ã¡nh giÃ¡ ká»¹ nÄƒng láº­p trÃ¬nh C#/ASP.NET Core.",
                    EmailLienHe = null, TrangThai = TrangThaiChungChi.DangHoatDong },
                new ChungChi { TenChungChi = "Tin há»c vÄƒn phÃ²ng MOS (cÅ©)",
                    MoTa = "Chá»©ng chá»‰ Ä‘Ã£ ngá»«ng tá»• chá»©c - dÃ¹ng Ä‘á»ƒ kiá»ƒm thá»­ tráº¡ng thÃ¡i Ngá»«ng hoáº¡t Ä‘á»™ng.",
                    EmailLienHe = "cntt@quanlykythi.local", TrangThai = TrangThaiChungChi.NgungHoatDong },
            };

            var daCo = await context.ChungChis.Select(c => c.TenChungChi.ToLower()).ToListAsync();
            context.ChungChis.AddRange(mau.Where(c => !daCo.Contains(c.TenChungChi.ToLower())));
            await context.SaveChangesAsync();
        }

        // Module 2 - Nguyá»…n Tiáº¿n Quang. MÃ£ sinh viÃªn cáº§n bá»• sung theo báº£ng phÃ¢n cÃ´ng.
        // Náº¡p 15 ká»³ thi Ä‘a dáº¡ng tráº¡ng thÃ¡i/thá»i háº¡n Ä‘á»ƒ kiá»ƒm thá»­ tÃ¬m kiáº¿m, lá»c vÃ  phÃ¢n trang.
        private static async Task NapKyThiAsync(ApplicationDbContext context)
        {
            var chungChi = await context.ChungChis
                .AsNoTracking()
                .ToDictionaryAsync(c => c.TenChungChi, c => c.MaChungChi);

            int Ma(string ten) => chungChi[ten];
            var homNay = DateTime.Today;
            var mau = new[]
            {
                new KyThi { TenKyThi = "á»¨ng dá»¥ng CNTT cÆ¡ báº£n Ä‘á»£t 1", MaChungChi = Ma("Chá»©ng chá»‰ á»¨ng dá»¥ng CNTT cÆ¡ báº£n"), SoChoThiToiDa = 60, CapDoThi = "CÆ¡ báº£n", DiemDauVaoToiThieu = 0, NgayBatDauDangKy = homNay.AddDays(-5), HanDangKy = homNay.AddDays(10).AddHours(23).AddMinutes(59), MoTaKyThi = "Ká»³ thi cáº¥p chá»©ng chá»‰ á»©ng dá»¥ng CNTT cÆ¡ báº£n.", DieuKienDuThi = "Há»“ sÆ¡ cÃ¡ nhÃ¢n Ä‘áº§y Ä‘á»§.", TrangThai = TrangThaiKyThi.DangMoDangKy },
                new KyThi { TenKyThi = "á»¨ng dá»¥ng CNTT cÆ¡ báº£n Ä‘á»£t 2", MaChungChi = Ma("Chá»©ng chá»‰ á»¨ng dá»¥ng CNTT cÆ¡ báº£n"), SoChoThiToiDa = 50, CapDoThi = "CÆ¡ báº£n", DiemDauVaoToiThieu = 0, NgayBatDauDangKy = homNay.AddDays(15), HanDangKy = homNay.AddDays(30), MoTaKyThi = "Äá»£t thi dá»± kiáº¿n tiáº¿p theo.", DieuKienDuThi = "Há»“ sÆ¡ cÃ¡ nhÃ¢n Ä‘áº§y Ä‘á»§.", TrangThai = TrangThaiKyThi.ChuaMo },
                new KyThi { TenKyThi = "á»¨ng dá»¥ng CNTT cÆ¡ báº£n Ä‘á»£t cÅ©", MaChungChi = Ma("Chá»©ng chá»‰ á»¨ng dá»¥ng CNTT cÆ¡ báº£n"), SoChoThiToiDa = 45, CapDoThi = "CÆ¡ báº£n", DiemDauVaoToiThieu = 0, NgayBatDauDangKy = homNay.AddDays(-60), HanDangKy = homNay.AddDays(-40), MoTaKyThi = "Dá»¯ liá»‡u kiá»ƒm thá»­ ká»³ thi Ä‘Ã£ Ä‘Ã³ng.", DieuKienDuThi = "Há»“ sÆ¡ cÃ¡ nhÃ¢n Ä‘áº§y Ä‘á»§.", TrangThai = TrangThaiKyThi.DaDong },
                new KyThi { TenKyThi = "á»¨ng dá»¥ng CNTT nÃ¢ng cao thÃ¡ng nÃ y", MaChungChi = Ma("Chá»©ng chá»‰ á»¨ng dá»¥ng CNTT nÃ¢ng cao"), SoChoThiToiDa = 40, CapDoThi = "NÃ¢ng cao", DiemDauVaoToiThieu = 5, NgayBatDauDangKy = homNay.AddDays(-3), HanDangKy = homNay.AddDays(12), MoTaKyThi = "ÄÃ¡nh giÃ¡ ká»¹ nÄƒng á»©ng dá»¥ng CNTT nÃ¢ng cao.", DieuKienDuThi = "CÃ³ kiáº¿n thá»©c CNTT cÆ¡ báº£n.", TrangThai = TrangThaiKyThi.DangMoDangKy },
                new KyThi { TenKyThi = "á»¨ng dá»¥ng CNTT nÃ¢ng cao táº¡m dá»«ng", MaChungChi = Ma("Chá»©ng chá»‰ á»¨ng dá»¥ng CNTT nÃ¢ng cao"), SoChoThiToiDa = 35, CapDoThi = "NÃ¢ng cao", DiemDauVaoToiThieu = 5, NgayBatDauDangKy = homNay.AddDays(-7), HanDangKy = homNay.AddDays(7), MoTaKyThi = "Táº¡m dá»«ng tiáº¿p nháº­n Ä‘á»ƒ Ä‘iá»u chá»‰nh káº¿ hoáº¡ch.", DieuKienDuThi = "CÃ³ kiáº¿n thá»©c CNTT cÆ¡ báº£n.", TrangThai = TrangThaiKyThi.TamDung },
                new KyThi { TenKyThi = "VSTEP B1 thÃ¡ng nÃ y", MaChungChi = Ma("Tiáº¿ng Anh VSTEP (B1-C1)"), SoChoThiToiDa = 100, CapDoThi = "B1", DiemDauVaoToiThieu = 3, NgayBatDauDangKy = homNay.AddDays(-10), HanDangKy = homNay.AddDays(5), MoTaKyThi = "Ká»³ thi Ä‘Ã¡nh giÃ¡ nÄƒng lá»±c tiáº¿ng Anh báº­c B1.", DieuKienDuThi = "HoÃ n thÃ nh há»“ sÆ¡ vÃ  lá»‡ phÃ­ theo quy Ä‘á»‹nh.", TrangThai = TrangThaiKyThi.DangMoDangKy },
                new KyThi { TenKyThi = "VSTEP B2 thÃ¡ng sau", MaChungChi = Ma("Tiáº¿ng Anh VSTEP (B1-C1)"), SoChoThiToiDa = 90, CapDoThi = "B2", DiemDauVaoToiThieu = 4, NgayBatDauDangKy = homNay.AddDays(10), HanDangKy = homNay.AddDays(25), MoTaKyThi = "Ká»³ thi Ä‘Ã¡nh giÃ¡ nÄƒng lá»±c tiáº¿ng Anh báº­c B2.", DieuKienDuThi = "HoÃ n thÃ nh há»“ sÆ¡ vÃ  lá»‡ phÃ­ theo quy Ä‘á»‹nh.", TrangThai = TrangThaiKyThi.ChuaMo },
                new KyThi { TenKyThi = "VSTEP C1 Ä‘á»£t trÆ°á»›c", MaChungChi = Ma("Tiáº¿ng Anh VSTEP (B1-C1)"), SoChoThiToiDa = 70, CapDoThi = "C1", DiemDauVaoToiThieu = 5, NgayBatDauDangKy = homNay.AddDays(-80), HanDangKy = homNay.AddDays(-55), MoTaKyThi = "Ká»³ thi Ä‘Ã£ káº¿t thÃºc nháº­n há»“ sÆ¡.", DieuKienDuThi = "ÄÃ¡p á»©ng Ä‘iá»u kiá»‡n há»“ sÆ¡ theo thÃ´ng bÃ¡o.", TrangThai = TrangThaiKyThi.DaDong },
                new KyThi { TenKyThi = "TOEIC 450 thÃ¡ng nÃ y", MaChungChi = Ma("TOEIC Listening & Reading"), SoChoThiToiDa = 120, CapDoThi = "450+", DiemDauVaoToiThieu = 0, NgayBatDauDangKy = homNay.AddDays(-2), HanDangKy = homNay.AddDays(14), MoTaKyThi = "BÃ i thi TOEIC Listening & Reading.", DieuKienDuThi = "CÃ³ giáº¥y tá» tÃ¹y thÃ¢n há»£p lá»‡.", TrangThai = TrangThaiKyThi.DangMoDangKy },
                new KyThi { TenKyThi = "TOEIC 650 Ä‘á»£t tiáº¿p theo", MaChungChi = Ma("TOEIC Listening & Reading"), SoChoThiToiDa = 100, CapDoThi = "650+", DiemDauVaoToiThieu = 3, NgayBatDauDangKy = homNay.AddDays(20), HanDangKy = homNay.AddDays(40), MoTaKyThi = "Äá»£t thi TOEIC dá»± kiáº¿n.", DieuKienDuThi = "CÃ³ giáº¥y tá» tÃ¹y thÃ¢n há»£p lá»‡.", TrangThai = TrangThaiKyThi.ChuaMo },
                new KyThi { TenKyThi = "TOEIC 750 Ä‘á»£t Ä‘Ã£ Ä‘Ã³ng", MaChungChi = Ma("TOEIC Listening & Reading"), SoChoThiToiDa = 80, CapDoThi = "750+", DiemDauVaoToiThieu = 4, NgayBatDauDangKy = homNay.AddDays(-50), HanDangKy = homNay.AddDays(-30), MoTaKyThi = "Dá»¯ liá»‡u kiá»ƒm thá»­ ká»³ thi háº¿t háº¡n.", DieuKienDuThi = "CÃ³ giáº¥y tá» tÃ¹y thÃ¢n há»£p lá»‡.", TrangThai = TrangThaiKyThi.DaDong },
                new KyThi { TenKyThi = "Láº­p trÃ¬nh .NET cÆ¡ báº£n", MaChungChi = Ma("Ká»¹ nÄƒng láº­p trÃ¬nh .NET"), SoChoThiToiDa = 40, CapDoThi = "CÆ¡ báº£n", DiemDauVaoToiThieu = 0, NgayBatDauDangKy = homNay.AddDays(-1), HanDangKy = homNay.AddDays(18), MoTaKyThi = "ÄÃ¡nh giÃ¡ kiáº¿n thá»©c C# vÃ  ASP.NET Core cÆ¡ báº£n.", DieuKienDuThi = "Biáº¿t láº­p trÃ¬nh C# cÆ¡ báº£n.", TrangThai = TrangThaiKyThi.DangMoDangKy },
                new KyThi { TenKyThi = "Láº­p trÃ¬nh ASP.NET Core nÃ¢ng cao", MaChungChi = Ma("Ká»¹ nÄƒng láº­p trÃ¬nh .NET"), SoChoThiToiDa = 30, CapDoThi = "NÃ¢ng cao", DiemDauVaoToiThieu = 6, NgayBatDauDangKy = homNay.AddDays(7), HanDangKy = homNay.AddDays(28), MoTaKyThi = "ÄÃ¡nh giÃ¡ MVC, EF Core vÃ  xá»­ lÃ½ nghiá»‡p vá»¥.", DieuKienDuThi = "CÃ³ kiáº¿n thá»©c C# vÃ  cÆ¡ sá»Ÿ dá»¯ liá»‡u.", TrangThai = TrangThaiKyThi.ChuaMo },
                new KyThi { TenKyThi = "Láº­p trÃ¬nh .NET Ä‘á»£t táº¡m dá»«ng", MaChungChi = Ma("Ká»¹ nÄƒng láº­p trÃ¬nh .NET"), SoChoThiToiDa = 25, CapDoThi = "Trung cáº¥p", DiemDauVaoToiThieu = 4, NgayBatDauDangKy = homNay.AddDays(-4), HanDangKy = homNay.AddDays(9), MoTaKyThi = "Táº¡m dá»«ng Ä‘á»ƒ cáº­p nháº­t phÃ²ng thi.", DieuKienDuThi = "CÃ³ kiáº¿n thá»©c C# cÆ¡ báº£n.", TrangThai = TrangThaiKyThi.TamDung },
                new KyThi { TenKyThi = "MOS vÄƒn phÃ²ng Ä‘á»£t lÆ°u trá»¯", MaChungChi = Ma("Tin há»c vÄƒn phÃ²ng MOS (cÅ©)"), SoChoThiToiDa = 50, CapDoThi = "CÆ¡ báº£n", DiemDauVaoToiThieu = 0, NgayBatDauDangKy = homNay.AddDays(-120), HanDangKy = homNay.AddDays(-100), MoTaKyThi = "Dá»¯ liá»‡u lá»‹ch sá»­ thuá»™c chá»©ng chá»‰ Ä‘Ã£ ngá»«ng hoáº¡t Ä‘á»™ng.", DieuKienDuThi = "Há»“ sÆ¡ há»£p lá»‡.", TrangThai = TrangThaiKyThi.DaDong }
            };

            var daCo = await context.KyThis.Select(k => k.TenKyThi.ToLower()).ToListAsync();
            context.KyThis.AddRange(mau.Where(k => !daCo.Contains(k.TenKyThi.ToLower())));
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Dá»¯ liá»‡u tÃ­ch há»£p tá»‘i thiá»ƒu cho Module 3: 30 tÃ i khoáº£n/há»“ sÆ¡ thÃ­ sinh vÃ 
        /// 45 há»“ sÆ¡ Ä‘Äƒng kÃ½. Chá»‰ thÃªm cÃ¡c báº£n ghi cÃ²n thiáº¿u, khÃ´ng sá»­a dá»¯ liá»‡u Ä‘Ã£ cÃ³.
        /// </summary>
        private static async Task NapThiSinhVaHoSoAsync(
            ApplicationDbContext context, IMatKhauService matKhauService)
        {
            var hoDem = new[]
            {
                "Nguyá»…n VÄƒn", "Tráº§n Thá»‹", "LÃª Minh", "Pháº¡m Thu", "HoÃ ng Äá»©c",
                "VÅ© Ngá»c", "Äá»— Quang", "BÃ¹i Thanh", "Äáº·ng Háº£i", "Phan Anh"
            };
            var ten = new[] { "An", "BÃ¬nh", "Chi", "DÅ©ng", "HÃ ", "HÃ¹ng", "Lan", "Minh", "NgÃ¢n", "QuÃ¢n" };

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
                    DiaChi = $"Quáº­n/Huyá»‡n máº«u {i % 8 + 1}, HÃ  Ná»™i",
                    TrinhDoHienTai = i % 3 == 0 ? "Äáº¡i há»c" : "Cao Ä‘áº³ng",
                    NgheNghiep = i % 4 == 0 ? "NhÃ¢n viÃªn vÄƒn phÃ²ng" : "Sinh viÃªn",
                    SoLanThiTruocDay = i % 3,
                    ChungChiDaCo = i % 5 == 0 ? "á»¨ng dá»¥ng CNTT cÆ¡ báº£n" : null,
                    TrangThai = TrangThaiThiSinh.HoatDong
                });
                thuTu++;
            }
            await context.SaveChangesAsync();

            var thiSinhs = await context.ThiSinhs.OrderBy(t => t.MaThiSinh).Take(30).ToListAsync();
            var kyThis = await context.KyThis.OrderBy(k => k.MaKyThi).Take(15).ToListAsync();
            if (thiSinhs.Count == 0 || kyThis.Count == 0) return;

            var trangThais = Enumerable.Repeat(TrangThaiHoSo.ChoDuyet, 12)
                .Concat(Enumerable.Repeat(TrangThaiHoSo.DuDieuKien, 7))
                .Concat(Enumerable.Repeat(TrangThaiHoSo.KhongDuDieuKien, 5))
                .Concat(Enumerable.Repeat(TrangThaiHoSo.ChoThi, 7))
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
                        ? "Bá»• sung chá»©ng chá»‰ phá»¥c vá»¥ há»c táº­p vÃ  chuáº©n Ä‘áº§u ra."
                        : "ÄÃ¡nh giÃ¡ nÄƒng lá»±c vÃ  hoÃ n thiá»‡n há»“ sÆ¡ nghá» nghiá»‡p.",
                    GhiChu = i % 7 == 0 ? "Äá» nghá»‹ kiá»ƒm tra ká»¹ thÃ´ng tin liÃªn há»‡." : null,
                    TrangThai = trangThai,
                    NgayXuLy = daXuLy ? homNay.AddDays(-30 + i).AddHours(14) : null,
                    NhanXetXetDuyet = daXuLy
                        ? trangThai == TrangThaiHoSo.KhongDuDieuKien
                            ? "Há»“ sÆ¡ cÃ²n thiáº¿u minh chá»©ng theo yÃªu cáº§u."
                            : "Há»“ sÆ¡ Ä‘Ã¡p á»©ng yÃªu cáº§u kiá»ƒm tra ban Ä‘áº§u."
                        : null,
                    MaNguoiXuLy = daXuLy && canBo.Count > 0 ? canBo[i % canBo.Count] : null
                });
            }
            await context.SaveChangesAsync();
        }

        /// <summary>Dá»¯ liá»‡u ná»n Ä‘á»ƒ Module 5 vÃ  cÃ¡c mÃ n hÃ¬nh tá»•ng há»£p cÃ³ thá»ƒ kiá»ƒm thá»­.</summary>
        private static async Task NapLichThiVaKetQuaAsync(ApplicationDbContext context)
        {
            var hoSoHopLe = await context.HoSoDangKyThis
                .Where(h => h.TrangThai == TrangThaiHoSo.ChoThi || h.TrangThai == TrangThaiHoSo.DaThi
                    || h.TrangThai == TrangThaiHoSo.Dat || h.TrangThai == TrangThaiHoSo.KhongDat)
                .OrderBy(h => h.MaHoSo).Take(18).ToListAsync();
            var maHoSoDaCoLich = await context.LichThis.Select(l => l.MaHoSo).ToListAsync();
            for (var i = 0; i < hoSoHopLe.Count; i++)
            {
                var hoSo = hoSoHopLe[i];
                if (maHoSoDaCoLich.Contains(hoSo.MaHoSo)) continue;
                var daThi = hoSo.TrangThai == TrangThaiHoSo.DaThi
                    || hoSo.TrangThai == TrangThaiHoSo.Dat
                    || hoSo.TrangThai == TrangThaiHoSo.KhongDat;
                var mocThi = daThi
                    ? DateTime.Today.AddDays(-20).AddHours(8)
                    : DateTime.Today.AddDays(14).AddHours(8);
                var batDau = mocThi.AddDays(i / 6).AddMinutes((i % 6) * 90);
                context.LichThis.Add(new LichThi
                {
                    MaHoSo = hoSo.MaHoSo, ThoiGianBatDau = batDau,
                    ThoiGianKetThuc = batDau.AddMinutes(75),
                    HinhThucThi = i % 4 == 0 ? HinhThucThi.TrucTuyen : HinhThucThi.TrucTiep,
                    DiaDiemHoacLienKet = i % 4 == 0 ? "https://thi.example.local/phong-demo" : "CÆ¡ sá»Ÿ Minh Khai",
                    PhongThi = i % 4 == 0 ? null : $"P.{201 + i % 6}",
                    GiamThi = $"GiÃ¡m thá»‹ {i % 5 + 1}",
                    TrangThai = daThi ? TrangThaiLichThi.DaHoanThanh : TrangThaiLichThi.DaLenLich
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
                    NhanXet = dat ? "Äáº¡t yÃªu cáº§u cá»§a ká»³ thi." : "ChÆ°a Ä‘áº¡t ngÆ°á»¡ng yÃªu cáº§u.",
                    KetQua = dat ? KetQuaCuoiCung.Dat : KetQuaCuoiCung.KhongDat,
                    NgayCapNhat = DateTime.Today.AddDays(-hoSo.MaHoSo % 10).AddHours(16)
                });
            }
            await context.SaveChangesAsync();
        }
    }
}
