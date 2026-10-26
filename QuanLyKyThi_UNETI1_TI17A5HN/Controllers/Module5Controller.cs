// Họ và tên: Phạm Quang Minh
// Mã sinh viên: 23103100201
// Nội dung thực hiện: Trang vào Module 5 trên nền main chưa có HomeController.
using Microsoft.AspNetCore.Mvc;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Controllers
{
    public class Module5Controller : Controller
    {
        public IActionResult Index() => View();
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Loi()
        {
            Response.StatusCode = 500;
            return View();
        }
    }
}
