using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAN.ApplicationCore.Migrations
{
    /// <inheritdoc />
    public partial class addFileErrorRecordsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileErrorRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileRowData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileDetailId = table.Column<bool>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),

                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    ReportQuarter = table.Column<DateTime>(type: "int", nullable: false),
                    Month = table.Column<string>(type: "int", nullable: false),
                    Year = table.Column<string>(type: "int", nullable: false),
                    UploadType = table.Column<bool>(type: "int", nullable: false),

                    Status = table.Column<bool>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileErrorRecordsData", x => x.Id);

                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("dbo.FileErrorRecords");
        }
    }
}
