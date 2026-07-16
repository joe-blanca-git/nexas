using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonVideoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "Lessons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "Lessons",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Lessons",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Thumbnail",
                table: "Lessons",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "Lessons",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Thumbnail",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Lessons");
        }
    }
}
