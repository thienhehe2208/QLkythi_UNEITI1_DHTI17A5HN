
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Models;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Data
{
    // ============================================================
    // Họ và tên: Nguyễn Tiến Quang
    // Module 2: Quản lý kỳ thi
    // Nội dung: Cấu hình DbContext
    // ============================================================

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Bảng quản lý kỳ thi
        public DbSet<KyThi> KyThis { get; set; } = null!;

        // Bảng quản lý chứng chỉ
        public DbSet<ChungChi> ChungChis { get; set; } = null!;
    }
}
