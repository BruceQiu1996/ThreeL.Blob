using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThreeL.Blob.Clients.Win.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdInTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "UploadFileRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "TransferCompleteRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "DownloadFileRecords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UploadFileRecords");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TransferCompleteRecords");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DownloadFileRecords");
        }
    }
}
