using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAN.ApplicationCore.Migrations
{
    /// <inheritdoc />
    public partial class FacilityLunchTimeDeduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasDeductions",
                table: "Organizations");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "kronosSftpFileEndpoints",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "KronosBlobStorageCreds",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasDeduction",
                table: "Facilities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BreakDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    RegularTime = table.Column<int>(type: "int", nullable: false),
                    OverTime = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreakDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BreakDeductions_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BreakDeductions_FacilityId",
                table: "BreakDeductions",
                column: "FacilityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BreakDeductions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "kronosSftpFileEndpoints");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "KronosBlobStorageCreds");

            migrationBuilder.DropColumn(
                name: "HasDeduction",
                table: "Facilities");

            migrationBuilder.AddColumn<bool>(
                name: "HasDeductions",
                table: "Organizations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
