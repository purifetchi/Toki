using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Toki.ActivityPub.Models.Posts;

#nullable disable

namespace Toki.ActivityPub.Migrations
{
    /// <inheritdoc />
    public partial class Makementionsmoreexpressive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mentions",
                table: "Posts");

            migrationBuilder.AddColumn<List<PostMention>>(
                name: "UserMentions",
                table: "Posts",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
