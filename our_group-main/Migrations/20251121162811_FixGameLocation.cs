using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace our_group.Migrations
{
    /// <inheritdoc />
    public partial class FixGameLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "game",
                table: "Locations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExternalLocationId",
                schema: "game",
                table: "Locations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                schema: "game",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ExternalLocationId",
                schema: "game",
                table: "Locations");
        }
    }
}
