using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThreeL.Blob.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddFileTrackPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrackPath",
                table: "FileObject",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4 ");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreateTime",
                value: new DateTime(2024, 2, 4, 15, 3, 7, 621, DateTimeKind.Local).AddTicks(1763));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrackPath",
                table: "FileObject");

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreateTime",
                value: new DateTime(2023, 12, 9, 14, 45, 29, 497, DateTimeKind.Local).AddTicks(1172));
        }
    }
}
