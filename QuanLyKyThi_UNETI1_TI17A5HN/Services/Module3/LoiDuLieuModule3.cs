// Họ và tên/Mã sinh viên: CHƯA CUNG CẤP.
// Nội dung: nhận diện cả lỗi SQL được EF bọc trong lỗi transient; không bắt nhầm lỗi lập trình.
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Services.Module3;

public static class LoiDuLieuModule3
{
    public static bool LaLoiDatabase(Exception error)
    {
        if (error is DbException or DbUpdateException) return true;
        return error is InvalidOperationException && error.InnerException is { } inner && LaLoiDatabase(inner);
    }
}
