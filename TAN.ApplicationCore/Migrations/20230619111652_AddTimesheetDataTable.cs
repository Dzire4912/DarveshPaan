using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAN.ApplicationCore.Migrations
{
    /// <inheritdoc />
    public partial class AddTimesheetDataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
              name: "Timesheet",
              columns: table => new
              {
                  Id = table.Column<long>(type: "bigint", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  FacilityId = table.Column<int>(type: "int", nullable: true),
                  AgencyId = table.Column<int>(type: "int", nullable: true),
                  ReportQuarter = table.Column<int>(type: "int", nullable: true),
                  Month = table.Column<int>(type: "int", nullable: true),
                  Year = table.Column<int>(type: "int", nullable: true),
                  FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  UploadType = table.Column<int>(type: "int", nullable: true),
                  Workday = table.Column<DateTime>(type: "datetime2", nullable: true),
                  THours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                  JobTitleCode = table.Column<int>(type: "int", nullable: true),
                  PayTypeCode = table.Column<int>(type: "int", nullable: true),
                  FileDetailId = table.Column<int>(type: "int", nullable: true),
                  Status = table.Column<int>(type: "int", nullable: true),
                  CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                  Createby = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                  UpdateBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                  IsActive = table.Column<bool>(type: "bit", nullable: true)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_TimesheetData", x => x.Id);
              });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Timesheet");
        }
    }
}
