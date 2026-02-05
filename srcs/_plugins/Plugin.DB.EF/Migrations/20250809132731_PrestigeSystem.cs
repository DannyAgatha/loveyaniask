using Microsoft.EntityFrameworkCore.Migrations;
using WingsAPI.Data.Prestige;

#nullable disable

namespace Plugin.Database.Migrations
{
    /// <inheritdoc />
    public partial class PrestigeSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<CharacterPrestigeDto>(
                name: "CharacterPrestigeDto",
                schema: "characters",
                table: "characters",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CharacterPrestigeDto",
                schema: "characters",
                table: "characters");
        }
    }
}
