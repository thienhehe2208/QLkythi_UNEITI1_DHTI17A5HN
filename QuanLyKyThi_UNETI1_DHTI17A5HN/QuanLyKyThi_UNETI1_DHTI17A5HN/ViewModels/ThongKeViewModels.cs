// Phạm Quang Minh - 23103100201: ViewModel thống kê Module 5.
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
namespace QuanLyKyThi_UNETI1_TI17A5HN.ViewModels
{
    public class ThongKeViewModel
    {
        public int Nam { get; set; }
        public List<ThongKeChungChiViewModel> ChungChis { get; set; } = new();
        public List<ThongKeKyThiViewModel> KyThis { get; set; } = new();
        public List<ThongKeTrangThaiViewModel> TrangThais { get; set; } = new();
        public List<ThongKeThangViewModel> Thangs { get; set; } = new();
        public int SoDaThi => KyThis.Sum(k => k.SoDaThi);
        public int SoDat => KyThis.Sum(k => k.SoDat);
        public decimal TyLeDat => TyLe(SoDat, SoDaThi);
        public static decimal TyLe(int tuSo, int mauSo) => mauSo == 0 ? 0 : Math.Round(100m * tuSo / mauSo, 1);
    }
    public class ThongKeChungChiViewModel
    {
        public string Ten { get; set; } = "";
        public int SoKyThi { get; set; }
        public int SoDat { get; set; }
    }
    public class ThongKeKyThiViewModel
    {
        public int MaKyThi { get; set; }
        public string Ten { get; set; } = "";
        public int TongHoSo { get; set; }
        public int SoDuDieuKien { get; set; }
        public int SoQuaKiemTra { get; set; }
        public int SoKhongDuDieuKien { get; set; }
        public int SoDaThi { get; set; }
        public int SoDat { get; set; }
        public decimal TyLeQuaKiemTra => ThongKeViewModel.TyLe(SoQuaKiemTra, SoQuaKiemTra + SoKhongDuDieuKien);
    }
    public class ThongKeTrangThaiViewModel
    {
        public TrangThaiHoSo TrangThai { get; set; }
        public int SoLuong { get; set; }
    }
    public class ThongKeThangViewModel
    {
        public int Thang { get; set; }
        public int SoHoSo { get; set; }
        public int SoLichThi { get; set; }
    }
}
