using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexas.SystemManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleClientIdToSystemApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleClientId",
                table: "Applications",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleClientId",
                table: "Applications");
        }
    }
}
