using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toki.ActivityPub.Migrations
{
    /// <inheritdoc />
    public partial class Addremoteidtofollowerrelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "FollowerRelations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "RemoteId",
                table: "FollowerRelations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "FollowerRelations");

            migrationBuilder.DropColumn(
                name: "RemoteId",
                table: "FollowerRelations");
        }
    }
}
