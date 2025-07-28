using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FigureShop.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFlashSaleToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFlashSale",
                table: "Products",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFlashSale",
                table: "Products");
        }
    }
}
