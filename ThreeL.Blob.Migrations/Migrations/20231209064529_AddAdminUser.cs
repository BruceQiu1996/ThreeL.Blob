using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThreeL.Blob.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Avatar", "CreateBy", "CreateTime", "DownloadSpeedLimit", "LastLoginTime", "Location", "MaxSpaceSize", "Password", "Role", "TodayUploadMaxSize", "UserName" },
                values: new object[] { 1L, null, 0L, new DateTime(2023, 12, 9, 14, 45, 29, 497, DateTimeKind.Local).AddTicks(1172), null, null, "D:\\ThreeL_blob\\admin", null, "87NLwc3m69ImImDt4PMbig==.cxgVDtWaQO7Z0p/2iAuPXQ6C1jM5Udt/qtW9m0ARpCY=", 1, 0L, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1L);
        }
    }
}
