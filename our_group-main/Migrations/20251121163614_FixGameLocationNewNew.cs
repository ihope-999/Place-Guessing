using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace our_group.Migrations
{
    /// <inheritdoc />
    public partial class FixGameLocationNewNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                schema: "game",
                table: "Locations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "game",
                table: "Locations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
