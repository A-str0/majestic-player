using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace majestic_player.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tracks_Id",
                table: "Tracks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tracks_Id",
                table: "Tracks",
                column: "Id",
                unique: true);
        }
    }
}
