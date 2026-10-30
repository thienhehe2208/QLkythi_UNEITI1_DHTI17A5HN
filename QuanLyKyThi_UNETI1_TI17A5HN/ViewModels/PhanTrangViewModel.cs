// Họ và tên: Phạm Quang Minh
// Mã sinh viên: 23103100201
// Nội dung thực hiện: Phân trang tương thích quy ước PhanTrangViewModel của nhóm,
// dùng trong Module 5 khi main chưa có thành phần chung này.
namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels
{
    public class PhanTrangViewModel
    {
        public const int KichThuocMacDinh = 10;
        public int TrangHienTai { get; set; } = 1;
        public int KichThuocTrang { get; set; } = KichThuocMacDinh;
        public int TongSoBanGhi { get; set; }
        public int TongSoTrang => Math.Max(1, (int)Math.Ceiling(TongSoBanGhi / (double)KichThuocTrang));
        public bool CoTrangTruoc => TrangHienTai > 1;
        public bool CoTrangSau => TrangHienTai < TongSoTrang;
        public string Action { get; set; } = "Index";
        public Dictionary<string, string?> ThamSo { get; set; } = new();

        public static int ChuanHoaTrang(int? trang, int tongSoBanGhi, int kichThuocTrang = KichThuocMacDinh)
        {
            var tongTrang = Math.Max(1, (int)Math.Ceiling(tongSoBanGhi / (double)kichThuocTrang));
            return Math.Clamp(trang ?? 1, 1, tongTrang);
        }

        public Dictionary<string, string> RouteChoTrang(int trang)
        {
            var route = ThamSo.Where(p => !string.IsNullOrWhiteSpace(p.Value)).ToDictionary(p => p.Key, p => p.Value!);
            route["trang"] = trang.ToString();
            return route;
        }
    }
}
