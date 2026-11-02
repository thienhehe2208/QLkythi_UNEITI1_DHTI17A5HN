// ============================================================
// Nội dung bổ sung: Danh sách chứng chỉ yêu thích của thí sinh.
// Mỗi cặp thí sinh - chứng chỉ chỉ được lưu một lần.
// ============================================================

using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Models
{
    [Table("ChungChiYeuThich")]
    public class ChungChiYeuThich
    {
        public int MaThiSinh { get; set; }
        public int MaChungChi { get; set; }
        public DateTime NgayThem { get; set; } = DateTime.Now;

        public ThiSinh? ThiSinh { get; set; }
        public ChungChi? ChungChi { get; set; }
    }
}
