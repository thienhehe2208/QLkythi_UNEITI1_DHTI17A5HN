// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Bộ lọc phân quyền phía server dựa trên Session.
//   - Chưa đăng nhập -> chuyển tới trang đăng nhập (kèm returnUrl).
//   - Tài khoản không còn tồn tại hoặc đã bị khóa -> xóa Session, buộc đăng nhập lại.
//   - Sai vai trò -> trang "Không có quyền truy cập" (HTTP 403).
//   Cách dùng: [PhanQuyen] (chỉ cần đăng nhập) hoặc [PhanQuyen(VaiTro.Admin, ...)].
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using QuanLyKyThi_UNETI1_TI17A5HN.Data;
using QuanLyKyThi_UNETI1_TI17A5HN.Models.Enums;
using QuanLyKyThi_UNETI1_TI17A5HN.Services;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PhanQuyenAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly VaiTro[] _vaiTroDuocPhep;

        /// <param name="vaiTroDuocPhep">Để trống = mọi vai trò đã đăng nhập đều được phép.</param>
        public PhanQuyenAttribute(params VaiTro[] vaiTroDuocPhep)
        {
            _vaiTroDuocPhep = vaiTroDuocPhep;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var http = context.HttpContext;
            var session = http.Session;

            // Không cho trình duyệt lưu cache trang cần đăng nhập
            // (tránh bấm Back sau khi đăng xuất vẫn thấy dữ liệu cũ).
            http.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            http.Response.Headers.Pragma = "no-cache";

            var maTaiKhoan = session.LayMaTaiKhoan();
            if (maTaiKhoan is null)
            {
                var returnUrl = http.Request.Path + http.Request.QueryString;
                context.Result = new RedirectToActionResult("DangNhap", "Auth", new { returnUrl });
                return;
            }

            // Kiểm tra lại với database ở mỗi request: tài khoản bị khóa/xóa sau khi
            // đăng nhập thì Session cũ không còn dùng được.
            var db = http.RequestServices.GetRequiredService<ApplicationDbContext>();
            var taiKhoan = await db.TaiKhoans
                .AsNoTracking()
                .Where(t => t.MaTaiKhoan == maTaiKhoan.Value)
                .Select(t => new { t.HoTen, t.VaiTro, t.TrangThai })
                .FirstOrDefaultAsync();

            if (taiKhoan is null || taiKhoan.TrangThai == TrangThaiTaiKhoan.BiKhoa)
            {
                session.Clear();
                var tempData = http.RequestServices
                    .GetRequiredService<ITempDataDictionaryFactory>()
                    .GetTempData(http);
                tempData["LoiThongBao"] = taiKhoan is null
                    ? "Tài khoản không còn tồn tại. Vui lòng đăng nhập lại."
                    : "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.";
                // Authorization filter chặn sớm nên SaveTempDataFilter không chạy -> lưu thủ công.
                tempData.Save();
                context.Result = new RedirectToActionResult("DangNhap", "Auth", null);
                return;
            }

            // Đồng bộ Session với dữ liệu mới nhất (vd. Admin vừa đổi vai trò/họ tên).
            if (session.LayVaiTro() != taiKhoan.VaiTro || session.LayHoTen() != taiKhoan.HoTen)
            {
                session.SetString(PhienDangNhap.KhoaHoTen, taiKhoan.HoTen);
                session.SetString(PhienDangNhap.KhoaVaiTro, taiKhoan.VaiTro.ToString());
            }

            if (_vaiTroDuocPhep.Length > 0 && !_vaiTroDuocPhep.Contains(taiKhoan.VaiTro))
            {
                context.Result = new RedirectToActionResult("TuChoiTruyCap", "Auth", null);
            }
        }
    }
}
