using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Models
{
    public class ThiSinh
    {
        [Key] public int MaThiSinh { get; set; }
        public int MaTaiKhoan { get; set; }
        public string HoTen { get; set; } = string.Empty;
        // ... các trường khác

        [ForeignKey("MaTaiKhoan")]
        public TaiKhoan? TaiKhoan { get; set; }
    }
}
