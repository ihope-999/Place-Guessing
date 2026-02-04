using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace our_group.Migrations.LocationDb
{
    /// <inheritdoc />
    public partial class FixLocationReviewTexts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CachedReviews_LocationId",
                schema: "location",
                table: "CachedReviews",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_CachedReviews_Locations_LocationId",
                schema: "location",
                table: "CachedReviews",
                column: "LocationId",
                principalSchema: "location",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CachedReviews_Locations_LocationId",
                schema: "location",
                table: "CachedReviews");

            migrationBuilder.DropIndex(
                name: "IX_CachedReviews_LocationId",
                schema: "location",
                table: "CachedReviews");
        }
    }
}
