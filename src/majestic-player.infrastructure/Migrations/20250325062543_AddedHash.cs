using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace majestic_player.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "Tracks",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Tracks");
        }
    }
}
