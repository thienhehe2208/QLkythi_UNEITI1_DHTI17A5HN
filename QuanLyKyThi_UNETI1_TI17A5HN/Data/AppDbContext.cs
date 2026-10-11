// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Quản lý chứng chỉ
// ============================================================

using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<ChungChi> ChungChis { get; set; }
        public DbSet<ThiSinh> ThiSinhs { get; set; }
        public DbSet<KyThi> KyThis { get; set; }
        // ... các DbSet khác do thành viên khác khai báo

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ràng buộc UNIQUE cho TenDangNhap
            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap)
                .IsUnique();

            // Ràng buộc UNIQUE cho TenChungChi
            modelBuilder.Entity<ChungChi>()
                .HasIndex(c => c.TenChungChi)
                .IsUnique();

            // Quan hệ 1-1: TaiKhoan - ThiSinh
            modelBuilder.Entity<ThiSinh>()
                .HasOne(s => s.TaiKhoan)
                .WithOne(t => t.ThiSinh)
                .HasForeignKey<ThiSinh>(s => s.MaTaiKhoan)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ 1-n: ChungChi - KyThi
            modelBuilder.Entity<KyThi>()
                .HasOne(k => k.ChungChi)
                .WithMany(c => c.DanhSachKyThi)
                .HasForeignKey(k => k.MaChungChi)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
