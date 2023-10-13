using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThreeL.Blob.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Initializedatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4 ");

            migrationBuilder.CreateTable(
                name: "FileObject",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4 "),
                    Location = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4 "),
                    Code = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4 "),
                    Size = table.Column<long>(type: "bigint", nullable: true),
                    ParentFolder = table.Column<long>(type: "bigint", nullable: true),
                    CreateBy = table.Column<long>(type: "bigint", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsFolder = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    TempFileLocation = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4 ")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileObject", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4 ");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    UserName = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4 "),
                    Password = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4 "),
                    CreateBy = table.Column<long>(type: "bigint", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    LastLoginTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    UploadMaxSizeLimit = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1073741824L),
                    DaliyUploadMaxSizeLimit = table.Column<long>(type: "bigint", nullable: false, defaultValue: 10737418240L),
                    TodayUploadMaxSize = table.Column<long>(type: "bigint", nullable: false),
                    DownloadSpeedLimit = table.Column<long>(type: "bigint", nullable: true),
                    MaxSpaceSize = table.Column<long>(type: "bigint", nullable: true),
                    Location = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4 ")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4 ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileObject");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
