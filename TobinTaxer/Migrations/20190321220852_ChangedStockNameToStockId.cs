using Microsoft.EntityFrameworkCore.Migrations;

namespace TobinTaxer.Migrations
{
    public partial class ChangedStockNameToStockId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockName",
                table: "TaxHistories");

            migrationBuilder.AddColumn<long>(
                name: "StockId",
                table: "TaxHistories",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockId",
                table: "TaxHistories");

            migrationBuilder.AddColumn<string>(
                name: "StockName",
                table: "TaxHistories",
                nullable: true);
        }
    }
}
