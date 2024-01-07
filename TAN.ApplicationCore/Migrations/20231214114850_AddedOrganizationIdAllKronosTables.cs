using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAN.ApplicationCore.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrganizationIdAllKronosTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "kronosTimeSheetMapped",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "KronosPunchExport",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrgId",
                table: "kronosPendingRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "KronosDataTracker",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ExecutionSpResults",
                columns: table => new
                {
                    result = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_KronosPunchExport_OrganizationId",
                table: "KronosPunchExport",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_KronosPunchExport_Organizations_OrganizationId",
                table: "KronosPunchExport",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KronosPunchExport_Organizations_OrganizationId",
                table: "KronosPunchExport");

            migrationBuilder.DropTable(
                name: "ExecutionSpResults");

            migrationBuilder.DropIndex(
                name: "IX_KronosPunchExport_OrganizationId",
                table: "KronosPunchExport");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "kronosTimeSheetMapped");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "KronosPunchExport");

            migrationBuilder.DropColumn(
                name: "OrgId",
                table: "kronosPendingRecords");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "KronosDataTracker");
        }
    }
}
