using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThreeL.Blob.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddFileLastUpdateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdateTime",
                table: "FileObject",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdateTime",
                table: "FileObject");
        }
    }
}
