using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Canteen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class add_price_column_into_product_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "MenuProducts");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Products",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Products");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "MenuProducts",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
