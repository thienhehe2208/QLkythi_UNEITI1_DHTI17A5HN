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
    }
}
