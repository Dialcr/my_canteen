using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Canteen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class changemodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DietaryRestrictionProduct_DietaryRestriction_DietaryRestric~",
                table: "DietaryRestrictionProduct");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DietaryRestriction",
                table: "DietaryRestriction");

            migrationBuilder.RenameTable(
                name: "DietaryRestriction",
                newName: "DietaryRestrictions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DietaryRestrictions",
                table: "DietaryRestrictions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DietaryRestrictionProduct_DietaryRestrictions_DietaryRestri~",
                table: "DietaryRestrictionProduct",
                column: "DietaryRestrictionsId",
                principalTable: "DietaryRestrictions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DietaryRestrictionProduct_DietaryRestrictions_DietaryRestri~",
                table: "DietaryRestrictionProduct");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DietaryRestrictions",
                table: "DietaryRestrictions");

            migrationBuilder.RenameTable(
                name: "DietaryRestrictions",
                newName: "DietaryRestriction");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DietaryRestriction",
                table: "DietaryRestriction",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DietaryRestrictionProduct_DietaryRestriction_DietaryRestric~",
                table: "DietaryRestrictionProduct",
                column: "DietaryRestrictionsId",
                principalTable: "DietaryRestriction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
