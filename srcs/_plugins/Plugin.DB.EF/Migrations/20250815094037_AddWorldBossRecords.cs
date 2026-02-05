using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using WingsAPI.Data.WorldBoss;

#nullable disable

namespace Plugin.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddWorldBossRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<WorldBossRecordDto>>(
                name: "WorldBossRecordsDto",
                schema: "characters",
                table: "characters",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorldBossRecordsDto",
                schema: "characters",
                table: "characters");
        }
    }
}
