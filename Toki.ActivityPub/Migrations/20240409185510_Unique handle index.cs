using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toki.ActivityPub.Migrations
{
    /// <inheritdoc />
    public partial class Uniquehandleindex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_User_Handle_Unique",
                table: "Users",
                column: "Handle",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RemoteInstance_Domain_Unique",
                table: "Instances",
                column: "Domain",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Handle_Unique",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_RemoteInstance_Domain_Unique",
                table: "Instances");
        }
    }
}
