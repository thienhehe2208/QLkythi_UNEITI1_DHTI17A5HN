using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Models
{
    public class KyThi
    {
        [Key] public int MaKyThi { get; set; }
        public string TenKyThi { get; set; } = string.Empty;
        public int MaChungChi { get; set; }
        // ... các trường khác

        [ForeignKey("MaChungChi")]
        public ChungChi? ChungChi { get; set; }
    }
}
