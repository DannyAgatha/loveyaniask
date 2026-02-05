using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using WingsAPI.Data.Character;

#nullable disable

namespace Plugin.Database.Migrations
{
    /// <inheritdoc />
    public partial class CookingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<CharacterCookingDto>>(
                name: "CharacterCookingDto",
                schema: "characters",
                table: "characters",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FoodValue",
                schema: "characters",
                table: "characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CharacterCookingDto",
                schema: "characters",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "FoodValue",
                schema: "characters",
                table: "characters");
        }
    }
}
