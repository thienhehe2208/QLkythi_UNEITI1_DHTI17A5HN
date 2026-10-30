# Kết quả kiểm thử Module 3

Ngày kiểm tra: 30/10/2026. Các kết quả dưới đây đã được thực thi, không chỉ là checklist dự kiến.

## Môi trường

- Ubuntu 24.04, .NET SDK 10.0.100, runtime .NET/ASP.NET Core 10.0.0.
- Package ứng dụng giữ nguyên EF Core SQL Server 10.0.12.
- SQL Server 2025 Developer 17.0.5005.3, Database kiểm thử mới tách riêng.
- Không thay provider bằng InMemory/SQLite; dùng chính Migration của ứng dụng.

## Kết quả

| Kiểm tra | Kết quả |
|---|---|
| Restore ứng dụng | Thành công |
| Build C# và Razor | 0 lỗi, 0 cảnh báo |
| EF has-pending-model-changes | Không có thay đổi model so với Migration |
| Bộ kiểm thử service + SQL Server | 72 xác nhận đạt |
| Bộ kiểm thử HTTP qua ứng dụng thật | 59 xác nhận đạt |
| Tổng xác nhận tự động | 131 |
| Phạm vi file nền | Chỉ Program.cs và layout sửa để tích hợp; các file nền khác giữ nguyên |
| Patch trên bản ZIP nền | git apply --check và áp dụng đối chiếu nội dung |

Log thực thi nằm ở thư mục `evidence`. Số xác nhận không có nghĩa đã chạy đủ mọi ca trong tài liệu TXT hoặc đã chạy UI của các branch chưa có.

## Đã kiểm tra trên SQL Server

- GET cá nhân chưa có không tạo dữ liệu; tạo/cập nhật đúng tài khoản, không sửa TaiKhoan.
- Họ tên trắng, Email/điện thoại thiếu/sai, ngày sinh thiếu/mặc định/tương lai, enum sai, số lần thiếu/âm và độ dài từng trường.
- Hai DbContext cùng khai báo lần đầu chỉ có một hồ sơ cá nhân.
- Tài khoản khóa/sai vai trò bị service chặn; thí sinh ngừng vẫn sửa cá nhân nhưng không tự bật trạng thái.
- Nộp hợp lệ: đúng chủ, ngày hệ thống, ChoDuyet, thông tin xét duyệt null.
- Thiếu hồ sơ cá nhân, dữ liệu bắt buộc cũ không đầy đủ, form nộp sai/quá dài.
- Chặn từng trạng thái kỳ thi không mở, chưa tới hạn, quá hạn; cho phép đúng biên bắt đầu/hạn cuối.
- POST kiểm tra lại khi thời gian qua hạn sau GET.
- Chống trùng riêng ChoDuyet/DuDieuKien/ChoThi/DaThi; nộp mới sau trạng thái cuối theo TXT.
- Không chặn nhầm thí sinh khác cùng kỳ hoặc cùng người kỳ khác.
- B không đọc/hủy được hồ sơ A.
- Hủy giữ bản ghi, ngày nộp và thông tin xét duyệt; chặn trạng thái không cho phép, lịch hiệu lực hoặc kết quả.
- Lịch DaHuy được giữ làm lịch sử và không chặn hủy khi không còn lịch hiệu lực.
- Hiển thị kết quả khớp trạng thái; không đưa kết quả lệch trạng thái xuống View.
- Hai DbContext nộp đồng thời chỉ commit một hồ sơ.
- Hủy đồng thời gọi Module5Service lập lịch không để hồ sơ DaHuy có lịch mới hiệu lực.

## Đã kiểm tra qua HTTP thật

Ứng dụng được chạy bằng Program.cs thật, dùng Session, AuthController, PhanQuyenAttribute và anti-forgery thực tế.

- Chặn các URL Module 3 khi chưa đăng nhập; đăng nhập A/B/Admin/Cán bộ/tài khoản chưa có cá nhân.
- Chặn thí sinh vào quản trị, Admin/cán bộ nộp thay và cán bộ đổi trạng thái dành riêng Admin.
- Các trang Razor cá nhân, form, quản trị, nộp, theo dõi và xác nhận hủy trả kết quả hợp lệ.
- POST cá nhân bỏ qua mã chủ sở hữu/trạng thái giả mạo; không cập nhật nhầm B.
- POST nộp bỏ qua MaThiSinh/NgayNop/TrangThai/NgayXuLy/NhanXet/MaNguoiXuLy giả mạo.
- POST thiếu token bị 400; GET hủy không thay dữ liệu; POST hủy hợp lệ chuyển DaHuy.
- POST trùng báo lỗi và giữ danh sách chọn; kỳ thi không tồn tại không bị tự chọn thay.
- Trang cá nhân/menu mới và các trang LichThi/KetQuaThi/Home cũ cùng mở được.
- Đăng xuất thật và URL cũ bị yêu cầu đăng nhập lại.

## Tự chạy lại bộ SQL

Từ thư mục Solution, dùng SQL Server kiểm thử riêng và quyền tạo Database fixture:

```powershell
$env:MODULE3_TEST_CONNECTION='Server=TEN_SERVER;Trusted_Connection=True;TrustServerCertificate=True'
$env:MODULE3_TEST_METADATA="$PWD\module3-test-metadata.json"
dotnet run --project .\tests\Module3.Integration\Module3.Integration.csproj
```

Runner tạo Database `Module3_Test_<GUID>` mới trên server đó, không xóa hoặc sử dụng Database chính của nhóm. Database kiểm thử được giữ lại để đối chiếu. Không commit metadata/mật khẩu cá nhân.

Để chạy HTTP tiếp, đặt `ConnectionStrings__DefaultConnection` trỏ Database fixture có tên trong metadata, chạy ứng dụng thật ở cổng 5075; trong cửa sổ khác:

```powershell
python .\tests\Module3.Integration\http_checks.py --metadata .\module3-test-metadata.json --base-url http://127.0.0.1:5075
```

Tài khoản fixture `m3_a`, `m3_b`, `m3_admin`, `m3_staff`, `m3_fresh` dùng mật khẩu thử `Module3@Test123`, chỉ có trong Database kiểm thử. Chạy HTTP một lần trên fixture mới; các ca này có tạo/hủy hồ sơ và cập nhật dữ liệu thử.

## Còn cần kiểm tra sau tích hợp nhóm

- Chưa push/merge GitHub vì người dùng chỉ cung cấp ZIP, chưa có URL/commit repository.
- Chưa chạy UI xét duyệt Module 4 hoặc UI quản lý kỳ thi Module 2 vì chúng chưa có trong bản nền. Fixture không được coi là bằng chứng cho UI của những module đó.
- Sau merge thật phải chạy luồng nộp → duyệt/từ chối → lịch → hoàn thành → kết quả bằng chức năng thật và thử cạnh tranh với Module 4.
- Chưa kiểm thử mọi trình duyệt/thiết bị, email/upload hoặc chức năng nâng cao ngoài phạm vi.
- Không đổi cấu hình xử lý lỗi đồng thời của Module 5; kiểm thử cạnh tranh ở mức service, còn thông báo lỗi UI của Module 5 thuộc người phụ trách module đó.
