using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Canteen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addestablishmentcategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Establishments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Establishments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Establishments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusBase",
                table: "Establishments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EstablishmentsCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Image = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StatusBase = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstablishmentsCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstablishmentEstablishmentCategory",
                columns: table => new
                {
                    EstablishmentCategoriesId = table.Column<int>(type: "integer", nullable: false),
                    EstablishmentsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstablishmentEstablishmentCategory", x => new { x.EstablishmentCategoriesId, x.EstablishmentsId });
                    table.ForeignKey(
                        name: "FK_EstablishmentEstablishmentCategory_EstablishmentsCategory_E~",
                        column: x => x.EstablishmentCategoriesId,
                        principalTable: "EstablishmentsCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstablishmentEstablishmentCategory_Establishments_Establish~",
                        column: x => x.EstablishmentsId,
                        principalTable: "Establishments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentEstablishmentCategory_EstablishmentsId",
                table: "EstablishmentEstablishmentCategory",
                column: "EstablishmentsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstablishmentEstablishmentCategory");

            migrationBuilder.DropTable(
                name: "EstablishmentsCategory");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "StatusBase",
                table: "Establishments");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Establishments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
