using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toki.ActivityPub.Migrations
{
    /// <inheritdoc />
    public partial class Makepostsoptionalfortheattachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostAttachments_Posts_ParentId",
                table: "PostAttachments");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "PostAttachments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_PostAttachments_Posts_ParentId",
                table: "PostAttachments",
                column: "ParentId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostAttachments_Posts_ParentId",
                table: "PostAttachments");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "PostAttachments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PostAttachments_Posts_ParentId",
                table: "PostAttachments",
                column: "ParentId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
