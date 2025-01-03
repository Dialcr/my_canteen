using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Canteen.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addnameappUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");
            {
                migrationBuilder.Sql(@"
        UPDATE ""AspNetUsers"" 
        SET ""Name"" = ""UserName"" 
        WHERE ""Name"" = ''");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "AspNetUsers");
            migrationBuilder.Sql(@"
        UPDATE ""AspNetUsers"" 
        SET ""Name"" = '' ");

        }
    }
}
