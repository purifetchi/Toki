using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toki.ActivityPub.Migrations
{
    /// <inheritdoc />
    public partial class Fixdumbissueinfollowrequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FollowRequests_FromId",
                table: "FollowRequests");

            migrationBuilder.DropIndex(
                name: "IX_FollowRequests_ToId",
                table: "FollowRequests");

            migrationBuilder.DropIndex(
                name: "IX_FollowerRelations_FolloweeId",
                table: "FollowerRelations");

            migrationBuilder.DropIndex(
                name: "IX_FollowerRelations_FollowerId",
                table: "FollowerRelations");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_FromId",
                table: "FollowRequests",
                column: "FromId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_ToId",
                table: "FollowRequests",
                column: "ToId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowerRelations_FolloweeId",
                table: "FollowerRelations",
                column: "FolloweeId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowerRelations_FollowerId",
                table: "FollowerRelations",
                column: "FollowerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FollowRequests_FromId",
                table: "FollowRequests");

            migrationBuilder.DropIndex(
                name: "IX_FollowRequests_ToId",
                table: "FollowRequests");

            migrationBuilder.DropIndex(
                name: "IX_FollowerRelations_FolloweeId",
                table: "FollowerRelations");

            migrationBuilder.DropIndex(
                name: "IX_FollowerRelations_FollowerId",
                table: "FollowerRelations");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_FromId",
                table: "FollowRequests",
                column: "FromId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_ToId",
                table: "FollowRequests",
                column: "ToId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FollowerRelations_FolloweeId",
                table: "FollowerRelations",
                column: "FolloweeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FollowerRelations_FollowerId",
                table: "FollowerRelations",
                column: "FollowerId",
                unique: true);
        }
    }
}
