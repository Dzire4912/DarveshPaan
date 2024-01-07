using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAN.ApplicationCore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserOranizationFacilityTableChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserOrganizationFacilities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "UserOrganizationFacilities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserOrganizationFacilities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "UserOrganizationFacilities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "UserOrganizationFacilities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserOrganizationFacilities");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "UserOrganizationFacilities");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserOrganizationFacilities");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "UserOrganizationFacilities");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "UserOrganizationFacilities");
        }
    }
}
