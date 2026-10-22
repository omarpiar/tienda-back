using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tienda_back.Migrations
{
    /// <inheritdoc />
    public partial class AgregarStockMinimoAProductos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockMinimo",
                table: "Productos",
                type: "integer",
                nullable: false,
                defaultValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockMinimo",
                table: "Productos");
        }
    }
}
