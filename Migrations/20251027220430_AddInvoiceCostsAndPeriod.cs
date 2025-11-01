using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace room_for_lease_api.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceCostsAndPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ElectricCost",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Period",
                table: "Invoices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "WaterCost",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElectricCost",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "WaterCost",
                table: "Invoices");
        }
    }
}
