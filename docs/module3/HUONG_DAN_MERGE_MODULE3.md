# Áp dụng Module 3 vào branch của nhóm

ZIP bàn giao dùng để chạy độc lập với đầy đủ mã nền. Khi đưa vào repository thật, dùng patch để giữ phạm vi Module 3. Không chép toàn bộ thư mục Models/Migrations/Views/Shared đè lên các nhánh khác.

## Chuẩn bị branch thật

Tại repository đã clone từ URL thật của nhóm, kiểm tra và lưu các thay đổi đang có trước. Không dùng reset --hard hoặc force push để làm sạch công việc.

```bash
git status
git fetch origin
git switch main
git pull --ff-only origin main
git rev-parse HEAD
git switch -c feature/module-3
```

Nếu branch đã tồn tại thì chuyển vào branch đó và cập nhật theo quy trình nhóm, không tạo đè. Ghi commit main thật vào PR; ZIP nền không cung cấp SHA GitHub.

## Áp dụng patch

Lấy `MODULE3_ONLY.patch` trong gói, đặt ngoài repository hoặc dùng đường dẫn đầy đủ:

```bash
git apply --check /duong/dan/MODULE3_ONLY.patch
git apply /duong/dan/MODULE3_ONLY.patch
git diff --check
git status --short
```

Patch đã được kiểm tra áp dụng trên snapshot ZIP gốc. Nếu main mới đã đổi Program/layout hoặc đã có file trùng tên, lệnh check có thể từ chối. Khi đó rà từng thay đổi; không ép ghi đè hoặc xóa file của module khác.

Hai patch dùng chung cần hợp nhất thủ công nếu có xung đột:

1. Program.cs: giữ đăng ký cũ, thêm đúng `AddScoped<QuanLyKyThi_UNETI1_TI17A5HN.Services.Module3.Module3Service>()` trước builder.Build().
2. Views/Shared/_Layout.cshtml: thêm menu cá nhân/đã nộp/nộp mới cho ThiSinh; quản lý thí sinh cho Admin/CanBoKhaoThi. Giữ đăng xuất, script, modal và menu khác.

Tất cả các file còn lại của phần chức năng là file mới trong Controller, Services/Module3, ViewModels/Module3, Views/ThiSinh, Views/HoSoThiSinh. Bộ test ở tests/Module3.Integration nằm riêng, không đổi csproj/solution ứng dụng.

## Kiểm tra và gửi Pull Request

1. Điền họ tên và mã sinh viên thật vào các chỗ CHƯA CUNG CẤP thuộc phần đóng góp mới.
2. Chạy các lệnh restore/build/kiểm tra Migration trong README.
3. Chạy bộ test trên SQL Server kiểm thử riêng và thử lại các trang mới trên giao diện.
4. Khi Module 4 đã merge, chạy luồng thật: đăng nhập → cá nhân → nộp → duyệt/từ chối → lịch → hoàn thành → kết quả → theo dõi.
5. Kiểm tra với hai thí sinh A/B, trường hợp hủy và tài khoản bị khóa.
6. Stage đúng file theo danh sách bàn giao, rà `git diff --cached`; tạo commit bằng tài khoản và thông tin thật của người thực hiện.
7. Push branch và mở PR về main theo quy trình nhóm; không push thẳng main.

```bash
git diff --name-status origin/main...HEAD
git diff --check
git status --short
git push -u origin feature/module-3
```

Trước khi có commit, lệnh so sánh HEAD không bao gồm working tree; cần rà thêm `git diff` và phần staged. Không coi “không xung đột Git” là bằng chứng nghiệp vụ tích hợp đã đúng.

## Phạm vi được giữ nguyên

Model/enum/DbContext/InitialCreate/snapshot, Auth, Session, bộ lọc quyền, seed, package, connection string, route mặc định và các file riêng Module 5 giữ nguyên so với ZIP gốc. Không cần Migration mới.

Các phần chưa có trong main như quản lý kỳ thi, xét duyệt, Dashboard/thống kê vẫn thuộc branch tương ứng. Gói Module 3 không tự xây chúng để tạo một bản “đủ mọi module”.
