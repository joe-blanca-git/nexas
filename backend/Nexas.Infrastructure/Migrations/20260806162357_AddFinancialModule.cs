using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ParticipationPercentage",
                table: "CourseTeachers",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "FinancialClosings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    GrossRevenue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    BankFeesTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NexasFeesTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NetValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalToReceive = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AsaasTransferId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentReceiptUrl = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialClosings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialClosings_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "TeacherId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FinancialTaxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTaxes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FinancialClosingItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FinancialClosingId = table.Column<int>(type: "int", nullable: false),
                    PurchaseId = table.Column<int>(type: "int", nullable: false),
                    AppliedTeacherPercentage = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    GrossValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    BankFeeValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NexasFeeValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CalculatedValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialClosingItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialClosingItems_FinancialClosings_FinancialClosingId",
                        column: x => x.FinancialClosingId,
                        principalTable: "FinancialClosings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinancialClosingItems_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "PurchaseId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClosingItems_FinancialClosingId",
                table: "FinancialClosingItems",
                column: "FinancialClosingId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClosingItems_PurchaseId",
                table: "FinancialClosingItems",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialClosings_TeacherId",
                table: "FinancialClosings",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialClosingItems");

            migrationBuilder.DropTable(
                name: "FinancialTaxes");

            migrationBuilder.DropTable(
                name: "FinancialClosings");

            migrationBuilder.DropColumn(
                name: "ParticipationPercentage",
                table: "CourseTeachers");
        }
    }
}
