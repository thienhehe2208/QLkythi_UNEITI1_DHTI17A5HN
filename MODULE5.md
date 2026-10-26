# Module 5 Lịch thi và kết quả thi

Người phụ trách: **Phạm Quang Minh — 23103100201**.

Phạm vi đợt này: nền chạy tối thiểu dựa trên main, lịch thi và kết quả thi. Chưa triển khai Dashboard và thống kê.

## Chạy demo trên máy cá nhân

Mở PowerShell tại thư mục repository rồi chạy:

```powershell
./scripts/Run-Module5Demo.ps1
```

Ứng dụng chạy tại http://127.0.0.1:5075. Script dùng SQL Server Express ` .\SQLEXPRESS` và database riêng `QuanLyKyThi_UNETI1_TI17A5HN_Module5Demo`; không đổi connection string trong file chung. Có thể đổi SQL instance bằng `-SqlServer 'TEN_MAY\INSTANCE'` và cổng bằng `-Port 5076`. Nhấn Ctrl+C để dừng; script khôi phục biến môi trường của phiên PowerShell.

Dữ liệu mẫu chỉ được nạp khi môi trường là Development, bật `Module5:SeedDemo` và tên database kết thúc bằng `_Module5Demo`. Seeder áp dụng migration sẵn có, chỉ nạp khi bảng tài khoản còn trống, không xóa hoặc đặt lại dữ liệu khi khởi động lại.

| Vai trò | Tài khoản demo | Mật khẩu demo |
|---|---|---|
| Admin | m5_admin | Module5@123 |
| Cán bộ khảo thí | m5_canbo | Module5@123 |
| Thí sinh | m5_thisinh01 đến m5_thisinh16 | Module5@123 |

Dữ liệu ban đầu có 16 hồ sơ: 01–04 đủ điều kiện để lập lịch; 05–10 có lịch tương lai; 11–12 có lịch đã qua giờ kết thúc nhưng chưa xác nhận hoàn thành; 13–14 đã thi, chưa có kết quả; 15–16 có kết quả Đạt/Không đạt. Số thứ tự này là số trong tên tài khoản thí sinh, không phải giả định mã khóa chính ở database của nhóm. Mỗi lần kiểm thử HTTP sẽ thêm một lịch hủy và nhập/sửa một kết quả trong database demo.

## Chạy trên database của nhóm

Tắt cờ `Module5:SeedDemo` hoặc không đặt cờ này. Dùng connection string SQL Server của nhóm và migration hiện có:

```powershell
dotnet tool restore
dotnet ef database update --project QuanLyKyThi_UNETI1_TI17A5HN
dotnet run --project QuanLyKyThi_UNETI1_TI17A5HN
```

Nền tối thiểu không tự nạp tài khoản vào database nhóm. Cần tài khoản và hồ sơ được Module 1/3/4 tạo sẵn. Không thay Entity, DbContext, schema hay tạo migration mới trong đợt này.

## Các quy tắc nghiệp vụ

- Admin và cán bộ khảo thí được tạo/sửa/hủy/hoàn thành lịch và nhập/sửa kết quả. Thí sinh chỉ xem lịch, kết quả của chính mình.
- Lập lịch mới chỉ cho hồ sơ Đủ điều kiện, thí sinh hoạt động, chưa có kết quả và chưa có lịch không hủy. Tạo lịch chuyển hồ sơ sang Chờ thi.
- Sửa lịch chỉ khi lịch chưa bắt đầu, trạng thái Đã lên lịch và hồ sơ Chờ thi; không được đổi hồ sơ của lịch.
- Bắt đầu phải ở tương lai, kết thúc phải sau bắt đầu. Các lịch không hủy được kiểm tra bằng EF Core/LINQ với công thức `batDauCu < ketThucMoi && batDauMoi < ketThucCu`. Hai lịch nối tiếp đúng giờ được phép.
- Chặn trùng thí sinh, giám thị, phòng. Phòng là mã dùng chung trong hệ thống (ví dụ A-301); các thành viên phải dùng cùng quy ước đặt tên. Giám thị dùng tên chuẩn thống nhất. Không phân biệt hoa/thường; đầu vào mới được loại khoảng trắng thừa.
- Thi trực tiếp phải có phòng; thi trực tuyến phải có liên kết HTTP/HTTPS và không dùng trường phòng.
- Chỉ hủy lịch chưa bắt đầu. Lịch hủy được giữ lại để bảo toàn lịch sử và hồ sơ trở về Đủ điều kiện để lập lại lịch.
- Chỉ hoàn thành lịch sau giờ kết thúc; lịch phải đang hiệu lực và hồ sơ Chờ thi. Hồ sơ chuyển sang Đã thi.
- Chỉ nhập kết quả mới cho hồ sơ Đã thi có lịch đã hoàn thành. Điểm tùy chọn từ 0 đến 100; cán bộ phải chọn Đạt/Không đạt, không suy ra tự động từ điểm.
- Mỗi hồ sơ có một kết quả. Sửa kết quả chỉ khi trạng thái hồ sơ còn khớp với kết quả cũ và có lịch hoàn thành. Sau khi lưu, trạng thái hồ sơ và kết quả cập nhật cùng nhau.
- Kết quả được coi là công bố ngay khi cán bộ lưu; chưa có cờ công bố riêng trong Entity chung.
- Thời gian nghiệp vụ và Ngày cập nhật dùng UTC+7. Các thao tác ghi dùng transaction Serializable để bảo vệ bước kiểm tra và ghi; nếu có xung đột database, giao diện yêu cầu tải lại và thử lại.
- POST có antiforgery; quyền được kiểm tra ở server; mã trong URL/form không cho phép đổi chủ sở hữu dữ liệu.

## File chính

| File/nhóm file | Vai trò |
|---|---|
| Services/Module5Service.cs | Kiểm tra nghiệp vụ và cập nhật nguyên tử lịch/hồ sơ/kết quả |
| Controllers/LichThiController.cs | Danh sách, chi tiết, form và action lịch thi |
| Controllers/KetQuaThiController.cs | Danh sách, chi tiết, form và action kết quả |
| ViewModels/LichThiViewModels.cs và KetQuaThiViewModels.cs | Chỉ nhận các trường cho phép sửa; validation |
| Views/LichThi và Views/KetQuaThi | Giao diện tìm kiếm, lọc, phân trang, tạo/sửa và chi tiết |
| Data/Module5DemoData.cs | Dữ liệu demo độc lập, không thay chức năng duyệt hồ sơ Module 4 |
| tests/Module5.Checks | Kiểm thử SQL Server và HTTP |

## Kiểm thử

```powershell
dotnet build QuanLyKyThi_UNETI1_TI17A5HN.slnx
dotnet run --project tests/Module5.Checks
```

Bộ kiểm thử tích hợp tạo database SQL Server riêng tên `QuanLyKyThi_Module5Checks_<GUID>`, áp dụng migration thật và tự xóa đúng database vừa tạo sau khi chạy. Có thể đặt biến môi trường `MODULE5_TEST_SQL` để đổi máy chủ; tên database luôn được chương trình thay bằng tên ngẫu nhiên riêng.

Khi ứng dụng demo đang chạy, chạy thêm trong PowerShell 7:

```powershell
./tests/Module5.Checks/HttpSmoke.ps1
```

HTTP smoke dùng tài khoản demo và có tạo/sửa/hủy lịch, nhập/sửa kết quả. Cần ít nhất một hồ sơ đủ điều kiện chưa có lịch và một hồ sơ đã thi chưa có kết quả. Mỗi lần chạy sử dụng một hồ sơ chưa có kết quả, nên không chạy lặp vô hạn trên cùng dữ liệu mẫu.

Kết quả kiểm tra ngày 26/10/2026: build thành công; 32 kiểm tra nghiệp vụ/quyền dữ liệu trên SQL Server và 24 kiểm tra HTTP đã qua. Đã mở trình duyệt kiểm tra đăng nhập, danh sách lịch và form lịch. Đã nâng phụ thuộc System.Security.Cryptography.Pkcs lên 9.0.21 để xử lý NU1902/NU1903, giữ EF Core 10.0.12. Bản vá theo https://github.com/advisories/GHSA-846f-f764-76mx và https://github.com/advisories/GHSA-2fmv-rv2c-59x7.

## Khi main được nhóm cập nhật

Không lấy code trực tiếp từ nhánh Module 1/2/3/4. Chỉ cập nhật nền từ main theo quy ước nhóm. Commit và push Module 5 được thực hiện khi người phụ trách yêu cầu; không tự merge nhánh của thành viên khác.

Main tại thời điểm bắt đầu thiếu Program.cs, HomeController, _ViewImports, _Layout và tài nguyên giao diện. Đợt này bổ sung nền nhỏ để Module 5 chạy được, không sao chép code từ nhánh khác. Khi main có nền đầy đủ, cần đối chiếu các file chung:

1. `Program.cs`: ưu tiên nền của nhóm nhưng giữ `AddSingleton(TimeProvider.System)` và `AddScoped<Module5Service>()`. Bỏ route tạm `home-module5` khi nhóm có HomeController để route này không che trang chủ thật. Seeder demo là tùy chọn Development.
2. `Views/_ViewImports.cshtml`: giữ các namespace Models, Enums, ViewModels, Services và MVC Tag Helper.
3. `Views/Shared/_Layout.cshtml`: dùng layout của nhóm và thêm menu LichThi/KetQuaThi. Giữ hiển thị TempData thông báo.
4. `Views/Shared/_ThongBao.cshtml`, `wwwroot/css/site.css`, `wwwroot/js/site.js`: đối chiếu với tài nguyên chung mới. Các file này hiện phục vụ layout đăng nhập cũ trên main.
5. `Views/Shared/_LayoutAuth.cshtml`, `Views/Auth/DangNhap.cshtml`: chỉnh nhỏ để bỏ tài nguyên icon đang thiếu và dùng chữ trên nút hiện mật khẩu.

Controller, service và view Module 5 dùng các Entity/enum hiện có. Trang hồ sơ của Module 3/4 có thể liên kết tới `LichThi/Create?maHoSo=...` khi hồ sơ đủ điều kiện, và `KetQuaThi/Create?maHoSo=...` khi đã thi. Quyền/trạng thái vẫn luôn được server kiểm tra lại.

## Đồng bộ phong cách nhóm

Tham khảo mã nguồn Module 1 và bản tích hợp Module 4 đang có tại máy, không merge các nhánh này. Giao diện dùng cùng bảng màu, menu ngang, page-header, card, filter-bar, table-app, badge-status, breadcrumb, Bootstrap toast và data-confirm. Không tham chiếu bộ icon chưa có trên main; nút thao tác có nhãn chữ rõ ràng.

Backend dùng namespace dạng khối, constructor thông thường, trường _context, ViewModel riêng cho danh sách/form/chi tiết và LINQ Select để chỉ lấy dữ liệu hiển thị. Danh sách dùng PhanTrangViewModel và partial _PhanTrang theo giao diện của nhóm, giữ tham số lọc khi chuyển trang. Service riêng giữ transaction và kiểm tra nghiệp vụ lịch/kết quả để kiểm thử độc lập.

Khi main bổ sung thành phần dùng chung, đối chiếu thêm PhanTrangViewModel, _PhanTrang và _BadgeTrangThai; ưu tiên bản chung của nhóm và giữ các trạng thái Module 5. Không cần thêm Entity hoặc migration cho thay đổi giao diện này.
