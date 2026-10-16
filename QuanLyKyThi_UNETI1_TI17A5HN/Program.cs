
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Họ và tên: Nguyễn Tiến Quang
// Module 2: Quản lý kỳ thi
// Nội dung: Đăng ký MVC và Entity Framework Core
// ============================================================

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=KyThi}/{action=Index}/{id?}");

app.Run();
