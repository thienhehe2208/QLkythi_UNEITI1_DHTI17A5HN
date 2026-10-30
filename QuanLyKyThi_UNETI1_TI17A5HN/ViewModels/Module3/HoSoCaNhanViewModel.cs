// Họ và tên: CHƯA CUNG CẤP. Mã sinh viên: CHƯA CUNG CẤP.
// Nội dung thực hiện: Module 3 — thông tin cá nhân hiển thị, không chứa mật khẩu tài khoản.
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels.Module3;

public class HoSoCaNhanViewModel : HoSoCaNhanFormViewModel
{
    public int MaThiSinh { get; set; }
    public TrangThaiThiSinh TrangThai { get; set; }
    public bool DaKhaiBao => MaThiSinh > 0;
    public List<string> ThongTinCanBoSung { get; set; } = [];
}
