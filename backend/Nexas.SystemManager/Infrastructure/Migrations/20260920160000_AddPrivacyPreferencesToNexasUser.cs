using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexas.SystemManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivacyPreferencesToNexasUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReceiveMarketingEmails",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReceiveProductNotifications",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiveMarketingEmails",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ReceiveProductNotifications",
                table: "AspNetUsers");
        }
    }
}
