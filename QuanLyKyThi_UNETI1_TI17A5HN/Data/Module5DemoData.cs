// Họ và tên: Phạm Quang Minh
// Mã sinh viên: 23103100201
// Nội dung thực hiện: Dữ liệu demo riêng, chỉ dùng trong Development và database có hậu tố _Module5Demo.
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.Services;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Data;

public static class Module5DemoData
{
    public static async Task TaoAsync(ApplicationDbContext db, IMatKhauService passwords, DateTime now)
    {
        if (!db.Database.GetDbConnection().Database.EndsWith("_Module5Demo", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Dữ liệu demo chỉ được nạp vào database có tên kết thúc bằng _Module5Demo.");
        await db.Database.MigrateAsync();
        if (await db.TaiKhoans.AnyAsync()) return;
        await using var transaction = await db.Database.BeginTransactionAsync();
        TaiKhoan TaoTaiKhoan(string ten, VaiTro vaiTro)
        {
            var tk = new TaiKhoan { TenDangNhap = ten, HoTen = "Demo " + ten, Email = ten + "@example.com", VaiTro = vaiTro };
            tk.MatKhau = passwords.BamMatKhau(tk, "Module5@123");
            return tk;
        }
        db.TaiKhoans.AddRange(TaoTaiKhoan("m5_admin", VaiTro.Admin), TaoTaiKhoan("m5_canbo", VaiTro.CanBoKhaoThi));
        var cc = new ChungChi { TenChungChi = "Chứng chỉ demo Module 5", TrangThai = TrangThaiChungChi.DangHoatDong };
        var ky = new KyThi
        {
            TenKyThi = "Kỳ thi demo Module 5", ChungChi = cc, CapDoThi = "Cơ bản", SoChoThiToiDa = 30,
            NgayBatDauDangKy = now.AddDays(-30), HanDangKy = now.AddDays(30), TrangThai = TrangThaiKyThi.DangMoDangKy
        };
        for (int i = 1; i <= 16; i++)
        {
            var ts = new ThiSinh
            {
                TaiKhoan = TaoTaiKhoan($"m5_thisinh{i:00}", VaiTro.ThiSinh), HoTen = $"Thí sinh demo {i:00}",
                NgaySinh = new DateTime(2004, 1, 1), SoDienThoai = "0900000000", Email = $"m5_thisinh{i:00}@example.com"
            };
            var hoSo = new HoSoDangKyThi
            {
                ThiSinh = ts, KyThi = ky, NgayNop = now.AddDays(-10), NgayXuLy = now.AddDays(-9),
                MucDichDuThi = "Kiểm thử Module 5", NhanXetXetDuyet = "Hồ sơ demo đã được duyệt",
                TrangThai = i <= 4 ? TrangThaiHoSo.DuDieuKien : TrangThaiHoSo.ChoThi
            };
            db.HoSoDangKyThis.Add(hoSo);
            if (i <= 4) continue;
            var past = i >= 11;
            var start = past ? now.AddDays(-i) : now.AddDays(i).Date.AddHours(9);
            var lich = new LichThi
            {
                HoSoDangKyThi = hoSo, ThoiGianBatDau = start, ThoiGianKetThuc = start.AddHours(1),
                DiaDiemHoacLienKet = "Cơ sở demo", PhongThi = "DEMO-101", GiamThi = "Giám thị demo",
                TrangThai = i >= 13 ? TrangThaiLichThi.DaHoanThanh : TrangThaiLichThi.DaLenLich
            };
            db.LichThis.Add(lich);
            if (i >= 13) hoSo.TrangThai = TrangThaiHoSo.DaThi;
            if (i >= 15)
            {
                var dat = i == 15;
                hoSo.TrangThai = dat ? TrangThaiHoSo.Dat : TrangThaiHoSo.KhongDat;
                db.KetQuaThis.Add(new KetQuaThi
                {
                    HoSoDangKyThi = hoSo, DiemDanhGia = dat ? 85 : 35, KetQua = dat ? KetQuaCuoiCung.Dat : KetQuaCuoiCung.KhongDat,
                    NgayCapNhat = start.AddHours(2), NhanXet = "Kết quả mẫu"
                });
            }
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
