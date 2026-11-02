// ============================================================
// Họ và tên: Chu Văn Trọng
// Nội dung thực hiện: Module 3 - nhận diện lỗi dữ liệu an toàn.
// ============================================================

using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace QuanLyKyThi_UNETI1_TI17A5HN.Services.Module3;

public static class LoiDuLieuModule3
{
    public static bool LaLoiDatabase(Exception error)
    {
        if (error is DbException or DbUpdateException) return true;
        return error is InvalidOperationException && error.InnerException is { } inner
            && LaLoiDatabase(inner);
    }
}
