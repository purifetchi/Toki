using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toki.ActivityPub.Migrations
{
    /// <inheritdoc />
    public partial class Minifymentions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserMentions",
                table: "Posts");

            migrationBuilder.AddColumn<List<string>>(
                name: "Mentions",
                table: "Posts",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mentions",
                table: "Posts");

            migrationBuilder.AddColumn<string>(
                name: "UserMentions",
                table: "Posts",
                type: "jsonb",
                nullable: true);
        }
    }
}
