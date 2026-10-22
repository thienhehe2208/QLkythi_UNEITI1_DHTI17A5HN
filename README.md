# QuanLyKyThi_UNETI1_TI17A5HN — Đề tài 28

Hệ thống quản lý kỳ thi chứng chỉ và hồ sơ đăng ký dự thi — ASP.NET Core 10 MVC, EF Core 10.0.12 (Code First), SQL Server.

## Yêu cầu môi trường

- .NET 10 SDK, Visual Studio 2026 (hoặc VS Code + C# Dev Kit)
- SQL Server / SQL Server Express (mặc định `.\SQLEXPRESS`)
- Công cụ EF: `dotnet tool restore` (đã khai báo `dotnet-ef` 10.0.12 trong `.config/dotnet-tools.json`)

## Chạy lần đầu

1. Sửa chuỗi kết nối `ConnectionStrings:DefaultConnection` trong `QuanLyKyThi_UNETI1_TI17A5HN/appsettings.json` nếu máy không dùng `.\SQLEXPRESS`
   (hoặc ghi đè trong `appsettings.Development.json` trên máy cá nhân, không commit mật khẩu thật).
2. Tạo/cập nhật database:
   ```powershell
   dotnet tool restore
   dotnet ef database update --project QuanLyKyThi_UNETI1_TI17A5HN
   ```
   (Visual Studio: Package Manager Console → `Update-Database`.)
3. Chạy project. Khi khởi động, `DbInitializer` tự áp dụng migration còn thiếu và nạp dữ liệu mẫu
   (chạy lặp lại không tạo trùng). Tắt bằng `"KhoiTaoDuLieu": { "TuDongMigrate": false }`.

## Tài khoản kiểm thử (dữ liệu demo, mật khẩu được hash trong DB)

| Vai trò | Tên đăng nhập | Mật khẩu |
|---|---|---|
| Admin | `admin`, `admin2` | `Admin@123` |
| Cán bộ khảo thí | `canbo1`, `canbo2`, `canbo3` | `CanBo@123` |
| Thí sinh | `thisinh01`, `thisinh02` | `ThiSinh@123` |
| Thí sinh **bị khóa** | `thisinh_khoa` | `ThiSinh@123` |

## Cấu trúc

```text
QuanLyKyThi_UNETI1_TI17A5HN/
├── Controllers/   Home, Auth, TaiKhoan, ChungChi (+ controller của các module khác)
├── Data/          ApplicationDbContext, DbInitializer
├── Filters/       PhanQuyenAttribute (kiểm tra Session + vai trò phía server)
├── Models/        7 Entity theo QUY_UOC_NHOM + Enums (giá trị không dấu, nhãn tiếng Việt)
├── Services/      MatKhauService (hash mật khẩu), PhienDangNhap (khóa Session)
├── ViewModels/    ViewModel từng màn hình + PhanTrangViewModel dùng chung
├── Views/         Razor View; Shared/_PhanTrang dùng chung cho mọi danh sách
└── Migrations/
```

## Hướng dẫn dùng chung cho các module

- **Phân quyền**: gắn `[PhanQuyen]` (chỉ cần đăng nhập) hoặc `[PhanQuyen(VaiTro.Admin, VaiTro.CanBoKhaoThi)]` lên Controller/Action.
  Không chỉ ẩn nút trên View.
- **Lấy người đăng nhập**: `HttpContext.Session.LayMaTaiKhoan()`, `LayVaiTro()`, `LayHoTen()` (namespace `Services`).
  Thí sinh: luôn lấy `MaThiSinh` từ `MaTaiKhoan` trong Session, không tin mã gửi từ URL/form.
- **Phân trang**: tạo `PhanTrangViewModel { TrangHienTai, TongSoBanGhi, ThamSo = {...bộ lọc...} }`, dùng
  `PhanTrangViewModel.ChuanHoaTrang(...)` và `<partial name="_PhanTrang" model="..." />` (10 bản ghi/trang).
- **Nhãn tiếng Việt của enum**: `trangThai.HienThi()`; select: `Html.GetEnumSelectList<TrangThaiKyThi>()`.
- **Thông báo**: đặt `TempData["ThongBao"]` / `TempData["LoiThongBao"]`, layout tự hiển thị.
- Thay đổi Entity dùng chung → thống nhất nhóm trước, rồi tạo migration mới có tên rõ nghĩa (không sửa migration cũ).

## Phân công

| Module | Nội dung | Thành viên |
|---|---|---|
| 1 | Tài khoản, đăng nhập, Session, phân quyền, quản lý chứng chỉ | Nguyễn Đắc Tiến — 23103100232 |
| 2 | Quản lý kỳ thi, tìm kiếm, lọc, sắp xếp, phân trang | |
| 3 | Thí sinh, hồ sơ cá nhân, nộp/hủy/theo dõi hồ sơ | |
| 4 | Tiếp nhận, kiểm tra điều kiện, trạng thái hồ sơ, chỉ tiêu | |
| 5 | Lịch thi, kết quả, dashboard, thống kê | |

Mẫu commit: `[MaSV] [Module] Noi dung cong viec`.
