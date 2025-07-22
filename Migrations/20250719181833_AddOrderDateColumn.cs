using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FigureShop.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderDateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<DateTime>(
        name: "OrderDate",
        table: "Orders",
        nullable: false,
        defaultValue: new DateTime(2000, 1, 1));
}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "OrderDate",
        table: "Orders");
}
    }
}
