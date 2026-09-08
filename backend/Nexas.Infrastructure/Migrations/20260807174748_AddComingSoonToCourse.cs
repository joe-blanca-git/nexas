using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddComingSoonToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsComingSoon",
                table: "Courses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleaseDate",
                table: "Courses",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsComingSoon",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "Courses");
        }
    }
}
