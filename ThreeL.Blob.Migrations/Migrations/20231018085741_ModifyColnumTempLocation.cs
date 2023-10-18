using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThreeL.Blob.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ModifyColnumTempLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TempFileLocation",
                table: "FileObject",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4 ")
                .OldAnnotation("MySql:CharSet", "utf8mb4 ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FileObject",
                keyColumn: "TempFileLocation",
                keyValue: null,
                column: "TempFileLocation",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "TempFileLocation",
                table: "FileObject",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4 ")
                .OldAnnotation("MySql:CharSet", "utf8mb4 ");
        }
    }
}
