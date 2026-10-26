// Họ và tên: Phạm Quang Minh
// Mã sinh viên: 23103100201
// Nội dung thực hiện: Nền khởi động tối thiểu cho Module 5 trên main hiện tại.
// Khi tích hợp Program.cs của nhóm, giữ đăng ký TimeProvider và Module5Service.
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Controllers;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;
using QuanLyKyThi_UNETI1_TI17A5HN.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = AuthController.TenCookieSession;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddScoped<IMatKhauService, MatKhauService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<Module5Service>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Module5/Loi");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
// AuthController hiện có điều hướng về Home sau khi đăng nhập.
app.MapControllerRoute("home-module5", "Home/{action=Index}", new { controller = "Module5" });
app.MapControllerRoute("default", "{controller=Module5}/{action=Index}/{id?}");
app.Run();
