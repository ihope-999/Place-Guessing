using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace our_group.Migrations.User
{
    /// <inheritdoc />
    public partial class UserFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Follows",
                schema: "user");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Follows",
                schema: "user",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FolloweeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Follows", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowerId_FolloweeId",
                schema: "user",
                table: "Follows",
                columns: new[] { "FollowerId", "FolloweeId" },
                unique: true);
        }
    }
}
