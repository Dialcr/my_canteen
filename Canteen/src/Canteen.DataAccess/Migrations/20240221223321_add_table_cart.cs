using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Canteen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class add_table_cart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Requests",
                type: "int",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(25)",
                oldMaxLength: 25)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CanteenCartId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "Products",
                type: "int",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Orders",
                type: "int",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "DiscountType",
                table: "Discounts",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EstablishmentId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    PrductsTotalAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ProductTotalDiscount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DeliveryTotalAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DeliveryTotalDiscount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carts_Establishments_EstablishmentId",
                        column: x => x.EstablishmentId,
                        principalTable: "Establishments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CanteenCartId",
                table: "Requests",
                column: "CanteenCartId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_EstablishmentId",
                table: "Carts",
                column: "EstablishmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Carts_CanteenCartId",
                table: "Requests",
                column: "CanteenCartId",
                principalTable: "Carts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Carts_CanteenCartId",
                table: "Requests");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Requests_CanteenCartId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "CanteenCartId",
                table: "Requests");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Requests",
                type: "varchar(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 25)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Products",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "DiscountType",
                table: "Discounts",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
