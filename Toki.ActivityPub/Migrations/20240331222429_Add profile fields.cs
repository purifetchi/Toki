using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toki.ActivityPub.Migrations
{
    /// <inheritdoc />
    public partial class Addprofilefields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Fields",
                table: "Users",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fields",
                table: "Users");
        }
    }
}
