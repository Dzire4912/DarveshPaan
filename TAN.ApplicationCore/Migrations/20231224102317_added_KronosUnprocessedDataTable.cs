using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAN.ApplicationCore.Migrations
{
    /// <inheritdoc />
    public partial class added_KronosUnprocessedDataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KronosPunchExportUnprocessedData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    facilityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NATIVEKRONOSID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DEPARTMENTNUMBER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JOBCODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADJUSTEDAPPLYDATE = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PayCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TIMEINHOURS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HrJob = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KronosPunchExportUnprocessedData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KronosPunchExportUnprocessedData_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KronosPunchExportUnprocessedData_OrganizationId",
                table: "KronosPunchExportUnprocessedData",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KronosPunchExportUnprocessedData");
        }
    }
}
