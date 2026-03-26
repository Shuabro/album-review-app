using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAlbumIdToUserSongRanking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlbumId",
                table: "UserSongRankings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserSongRankings_AlbumId",
                table: "UserSongRankings",
                column: "AlbumId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSongRankings_Albums_AlbumId",
                table: "UserSongRankings",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSongRankings_Albums_AlbumId",
                table: "UserSongRankings");

            migrationBuilder.DropIndex(
                name: "IX_UserSongRankings_AlbumId",
                table: "UserSongRankings");

            migrationBuilder.DropColumn(
                name: "AlbumId",
                table: "UserSongRankings");
        }
    }
}
