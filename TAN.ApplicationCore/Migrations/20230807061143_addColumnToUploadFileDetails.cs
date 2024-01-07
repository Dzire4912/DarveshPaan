using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAN.ApplicationCore.Migrations
{
    /// <inheritdoc />
    public partial class addColumnToUploadFileDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReportQuarter",
                table: "UploadFileDetails",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Year",
                table: "UploadFileDetails",
                type: "int",
                nullable: true);
            migrationBuilder.AddColumn<string>(
                name: "Month",
                table: "UploadFileDetails",
                type: "int",
                nullable: true); 
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportQuarter",
                table: "UploadFileDetails");
            migrationBuilder.DropColumn(
                name: "Year",
                table: "UploadFileDetails");
            migrationBuilder.DropColumn(
                name: "Month",
                table: "UploadFileDetails");
        }
    }
}
