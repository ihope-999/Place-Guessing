using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace our_group.Migrations.LocationDb
{
    /// <inheritdoc />
    public partial class AddCachedReviewFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Flagged",
                schema: "location",
                table: "CachedReviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flagged",
                schema: "location",
                table: "CachedReviews");
        }
    }
}
