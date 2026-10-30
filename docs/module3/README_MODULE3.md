# Module 3 quản lý thí sinh và đăng ký dự thi

Module 3 được triển khai trên đúng ZIP người dùng cung cấp, theo file TXT và mục 7 của Word. Gói này giữ các chức năng có sẵn của main và bổ sung phần Module 3. Không xây thay chức năng quản lý kỳ thi, xét duyệt, lịch/kết quả, Dashboard hoặc thống kê.

## Nền mã nguồn

- ZIP nền: `QLkythi_UNEITI1_DHTI17A5HN-main(1).zip`.
- SHA-256: `c1bee285b7cc60f743b87cacee2b991e8d97aa1fefdb803c361e0551878246d7`.
- Project/namespace giữ nguyên: `QuanLyKyThi_UNETI1_TI17A5HN`.
- ZIP nền không có metadata Git hoặc URL repository. Vì vậy chưa xác minh HEAD trên GitHub, chưa push hoặc merge repository của nhóm.
- Có một branch cục bộ `feature/module-3` để rà diff trên snapshot ZIP; đó không phải branch đã được đẩy lên GitHub. Gói không mang lịch sử Git giả làm lịch sử commit của sinh viên.

## Chức năng đã triển khai

1. Xem hồ sơ cá nhân, khai báo lần đầu và cập nhật đúng chủ sở hữu. GET không tạo dữ liệu.
2. Validation họ tên, ngày sinh, giới tính, điện thoại, Email, số lần thi và độ dài theo model nền.
3. Chọn kỳ thi từ Database; nhận `maKyThi` chọn trước từ link Module 2.
4. Kiểm tra tài khoản, hồ sơ cá nhân, trạng thái thí sinh, kỳ thi, thời hạn và trùng đang xử lý trong POST.
5. Tạo đăng ký Chờ duyệt với ngày nộp hệ thống, dữ liệu xét duyệt null.
6. Danh sách hồ sơ của tôi có tìm kiếm, lọc và phân trang trên EF Core/LINQ.
7. Chi tiết hiển thị kỳ thi/chứng chỉ, xét duyệt, mọi lịch lịch sử và kết quả hợp lệ.
8. GET xác nhận hủy, POST có token và kiểm tra lại quyền, trạng thái, lịch/kết quả; giữ lịch sử và dữ liệu xét duyệt.
9. Admin/Cán bộ tra cứu thí sinh và các hồ sơ liên quan; chỉ Admin đổi trạng thái thí sinh.
10. Tái sử dụng PhanQuyen, Session, layout, thông báo, badge và phân trang của main.

## Chạy bằng Visual Studio

1. Giải nén gói vào một thư mục mới, không chép đè repository đang có thay đổi.
2. Mở `QuanLyKyThi_UNETI1_TI17A5HN.slnx` ở thư mục gốc của gói. Thư mục bên trong chứa csproj là ứng dụng thật, không phải bản sao project.
3. Dùng môi trường hỗ trợ .NET 10 và SQL Server. Chọn project ứng dụng làm Startup Project.
4. Cấu hình `ConnectionStrings__DefaultConnection` cho SQL Server cá nhân hoặc Database kiểm thử. Không commit tên máy/mật khẩu cá nhân.
5. Restore và Build. Project giữ nguyên EF Core 10.0.12 và SQL Server provider.
6. Chạy profile `http` hoặc `https`. Cấu hình nền là http://localhost:5075 và https://localhost:7044; đọc địa chỉ thật runtime in ra.
7. Main tự áp dụng Migration và thêm dữ liệu mẫu khi khởi động. Không cần xóa Database hoặc tạo lại InitialCreate.

## Chạy bằng PowerShell từ thư mục có Solution

```powershell
dotnet --info
dotnet tool restore
$env:ConnectionStrings__DefaultConnection='Server=TEN_SQL_SERVER_CUA_BAN;Database=QuanLyKyThi_Module3_Local;Trusted_Connection=True;TrustServerCertificate=True'
dotnet restore .\QuanLyKyThi_UNETI1_TI17A5HN\QuanLyKyThi_UNETI1_TI17A5HN.csproj
dotnet build .\QuanLyKyThi_UNETI1_TI17A5HN\QuanLyKyThi_UNETI1_TI17A5HN.csproj --no-restore
dotnet ef migrations has-pending-model-changes --project .\QuanLyKyThi_UNETI1_TI17A5HN
dotnet run --project .\QuanLyKyThi_UNETI1_TI17A5HN\QuanLyKyThi_UNETI1_TI17A5HN.csproj --launch-profile http
```

Thay server bằng tên SQL Server thật. Nếu dùng SQL Authentication, cấu hình User Id/Password riêng trong môi trường. Không thay provider, không dùng EnsureCreated và không tạo Migration chỉ vì thêm Controller/View.

## Tài khoản mẫu của main

| Vai trò | Tên đăng nhập | Mật khẩu seed |
|---|---|---|
| Admin | admin, admin2 | Admin@123 |
| Cán bộ | canbo1, canbo2, canbo3 | CanBo@123 |
| Thí sinh | thisinh01 đến thisinh30 | ThiSinh@123 |

Database đã sử dụng có thể có mật khẩu khác. Dữ liệu mẫu gốc có lỗi dấu tiếng Việt; gói Module 3 giữ nguyên seed để không sửa ngoài phạm vi. Dữ liệu mới nhập qua Module 3 dùng Unicode bình thường.

## URL kiểm tra nhanh

| Vai trò | URL |
|---|---|
| Thí sinh | /ThiSinh/HoSoCaNhan |
| Thí sinh | /ThiSinh/CapNhatHoSo |
| Thí sinh | /HoSoThiSinh |
| Thí sinh | /HoSoThiSinh/Create |
| Thí sinh | /HoSoThiSinh/Create?maKyThi=MA_KY_THI_THAT |
| Thí sinh | /HoSoThiSinh/Details/MA_HO_SO |
| Thí sinh | /HoSoThiSinh/Huy/MA_HO_SO |
| Admin/Cán bộ | /ThiSinh |
| Admin/Cán bộ | /ThiSinh/Details/MA_THI_SINH |

ID phải lấy từ Database/giao diện, không mặc định các ID giữa hồ sơ/lịch/kết quả bằng nhau.

## Thông tin người thực hiện

Người dùng chưa cung cấp họ tên và mã sinh viên của người phụ trách Module 3. Đầu các file mới và hai đoạn đóng góp dùng chung ghi `CHƯA CUNG CẤP`. Hãy điền thông tin thật của người thực hiện trước khi nộp; giữ nguyên tên tác giả của phần khung có sẵn. Không dùng tên người khác hoặc tạo lịch sử commit giả.

## Tài liệu bàn giao

- `HOP_DONG_TICH_HOP_MODULE3.md`: quy tắc dữ liệu, quyền và liên kết.
- `HUONG_DAN_MERGE_MODULE3.md`: áp dụng patch vào branch thật của nhóm.
- `KIEM_THU_MODULE3.md`: môi trường, lệnh và phạm vi đã kiểm tra.
- `DANH_SACH_FILE_MODULE3.txt`: file mới, file sửa và hash bảo toàn nền.
- `evidence/`: log build, Migration và kiểm thử đã chạy.
- `MODULE3_ONLY.patch` ở gốc gói: patch có thể áp dụng vào bản nền tương thích.

Không cam kết mọi branch tương lai sẽ merge tự động. Cần áp dụng trên main thật, rà hai file dùng chung rồi chạy kiểm thử sau tích hợp.
