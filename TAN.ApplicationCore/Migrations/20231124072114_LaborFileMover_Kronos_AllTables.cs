using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAN.ApplicationCore.Migrations
{
    /// <inheritdoc />
    public partial class LaborFileMover_Kronos_AllTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
       
            migrationBuilder.AddColumn<bool>(
                name: "HasDeductions",
                table: "Organizations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Quarter",
                table: "Months",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "KronosBlobStorageCreds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StorageConnectionString = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KronosDecryptOrEncryptKeyBlobNameGLC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KronosDecryptionPasswordGLC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KronosBlobStorageCreds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KronosBlobStorageCreds_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KronosDataTracker",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecordsCount = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToTable = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AutoSyncCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KronosDataTracker", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "kronosFacilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacilityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    Refreshmentdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kronosFacilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KronosOneTimeUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkDay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KronosOneTimeUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KronosOneTimeUpdates_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KronosPaytypeMappings",
                columns: table => new
                {
                    kronosPaytype = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Paytype = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "kronosPendingRecords",
                columns: table => new
                {
                    rowid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    FacilityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkDay = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kronosPendingRecords", x => x.rowid);
                });

            migrationBuilder.CreateTable(
                name: "KronosPunchExport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    facilityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NATIVEKRONOSID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DEPARTMENTNUMBER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JOBCODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADJUSTEDAPPLYDATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TIMEINHOURS = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HrJob = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KronosPunchExport", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "kronosSftpCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KronosUserNameGLC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KronosPasswordGLC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KronosHostGLC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KronosPortGLC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kronosSftpCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kronosSftpCredentials_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kronosSftpFileEndpoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KronosPunchExportGLC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KronosPayrollGLC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kronosSftpFileEndpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kronosSftpFileEndpoints_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KronosSpResults",
                columns: table => new
                {
                    EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayTypeCode = table.Column<int>(type: "int", nullable: false),
                    JobTitleCode = table.Column<int>(type: "int", nullable: false),
                    Workday = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    THours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "kronosTimeSheetMapped",
                columns: table => new
                {
                    rowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayTypeCode = table.Column<int>(type: "int", nullable: false),
                    JobTitleCode = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    ReportQuarter = table.Column<int>(type: "int", nullable: false),
                    Workday = table.Column<DateTime>(type: "datetime2", nullable: false),
                    THours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    UploadType = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Createby = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kronosTimeSheetMapped", x => x.rowId);
                });

            migrationBuilder.CreateTable(
                name: "KronosToPbj",
                columns: table => new
                {
                    DepartmentCode = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Job = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PBJJobCode = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_KronosBlobStorageCreds_OrganizationId",
                table: "KronosBlobStorageCreds",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_KronosOneTimeUpdates_FacilityId",
                table: "KronosOneTimeUpdates",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_kronosSftpCredentials_OrganizationId",
                table: "kronosSftpCredentials",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_kronosSftpFileEndpoints_OrganizationId",
                table: "kronosSftpFileEndpoints",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KronosBlobStorageCreds");

            migrationBuilder.DropTable(
                name: "KronosDataTracker");

            migrationBuilder.DropTable(
                name: "kronosFacilities");

            migrationBuilder.DropTable(
                name: "KronosOneTimeUpdates");

            migrationBuilder.DropTable(
                name: "KronosPaytypeMappings");

            migrationBuilder.DropTable(
                name: "kronosPendingRecords");

            migrationBuilder.DropTable(
                name: "KronosPunchExport");

            migrationBuilder.DropTable(
                name: "kronosSftpCredentials");

            migrationBuilder.DropTable(
                name: "kronosSftpFileEndpoints");

            migrationBuilder.DropTable(
                name: "KronosSpResults");

            migrationBuilder.DropTable(
                name: "kronosTimeSheetMapped");

            migrationBuilder.DropTable(
                name: "KronosToPbj");

            migrationBuilder.DropColumn(
                name: "HasDeductions",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "Quarter",
                table: "Months");

          
        }
    }
}
