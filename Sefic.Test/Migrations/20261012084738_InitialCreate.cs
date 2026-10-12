using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sefic.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TaxNumber = table.Column<string>(type: "nchar(9)", fixedLength: true, maxLength: 9, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                    table.CheckConstraint("CK_Partners_Type", "[Type] IN ('CUSTOMER', 'SUPPLIER', 'BOTH')");
                });

            migrationBuilder.InsertData(
                table: "Partners",
                columns: new[] { "Id", "CreatedAt", "CreditLimit", "Email", "IsActive", "Name", "TaxNumber", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("2f3a9c2e-6b41-4d85-a027-e9c6b51f834d"), new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), 20000m, "lucasmudile@gmail.com", true, "Lucas Santana", "002345678", "SUPPLIER", new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("7f369c2e-6b41-4d85-a027-e9c6b51f834d"), new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), 5000m, null, true, "Maria Gomes", "003456789", "BOTH", new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("7f387c2e-6b41-4d85-a027-e9c6b51f834d"), new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), 1000000m, null, true, "Beta Logistica", "006789012", "SUPPLIER", new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("7f3a9c2e-6871-4d85-a027-e9c6b51f834d"), new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), 750m, "contacto@gmail.com", true, "Beta Logistica", "005678901", "CUSTOMER", new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("7f3a9c2e-6b41-4d85-a027-e9c6b51f684d"), new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), 0m, null, false, "Carlos Nunes", "004567890", "CUSTOMER", new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("7f3a9c2e-6b41-4d85-a027-e9c6b51f834d"), new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc), 1500.50m, "maria.santos@gmail.com", true, "Maria Santos", "009876543", "CUSTOMER", new DateTime(2026, 1, 15, 12, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partners_TaxNumber",
                table: "Partners",
                column: "TaxNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Partners");
        }
    }
}
