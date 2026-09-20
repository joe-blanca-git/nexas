using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexas.SystemManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToSystemApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Applications",
                type: "longtext",
                nullable: false,
                defaultValue: "active")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Applications");
        }
    }
}
