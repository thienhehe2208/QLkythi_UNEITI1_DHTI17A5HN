// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Cấu hình ứng dụng - DbContext (SQL Server), Session,
//   dịch vụ mật khẩu, nạp dữ liệu mẫu khi khởi động.
// ============================================================

using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Controllers;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;
using QuanLyKyThi_UNETI1_TI17A5HN.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// EF Core 10 + SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session: lưu MaTaiKhoan, HoTen, VaiTro sau khi đăng nhập
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = AuthController.TenCookieSession;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddScoped<IMatKhauService, MatKhauService>();

// Module 5 - Phạm Quang Minh (23103100201): thời gian nghiệp vụ và xử lý lịch/kết quả thi.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<Module5Service>();

var app = builder.Build();

// Tự áp dụng migration + nạp dữ liệu mẫu (tắt bằng "KhoiTaoDuLieu:TuDongMigrate": false)
await DbInitializer.KhoiTaoAsync(app.Services,
    app.Configuration.GetValue("KhoiTaoDuLieu:TuDongMigrate", true));

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
