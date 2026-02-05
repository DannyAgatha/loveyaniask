using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plugin.Database.Migrations
{
    /// <inheritdoc />
    public partial class SubClassTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "TierLevel",
                schema: "characters",
                table: "characters",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
            
            migrationBuilder.AddColumn<long>(
                name: "TierExperience",
                schema: "characters",
                table: "characters",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TierLevel",
                schema: "characters",
                table: "characters");
            
            migrationBuilder.DropColumn(
                name: "TierExperience",
                schema: "characters",
                table: "characters");
        }
    }
}
