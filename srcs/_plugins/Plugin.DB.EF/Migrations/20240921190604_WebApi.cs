using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Plugin.Database.Migrations
{
    /// <inheritdoc />
    public partial class WebApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "accounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "text", nullable: true),
                    Expires = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false),
                    AccountEntityId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_accounts_AccountEntityId",
                        column: x => x.AccountEntityId,
                        principalSchema: "accounts",
                        principalTable: "accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_AccountEntityId",
                schema: "accounts",
                table: "refresh_tokens",
                column: "AccountEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "accounts");
        }
    }
}
