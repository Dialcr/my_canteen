using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Canteen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class deliverytime_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Carts_CanteenCartId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_CanteenCartId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "CanteenCartId",
                table: "Requests");

            migrationBuilder.AddColumn<int>(
                name: "CartId",
                table: "Requests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryTimeId",
                table: "Requests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DeliveryTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EstablishmentId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    DeliveryTimeType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryTimes_Establishments_EstablishmentId",
                        column: x => x.EstablishmentId,
                        principalTable: "Establishments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CartId",
                table: "Requests",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_DeliveryTimeId",
                table: "Requests",
                column: "DeliveryTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryTimes_EstablishmentId",
                table: "DeliveryTimes",
                column: "EstablishmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Carts_CartId",
                table: "Requests",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_DeliveryTimes_DeliveryTimeId",
                table: "Requests",
                column: "DeliveryTimeId",
                principalTable: "DeliveryTimes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Carts_CartId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_DeliveryTimes_DeliveryTimeId",
                table: "Requests");

            migrationBuilder.DropTable(
                name: "DeliveryTimes");

            migrationBuilder.DropIndex(
                name: "IX_Requests_CartId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_DeliveryTimeId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "CartId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DeliveryTimeId",
                table: "Requests");

            migrationBuilder.AddColumn<int>(
                name: "CanteenCartId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CanteenCartId",
                table: "Requests",
                column: "CanteenCartId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Carts_CanteenCartId",
                table: "Requests",
                column: "CanteenCartId",
                principalTable: "Carts",
                principalColumn: "Id");
        }
    }
}
