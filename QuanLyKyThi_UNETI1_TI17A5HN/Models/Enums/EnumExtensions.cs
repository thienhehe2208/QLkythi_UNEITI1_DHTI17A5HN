// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Hàm tiện ích lấy nhãn tiếng Việt của enum để hiển thị trên View.
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums
{
    public static class EnumExtensions
    {
        /// <summary>Trả về [Display(Name)] của giá trị enum, nếu không có thì trả về tên enum.</summary>
        public static string HienThi(this Enum giaTri)
        {
            var member = giaTri.GetType().GetMember(giaTri.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? giaTri.ToString();
        }
    }
}
