using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexas.SystemManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeyToSystemApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "Applications",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApiKey",
                table: "Applications",
                column: "ApiKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Applications_ApiKey",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "Applications");
        }
    }
}
