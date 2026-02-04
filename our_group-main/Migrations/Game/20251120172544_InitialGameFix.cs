using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace our_group.Migrations
{
    /// <inheritdoc />
    public partial class InitialGameFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "game");

            migrationBuilder.CreateTable(
                name: "Games",
                schema: "game",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrentRound = table.Column<int>(type: "integer", nullable: false),
                    GameStatus = table.Column<int>(type: "integer", nullable: false),
                    Rulebook_NumberOfRounds = table.Column<int>(type: "integer", nullable: false),
                    Rulebook_TimeLimit = table.Column<int>(type: "integer", nullable: false),
                    Rulebook_NumberOfPlayers = table.Column<int>(type: "integer", nullable: false),
                    Rulebook_Threshhold = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                schema: "game",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Difficulty = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                schema: "game",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    PlayerRank = table.Column<int>(type: "integer", nullable: false),
                    PlayerName = table.Column<string>(type: "text", nullable: false),
                    LobbyStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Games_GameId",
                        column: x => x.GameId,
                        principalSchema: "game",
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Review",
                schema: "game",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    Stars = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    AuthorName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review", x => new { x.LocationId, x.Id });
                    table.ForeignKey(
                        name: "FK_Review_Locations_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "game",
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rounds",
                schema: "game",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    WinnerId = table.Column<List<int>>(type: "integer[]", nullable: false),
                    WinnerName = table.Column<List<string>>(type: "text[]", nullable: false),
                    AmountOfPointsGiven = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentReview = table.Column<int>(type: "integer", nullable: false),
                    GameId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rounds_Games_GameId",
                        column: x => x.GameId,
                        principalSchema: "game",
                        principalTable: "Games",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rounds_Locations_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "game",
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerGuess",
                schema: "game",
                columns: table => new
                {
                    RoundId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Lat = table.Column<double>(type: "double precision", nullable: false),
                    Lng = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerGuess", x => new { x.RoundId, x.Id });
                    table.ForeignKey(
                        name: "FK_PlayerGuess_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalSchema: "game",
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_GameId",
                schema: "game",
                table: "Players",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_GameId",
                schema: "game",
                table: "Rounds",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_LocationId",
                schema: "game",
                table: "Rounds",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerGuess",
                schema: "game");

            migrationBuilder.DropTable(
                name: "Players",
                schema: "game");

            migrationBuilder.DropTable(
                name: "Review",
                schema: "game");

            migrationBuilder.DropTable(
                name: "Rounds",
                schema: "game");

            migrationBuilder.DropTable(
                name: "Games",
                schema: "game");

            migrationBuilder.DropTable(
                name: "Locations",
                schema: "game");
        }
    }
}
