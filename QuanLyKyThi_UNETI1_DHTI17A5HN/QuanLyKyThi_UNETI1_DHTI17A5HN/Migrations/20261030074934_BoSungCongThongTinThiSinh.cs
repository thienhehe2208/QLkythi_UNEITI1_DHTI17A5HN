using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKyThi_UNETI1_TI17A5HN.Migrations
{
    /// <inheritdoc />
    public partial class BoSungCongThongTinThiSinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LePhiDuThi",
                table: "KyThi",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MaNganhNghe",
                table: "ChungChi",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChungChiYeuThich",
                columns: table => new
                {
                    MaThiSinh = table.Column<int>(type: "int", nullable: false),
                    MaChungChi = table.Column<int>(type: "int", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChungChiYeuThich", x => new { x.MaThiSinh, x.MaChungChi });
                    table.ForeignKey(
                        name: "FK_ChungChiYeuThich_ChungChi_MaChungChi",
                        column: x => x.MaChungChi,
                        principalTable: "ChungChi",
                        principalColumn: "MaChungChi",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChungChiYeuThich_ThiSinh_MaThiSinh",
                        column: x => x.MaThiSinh,
                        principalTable: "ThiSinh",
                        principalColumn: "MaThiSinh",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NganhNghe",
                columns: table => new
                {
                    MaNganhNghe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNganhNghe = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BieuTuong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DangHienThi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NganhNghe", x => x.MaNganhNghe);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChungChi_MaNganhNghe",
                table: "ChungChi",
                column: "MaNganhNghe");

            migrationBuilder.CreateIndex(
                name: "IX_ChungChiYeuThich_MaChungChi",
                table: "ChungChiYeuThich",
                column: "MaChungChi");

            migrationBuilder.CreateIndex(
                name: "IX_NganhNghe_TenNganhNghe",
                table: "NganhNghe",
                column: "TenNganhNghe",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChungChi_NganhNghe_MaNganhNghe",
                table: "ChungChi",
                column: "MaNganhNghe",
                principalTable: "NganhNghe",
                principalColumn: "MaNganhNghe",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChungChi_NganhNghe_MaNganhNghe",
                table: "ChungChi");

            migrationBuilder.DropTable(
                name: "ChungChiYeuThich");

            migrationBuilder.DropTable(
                name: "NganhNghe");

            migrationBuilder.DropIndex(
                name: "IX_ChungChi_MaNganhNghe",
                table: "ChungChi");

            migrationBuilder.DropColumn(
                name: "LePhiDuThi",
                table: "KyThi");

            migrationBuilder.DropColumn(
                name: "MaNganhNghe",
                table: "ChungChi");
        }
    }
}
