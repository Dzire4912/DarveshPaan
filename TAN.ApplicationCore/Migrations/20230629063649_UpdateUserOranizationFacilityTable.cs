using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAN.ApplicationCore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserOranizationFacilityTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MyProperty",
                table: "UserOrganizationFacilities",
                newName: "Id");

            //migrationBuilder.AddColumn<int>(
            //    name: "AgencyId",
            //    table: "Timesheet",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "AgencyId",
            //    table: "Timesheet");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserOrganizationFacilities",
                newName: "MyProperty");
        }
    }
}
