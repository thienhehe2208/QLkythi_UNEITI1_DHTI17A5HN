# Quản lý kỳ thi chứng chỉ — Nhóm 1

Mở `QuanLyKyThi_UNETI1_TI17A5HN.slnx` ở thư mục gốc repository.
Solution này trỏ tới project tích hợp của nhóm trong:
`QuanLyKyThi_UNETI1_DHTI17A5HN/QuanLyKyThi_UNETI1_DHTI17A5HN`.
Bản source Module 5 riêng trước đây đã được hợp nhất vào project này.

## Chạy ứng dụng

```powershell
dotnet build QuanLyKyThi_UNETI1_TI17A5HN.slnx
dotnet run --project QuanLyKyThi_UNETI1_DHTI17A5HN/QuanLyKyThi_UNETI1_DHTI17A5HN --launch-profile http
```

Cấu hình SQL Server trong `appsettings.json` của project tích hợp. Khi khởi động,
ứng dụng dùng migration và bộ dữ liệu mẫu chung của nhóm. Dữ liệu mẫu chỉ tạo
mật khẩu cho tài khoản chưa tồn tại; không đổi mật khẩu tài khoản cũ.

## Module 5 — Phạm Quang Minh, 23103100201

- Tổng quan: đủ 9 chỉ số mục 9.5; lịch sắp tới chưa bắt đầu và chưa hủy.
- Lịch thi, kết quả thi: giữ nguyên nghiệp vụ của bản nhóm đã tích hợp.
- Thống kê: đủ 10 nội dung mục 9.6, tại `/ThongKe`, dành cho Admin/cán bộ.
- Bộ lọc năm chỉ áp dụng cho hai thống kê theo tháng; bảng khác tính toàn bộ dữ liệu.
- Qua kiểm tra: Đủ điều kiện, Chờ thi, Đã thi, Đạt, Không đạt.
  Tỷ lệ qua kiểm tra tính trên tổng hồ sơ đã có kết luận đủ/không đủ điều kiện;
  không tính Chờ duyệt và Đã hủy.
- Tỷ lệ đạt = Đạt / (Đã thi + Đạt + Không đạt). Có tính hồ sơ chưa nhập kết quả.
- Tháng không có dữ liệu hiển thị 0; mẫu số 0 hiển thị 0% kèm giải thích.
- Thống kê lịch theo tháng tính theo ngày bắt đầu, bao gồm lịch đã hủy.
- Không thêm bảng hay migration riêng cho Dashboard/thống kê.
