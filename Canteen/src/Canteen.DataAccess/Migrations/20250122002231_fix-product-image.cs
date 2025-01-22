using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Canteen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class fixproductimage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImageUrls_Products_ProductId",
                table: "ProductImageUrls");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ProductImageUrls",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImageUrls_Products_ProductId",
                table: "ProductImageUrls",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImageUrls_Products_ProductId",
                table: "ProductImageUrls");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ProductImageUrls",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImageUrls_Products_ProductId",
                table: "ProductImageUrls",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
