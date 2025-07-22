using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FigureShop.Migrations
{
    /// <inheritdoc />
    public partial class AddProductImagePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath2",
                table: "Products",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath3",
                table: "Products",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath2",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImagePath3",
                table: "Products");
        }
    }
}
