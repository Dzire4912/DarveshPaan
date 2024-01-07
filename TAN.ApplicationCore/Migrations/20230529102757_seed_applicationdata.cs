using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAN.ApplicationCore.Migrations
{
    /// <inheritdoc />
    public partial class seed_applicationdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(table: "Application", columns: new[] { "Name", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "IsActive" }, values: new object[,] { 
                { "PBJSnap", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "", "", true }, 
                { "Inventory", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "", "", true }, 
                { "Reporting", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "", "", true } 
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Application", keyColumn: "Name", keyValues: new object[] { "PBJSnap", "Inventory", "Reporting" });
        }
    }
}
