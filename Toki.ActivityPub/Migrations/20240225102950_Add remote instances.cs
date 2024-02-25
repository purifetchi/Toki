using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toki.ActivityPub.Migrations
{
    /// <inheritdoc />
    public partial class Addremoteinstances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentInstanceId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentWarning",
                table: "Posts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Instances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: false),
                    SharedInbox = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Software = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ParentInstanceId",
                table: "Users",
                column: "ParentInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Instances_ParentInstanceId",
                table: "Users",
                column: "ParentInstanceId",
                principalTable: "Instances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Instances_ParentInstanceId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Instances");

            migrationBuilder.DropIndex(
                name: "IX_Users_ParentInstanceId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ParentInstanceId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContentWarning",
                table: "Posts");
        }
    }
}
