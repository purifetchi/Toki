using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toki.ActivityPub.Migrations
{
    /// <inheritdoc />
    public partial class PostandUserindexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_User_Id_Descending",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Post_Id_Descending",
                table: "Posts",
                column: "Id",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Id_Descending",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Post_Id_Descending",
                table: "Posts");
        }
    }
}
