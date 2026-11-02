using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKyThi_UNETI1_TI17A5HN.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChungChi",
                columns: table => new
                {
                    MaChungChi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenChungChi = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmailLienHe = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChungChi", x => x.MaChungChi);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.MaTaiKhoan);
                });

            migrationBuilder.CreateTable(
                name: "KyThi",
                columns: table => new
                {
                    MaKyThi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKyThi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaChungChi = table.Column<int>(type: "int", nullable: false),
                    SoChoThiToiDa = table.Column<int>(type: "int", nullable: false),
                    CapDoThi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DiemDauVaoToiThieu = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    NgayBatDauDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HanDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MoTaKyThi = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DieuKienDuThi = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KyThi", x => x.MaKyThi);
                    table.ForeignKey(
                        name: "FK_KyThi_ChungChi_MaChungChi",
                        column: x => x.MaChungChi,
                        principalTable: "ChungChi",
                        principalColumn: "MaChungChi",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThiSinh",
                columns: table => new
                {
                    MaThiSinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "date", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrinhDoHienTai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NgheNghiep = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SoLanThiTruocDay = table.Column<int>(type: "int", nullable: false),
                    ChungChiDaCo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThiSinh", x => x.MaThiSinh);
                    table.ForeignKey(
                        name: "FK_ThiSinh_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoSoDangKyThi",
                columns: table => new
                {
                    MaHoSo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaThiSinh = table.Column<int>(type: "int", nullable: false),
                    MaKyThi = table.Column<int>(type: "int", nullable: false),
                    NgayNop = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MucDichDuThi = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayXuLy = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NhanXetXetDuyet = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MaNguoiXuLy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoSoDangKyThi", x => x.MaHoSo);
                    table.ForeignKey(
                        name: "FK_HoSoDangKyThi_KyThi_MaKyThi",
                        column: x => x.MaKyThi,
                        principalTable: "KyThi",
                        principalColumn: "MaKyThi",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoSoDangKyThi_TaiKhoan_MaNguoiXuLy",
                        column: x => x.MaNguoiXuLy,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoSoDangKyThi_ThiSinh_MaThiSinh",
                        column: x => x.MaThiSinh,
                        principalTable: "ThiSinh",
                        principalColumn: "MaThiSinh",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KetQuaThi",
                columns: table => new
                {
                    MaKetQuaThi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoSo = table.Column<int>(type: "int", nullable: false),
                    DiemDanhGia = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    NhanXet = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    KetQua = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetQuaThi", x => x.MaKetQuaThi);
                    table.ForeignKey(
                        name: "FK_KetQuaThi_HoSoDangKyThi_MaHoSo",
                        column: x => x.MaHoSo,
                        principalTable: "HoSoDangKyThi",
                        principalColumn: "MaHoSo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichThi",
                columns: table => new
                {
                    MaLichThi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoSo = table.Column<int>(type: "int", nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HinhThucThi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiaDiemHoacLienKet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhongThi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GiamThi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichThi", x => x.MaLichThi);
                    table.ForeignKey(
                        name: "FK_LichThi_HoSoDangKyThi_MaHoSo",
                        column: x => x.MaHoSo,
                        principalTable: "HoSoDangKyThi",
                        principalColumn: "MaHoSo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChungChi_TenChungChi",
                table: "ChungChi",
                column: "TenChungChi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoSoDangKyThi_MaKyThi",
                table: "HoSoDangKyThi",
                column: "MaKyThi");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoDangKyThi_MaNguoiXuLy",
                table: "HoSoDangKyThi",
                column: "MaNguoiXuLy");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoDangKyThi_MaThiSinh_MaKyThi",
                table: "HoSoDangKyThi",
                columns: new[] { "MaThiSinh", "MaKyThi" });

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaThi_MaHoSo",
                table: "KetQuaThi",
                column: "MaHoSo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KyThi_MaChungChi",
                table: "KyThi",
                column: "MaChungChi");

            migrationBuilder.CreateIndex(
                name: "IX_LichThi_MaHoSo",
                table: "LichThi",
                column: "MaHoSo");

            migrationBuilder.CreateIndex(
                name: "IX_LichThi_ThoiGianBatDau_ThoiGianKetThuc",
                table: "LichThi",
                columns: new[] { "ThoiGianBatDau", "ThoiGianKetThuc" });

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_TenDangNhap",
                table: "TaiKhoan",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThiSinh_MaTaiKhoan",
                table: "ThiSinh",
                column: "MaTaiKhoan",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KetQuaThi");

            migrationBuilder.DropTable(
                name: "LichThi");

            migrationBuilder.DropTable(
                name: "HoSoDangKyThi");

            migrationBuilder.DropTable(
                name: "KyThi");

            migrationBuilder.DropTable(
                name: "ThiSinh");

            migrationBuilder.DropTable(
                name: "ChungChi");

            migrationBuilder.DropTable(
                name: "TaiKhoan");
        }
    }
}
