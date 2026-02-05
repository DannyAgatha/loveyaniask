using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plugin.Database.Migrations
{
    /// <inheritdoc />
    public partial class SubClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "SubClass",
                schema: "characters",
                table: "characters",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubClass",
                schema: "characters",
                table: "characters");
        }
    }
}
