// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: ViewModel phân trang dùng chung (partial _PhanTrang),
//   giữ lại tham số tìm kiếm/lọc/sắp xếp khi chuyển trang.
// ============================================================

namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels
{
    public class PhanTrangViewModel
    {
        public const int KichThuocMacDinh = 10;

        public int TrangHienTai { get; set; } = 1;
        public int KichThuocTrang { get; set; } = KichThuocMacDinh;
        public int TongSoBanGhi { get; set; }

        public int TongSoTrang => TongSoBanGhi == 0 ? 1 : (int)Math.Ceiling(TongSoBanGhi / (double)KichThuocTrang);
        public bool CoTrangTruoc => TrangHienTai > 1;
        public bool CoTrangSau => TrangHienTai < TongSoTrang;

        /// <summary>Action đích của link phân trang (mặc định Index).</summary>
        public string Action { get; set; } = "Index";

        /// <summary>Tham số query string cần giữ khi chuyển trang (từ khóa, bộ lọc, sắp xếp).</summary>
        public Dictionary<string, string?> ThamSo { get; set; } = new();

        /// <summary>Chuẩn hóa số trang về khoảng hợp lệ [1, TongSoTrang].</summary>
        public static int ChuanHoaTrang(int? trang, int tongSoBanGhi, int kichThuocTrang = KichThuocMacDinh)
        {
            var tongSoTrang = tongSoBanGhi == 0 ? 1 : (int)Math.Ceiling(tongSoBanGhi / (double)kichThuocTrang);
            var t = trang ?? 1;
            if (t < 1) t = 1;
            if (t > tongSoTrang) t = tongSoTrang;
            return t;
        }

        /// <summary>Tạo route values cho một trang cụ thể (gộp tham số đang lọc + trang).</summary>
        public Dictionary<string, string> RouteChoTrang(int trang)
        {
            var route = ThamSo
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .ToDictionary(kv => kv.Key, kv => kv.Value!);
            route["trang"] = trang.ToString();
            return route;
        }
    }
}
