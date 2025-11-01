using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace room_for_lease_api.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Deposit",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ElectricConsumption",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ElectricNewReading",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ElectricOldReading",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ElectricUnitPrice",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RoomRent",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RoomRentUnitPrice",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceCost",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ServiceUnitPrice",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WaterConsumption",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WaterNewReading",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WaterOldReading",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WaterUnitPrice",
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
                name: "Deposit",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ElectricConsumption",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ElectricNewReading",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ElectricOldReading",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ElectricUnitPrice",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RoomRent",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RoomRentUnitPrice",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ServiceCost",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ServiceUnitPrice",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "WaterConsumption",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "WaterNewReading",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "WaterOldReading",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "WaterUnitPrice",
                table: "Invoices");
        }
    }
}
