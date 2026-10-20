// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: DbContext dùng chung - khai báo DbSet, unique index,
//   quan hệ FK (không cascade delete), lưu enum dạng chuỗi.
// ============================================================

using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
        public DbSet<ChungChi> ChungChis => Set<ChungChi>();
        public DbSet<KyThi> KyThis => Set<KyThi>();
        public DbSet<ThiSinh> ThiSinhs => Set<ThiSinh>();
        public DbSet<HoSoDangKyThi> HoSoDangKyThis => Set<HoSoDangKyThi>();
        public DbSet<LichThi> LichThis => Set<LichThi>();
        public DbSet<KetQuaThi> KetQuaThis => Set<KetQuaThi>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- TaiKhoan ----------
            modelBuilder.Entity<TaiKhoan>(e =>
            {
                e.HasIndex(t => t.TenDangNhap).IsUnique();
                e.Property(t => t.VaiTro).HasConversion<string>().HasMaxLength(20);
                e.Property(t => t.TrangThai).HasConversion<string>().HasMaxLength(20);
            });

            // ---------- ChungChi ----------
            modelBuilder.Entity<ChungChi>(e =>
            {
                e.HasIndex(c => c.TenChungChi).IsUnique();
                e.Property(c => c.TrangThai).HasConversion<string>().HasMaxLength(20);
            });

            // ---------- KyThi: ChungChi (1) - (n) KyThi ----------
            modelBuilder.Entity<KyThi>(e =>
            {
                e.Property(k => k.TrangThai).HasConversion<string>().HasMaxLength(20);
                e.HasOne(k => k.ChungChi)
                    .WithMany(c => c.KyThis)
                    .HasForeignKey(k => k.MaChungChi)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- ThiSinh: TaiKhoan (1) - (0..1) ThiSinh ----------
            modelBuilder.Entity<ThiSinh>(e =>
            {
                e.HasIndex(t => t.MaTaiKhoan).IsUnique();
                e.Property(t => t.GioiTinh).HasConversion<string>().HasMaxLength(10);
                e.Property(t => t.TrangThai).HasConversion<string>().HasMaxLength(20);
                e.HasOne(t => t.TaiKhoan)
                    .WithOne(tk => tk.ThiSinh)
                    .HasForeignKey<ThiSinh>(t => t.MaTaiKhoan)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- HoSoDangKyThi ----------
            modelBuilder.Entity<HoSoDangKyThi>(e =>
            {
                e.Property(h => h.TrangThai).HasConversion<string>().HasMaxLength(20);
                e.HasIndex(h => new { h.MaThiSinh, h.MaKyThi });

                e.HasOne(h => h.ThiSinh)
                    .WithMany(t => t.HoSoDangKyThis)
                    .HasForeignKey(h => h.MaThiSinh)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(h => h.KyThi)
                    .WithMany(k => k.HoSoDangKyThis)
                    .HasForeignKey(h => h.MaKyThi)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(h => h.NguoiXuLy)
                    .WithMany()
                    .HasForeignKey(h => h.MaNguoiXuLy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- LichThi: HoSoDangKyThi (1) - (0..n) LichThi ----------
            modelBuilder.Entity<LichThi>(e =>
            {
                e.Property(l => l.HinhThucThi).HasConversion<string>().HasMaxLength(20);
                e.Property(l => l.TrangThai).HasConversion<string>().HasMaxLength(20);
                e.HasIndex(l => new { l.ThoiGianBatDau, l.ThoiGianKetThuc });

                e.HasOne(l => l.HoSoDangKyThi)
                    .WithMany(h => h.LichThis)
                    .HasForeignKey(l => l.MaHoSo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- KetQuaThi: HoSoDangKyThi (1) - (0..1) KetQuaThi ----------
            modelBuilder.Entity<KetQuaThi>(e =>
            {
                e.HasIndex(k => k.MaHoSo).IsUnique();
                e.Property(k => k.KetQua).HasConversion<string>().HasMaxLength(20);

                e.HasOne(k => k.HoSoDangKyThi)
                    .WithOne(h => h.KetQuaThi)
                    .HasForeignKey<KetQuaThi>(k => k.MaHoSo)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
