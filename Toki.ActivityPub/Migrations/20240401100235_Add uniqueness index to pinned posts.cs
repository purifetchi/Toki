using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toki.ActivityPub.Migrations
{
    /// <inheritdoc />
    public partial class Adduniquenessindextopinnedposts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PinnedPosts_PostId",
                table: "PinnedPosts");

            migrationBuilder.DropIndex(
                name: "IX_PinnedPosts_UserId",
                table: "PinnedPosts");

            migrationBuilder.CreateIndex(
                name: "IX_PinnedPosts_UserId",
                table: "PinnedPosts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PostUser_Unique",
                table: "PinnedPosts",
                columns: new[] { "PostId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PinnedPosts_UserId",
                table: "PinnedPosts");

            migrationBuilder.DropIndex(
                name: "IX_PostUser_Unique",
                table: "PinnedPosts");

            migrationBuilder.CreateIndex(
                name: "IX_PinnedPosts_PostId",
                table: "PinnedPosts",
                column: "PostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PinnedPosts_UserId",
                table: "PinnedPosts",
                column: "UserId",
                unique: true);
        }
    }
}
