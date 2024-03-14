using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Toki.ActivityPub.Migrations
{
    /// <inheritdoc />
    public partial class TheGreatUlidening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Instances",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "OAuthApps",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: false),
                    ClientSecret = table.Column<string>(type: "text", nullable: false),
                    Scopes = table.Column<List<string>>(type: "text[]", nullable: false),
                    Website = table.Column<string>(type: "text", nullable: true),
                    RedirectUris = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthApps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    Handle = table.Column<string>(type: "text", nullable: false),
                    ParentInstanceId = table.Column<string>(type: "character varying(26)", nullable: true),
                    RequiresFollowApproval = table.Column<bool>(type: "boolean", nullable: false),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    IsRemote = table.Column<bool>(type: "boolean", nullable: false),
                    Inbox = table.Column<string>(type: "text", nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    BannerUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RemoteId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Instances_ParentInstanceId",
                        column: x => x.ParentInstanceId,
                        principalTable: "Instances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Credentials",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false),
                    UserId = table.Column<string>(type: "character varying(26)", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "FollowerRelations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false),
                    FollowerId = table.Column<string>(type: "character varying(26)", nullable: false),
                    FolloweeId = table.Column<string>(type: "character varying(26)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowerRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FollowerRelations_Users_FolloweeId",
                        column: x => x.FolloweeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FollowerRelations_Users_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FollowRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false),
                    FromId = table.Column<string>(type: "character varying(26)", nullable: false),
                    ToId = table.Column<string>(type: "character varying(26)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RemoteId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FollowRequests_Users_FromId",
                        column: x => x.FromId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FollowRequests_Users_ToId",
                        column: x => x.ToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Keypairs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false),
                    OwnerId = table.Column<string>(type: "character varying(26)", nullable: false),
                    PublicKey = table.Column<string>(type: "text", nullable: false),
                    PrivateKey = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RemoteId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keypairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Keypairs_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OAuthTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false),
                    AuthorizationCode = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ParentAppId = table.Column<string>(type: "character varying(26)", nullable: false),
                    UserId = table.Column<string>(type: "character varying(26)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Scopes = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OAuthTokens_OAuthApps_ParentAppId",
                        column: x => x.ParentAppId,
                        principalTable: "OAuthApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OAuthTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false),
                    AuthorId = table.Column<string>(type: "character varying(26)", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<string>(type: "character varying(26)", nullable: true),
                    Context = table.Column<Guid>(type: "uuid", nullable: true),
                    BoostingId = table.Column<string>(type: "character varying(26)", nullable: true),
                    Sensitive = table.Column<bool>(type: "boolean", nullable: false),
                    ContentWarning = table.Column<string>(type: "text", nullable: true),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    LikeCount = table.Column<int>(type: "integer", nullable: false),
                    BoostCount = table.Column<int>(type: "integer", nullable: false),
                    Mentions = table.Column<List<string>>(type: "text[]", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RemoteId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_Posts_BoostingId",
                        column: x => x.BoostingId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Posts_Posts_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Posts_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<string>(type: "character varying(26)", nullable: false),
                    ActorId = table.Column<string>(type: "character varying(26)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RelevantPostId = table.Column<string>(type: "character varying(26)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Posts_RelevantPostId",
                        column: x => x.RelevantPostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostAttachments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false),
                    ParentId = table.Column<string>(type: "character varying(26)", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Mime = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostAttachments_Posts_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PostLikes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false),
                    PostId = table.Column<string>(type: "character varying(26)", nullable: false),
                    LikingUserId = table.Column<string>(type: "character varying(26)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostLikes_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostLikes_Users_LikingUserId",
                        column: x => x.LikingUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_UserId",
                table: "Credentials",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FollowerRelations_FolloweeId",
                table: "FollowerRelations",
                column: "FolloweeId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowerRelations_FollowerId",
                table: "FollowerRelations",
                column: "FollowerId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_FromId",
                table: "FollowRequests",
                column: "FromId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_ToId",
                table: "FollowRequests",
                column: "ToId");

            migrationBuilder.CreateIndex(
                name: "IX_Keypairs_OwnerId",
                table: "Keypairs",
                column: "OwnerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ActorId",
                table: "Notifications",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelevantPostId",
                table: "Notifications",
                column: "RelevantPostId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TargetId",
                table: "Notifications",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_OAuthTokens_ParentAppId",
                table: "OAuthTokens",
                column: "ParentAppId");

            migrationBuilder.CreateIndex(
                name: "IX_OAuthTokens_UserId",
                table: "OAuthTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PostAttachments_ParentId",
                table: "PostAttachments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_LikingUserId",
                table: "PostLikes",
                column: "LikingUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_PostId",
                table: "PostLikes",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_AuthorId",
                table: "Posts",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_BoostingId",
                table: "Posts",
                column: "BoostingId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_ParentId",
                table: "Posts",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ParentInstanceId",
                table: "Users",
                column: "ParentInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Credentials");

            migrationBuilder.DropTable(
                name: "FollowerRelations");

            migrationBuilder.DropTable(
                name: "FollowRequests");

            migrationBuilder.DropTable(
                name: "Keypairs");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OAuthTokens");

            migrationBuilder.DropTable(
                name: "PostAttachments");

            migrationBuilder.DropTable(
                name: "PostLikes");

            migrationBuilder.DropTable(
                name: "OAuthApps");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Instances");
        }
    }
}
