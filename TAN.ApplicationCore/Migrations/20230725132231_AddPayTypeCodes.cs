using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAN.ApplicationCore.Migrations
{
    /// <inheritdoc />
    public partial class AddPayTypeCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayTypeCodes",
                columns: table => new
                {
                    PayTypeCode = table.Column<DateTime>(type: "int", nullable: false),
                    PayTypeDescription = table.Column<DateTime>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("PayTypeCodes");
        }
    }
}
