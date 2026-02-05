using Microsoft.EntityFrameworkCore.Migrations;
using WingsAPI.Data.Character;

#nullable disable

namespace Plugin.Database.Migrations
{
    /// <inheritdoc />
    public partial class LandOfLifeRestriction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<LandOfLifeRestrictionDto>(
                name: "LandOfLifeRestrictionDto",
                schema: "characters",
                table: "characters",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LandOfLifeRestrictionDto",
                schema: "characters",
                table: "characters");
        }
    }
}
