using Microsoft.EntityFrameworkCore.Migrations;
using WingsAPI.Data.Character;

#nullable disable

namespace Plugin.Database.Migrations
{
    /// <inheritdoc />
    public partial class Icebreaker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<IcebreakerLeaverBusterDto>(
                name: "IcebreakerLeaverBusterDto",
                schema: "characters",
                table: "characters",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IcebreakerLeaverBusterDto",
                schema: "characters",
                table: "characters");
        }
    }
}
