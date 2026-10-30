# Hợp đồng tích hợp Module 3

Gói dùng nguyên Entity, enum, DbSet, Session và Migration của ZIP nền. Không thêm bảng/cột, package hoặc provider; không sửa AuthController, bộ lọc quyền, seed hay mã Module 5.

## Quy tắc đã áp dụng theo TXT

| Quy tắc | Thực hiện |
|---|---|
| M3-POL-01 | Họ tên, ngày sinh, giới tính, điện thoại, Email, số lần thi hợp lệ; độ dài theo main. Các trường nullable khác vẫn tùy chọn. |
| M3-POL-02 | Chặn trùng cùng thí sinh/kỳ thi khi ChoDuyet, DuDieuKien, ChoThi, DaThi. |
| Nộp lại | DaHuy, KhongDuDieuKien, Dat, KhongDat không thuộc nhóm đang xử lý; có thể tạo hồ sơ mới khi kỳ thi và mọi điều kiện vẫn hợp lệ. Không sửa hồ sơ cũ. |
| M3-POL-03 | Hủy chỉ khi ChoDuyet/DuDieuKien, không có lịch khác DaHuy, không có kết quả và đúng chủ sở hữu. |
| Lịch đã hủy | Giữ lịch sử; không chặn hủy hồ sơ nếu không còn lịch khác chưa hủy. |
| M3-POL-04 | Kết quả được hiển thị khi bản ghi Dat/KhongDat khớp trạng thái cuối của hồ sơ; không thêm cột công bố. |
| M3-POL-05 | Chỉ cập nhật ThiSinh.HoTen/Email, không đồng bộ sang TaiKhoan hoặc Session. Hồ sơ cũ tham chiếu thông tin thí sinh hiện tại. |
| M3-POL-06 | Không thêm chức năng tự sửa hồ sơ đăng ký đã nộp. |
| Thời gian | TimeProvider hiện có, UTC+7; so sánh chính xác NgayBatDauDangKy <= thời điểm nộp <= HanDangKy. |

Đây là các quyết định triển khai theo TXT người dùng yêu cầu áp dụng; các chỗ đề Word chưa định nghĩa chi tiết không được trình bày như nguyên văn của đề. Nếu nhóm đổi chính sách, cần sửa và kiểm thử cùng các bên liên quan.

## Module 1

- Dùng `LayMaTaiKhoan`, `LaVaiTro` và `PhanQuyenAttribute` hiện có.
- Không nhận chủ sở hữu từ form/query. Service nhận mã tài khoản do Controller lấy từ Session.
- Tài khoản không có ThiSinh: GET không tạo, POST khai báo lần đầu tạo đúng liên kết unique.
- Thí sinh ngừng hoạt động vẫn xem/cập nhật cá nhân nhưng không tự bật lại trạng thái hoặc nộp mới.
- Admin đổi ThiSinh.TrangThai; khóa TaiKhoan thuộc Module 1.

## Module 2

Link từ trang kỳ thi có thể dùng:

```cshtml
<a asp-controller="HoSoThiSinh" asp-action="Create" asp-route-maKyThi="@Model.MaKyThi">Nộp hồ sơ</a>
```

Tên Model trong View Module 2 phụ thuộc code của module đó. Module 3 tự đọc cùng DbContext, không cần endpoint giả. Nếu Module 2 chưa merge, thí sinh vẫn chọn kỳ thi từ Database trên form Module 3. Module 2 phải lưu cùng quy ước thời hạn, đặc biệt nếu giao diện chỉ nhập ngày.

## Module 4

Module 3 tạo `HoSoDangKyThi`: MaThiSinh tin cậy, MaKyThi đã kiểm tra, NgayNop hệ thống, mục đích/ghi chú hợp lệ, ChoDuyet. NgayXuLy/NhanXetXetDuyet/MaNguoiXuLy để null.

Module 4 đọc bản ghi đó, chỉ duyệt/từ chối từ ChoDuyet, kiểm tra số chỗ và ghi thông tin xét duyệt. Khi cùng ghi một hồ sơ phải đọc trạng thái mới nhất trong transaction tương thích; không duyệt hồ sơ đã hủy. Module 3 chỉ đọc kết quả xét duyệt và khi hủy chỉ đổi TrangThai.

Controller phía thí sinh là `HoSoThiSinhController`, tách khỏi tên Controller tiếp nhận của Module 4 để giảm chồng lấn.

## Module 5

- Dùng nguyên Module5Service và các Controller/View hiện có.
- Đọc mọi lịch của đúng MaHoSo, phân biệt DaHuy; link lịch dùng MaLichThi thật.
- Link kết quả dùng MaKetQuaThi thật; Module 3 chỉ đưa kết quả xuống View khi trạng thái khớp.
- Module 5 hủy lịch đưa hồ sơ về DuDieuKien; Module 3 đánh giá lại điều kiện hủy.
- Không thêm nút thí sinh sửa/hủy lịch, nhập kết quả hoặc duyệt hồ sơ.
- Nếu Module 5 thêm công bố riêng sau này, người phụ trách phải thống nhất schema và giới hạn hiển thị tại cả endpoint Module 5; Module 3 không tự thay chức năng đó.

## Đồng thời và lỗi dữ liệu

Nộp, hủy, lưu hồ sơ cá nhân và đổi trạng thái dùng transaction ngắn Serializable. Các phép đọc kiểm tra và ghi cùng transaction. Khi SQL Server chọn deadlock victim, thao tác lỗi không được báo thành công; Controller Module 3 nhận diện cả DbException/DbUpdateException và InvalidOperationException bọc lỗi Database của EF, hiện yêu cầu tải lại/thử lại. Không tự bật retry toàn cục hoặc thay cấu hình Module 5.

Kiểm thử thực tế đã xác minh chống nộp trùng, tạo cá nhân đồng thời và hủy đồng thời Module 5 lập lịch. Điều này không thay cho việc kiểm thử branch Module 4 khi được tích hợp.
