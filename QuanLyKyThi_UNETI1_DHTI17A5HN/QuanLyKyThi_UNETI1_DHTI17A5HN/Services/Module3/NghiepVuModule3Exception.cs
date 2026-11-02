// ============================================================
// Họ và tên: Chu Văn Trọng
// Nội dung thực hiện: Module 3 - lỗi nghiệp vụ hiển thị an toàn.
// ============================================================

namespace QuanLyKyThi_UNETI1_TI17A5HN.Services.Module3;

public class NghiepVuModule3Exception(string maLoi, string message, string truong = "") : Exception(message)
{
    public string MaLoi { get; } = maLoi;
    public string Truong { get; } = truong;
}
