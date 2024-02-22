using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toki.ActivityPub.Migrations
{
    /// <inheritdoc />
    public partial class Unbreakschemarelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Keypairs",
                table: "Keypairs");

            migrationBuilder.DropIndex(
                name: "IX_FollowerRelations_FolloweeId",
                table: "FollowerRelations");

            migrationBuilder.DropIndex(
                name: "IX_FollowerRelations_FollowerId",
                table: "FollowerRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Keypairs",
                table: "Keypairs",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Credentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Salt = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Credentials_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Keypairs_OwnerId",
                table: "Keypairs",
                column: "OwnerId",
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

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_UserId",
                table: "Credentials",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Credentials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Keypairs",
                table: "Keypairs");

            migrationBuilder.DropIndex(
                name: "IX_Keypairs_OwnerId",
                table: "Keypairs");

            migrationBuilder.DropIndex(
                name: "IX_FollowerRelations_FolloweeId",
                table: "FollowerRelations");

            migrationBuilder.DropIndex(
                name: "IX_FollowerRelations_FollowerId",
                table: "FollowerRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Keypairs",
                table: "Keypairs",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowerRelations_FolloweeId",
                table: "FollowerRelations",
                column: "FolloweeId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowerRelations_FollowerId",
                table: "FollowerRelations",
                column: "FollowerId");
        }
    }
}
