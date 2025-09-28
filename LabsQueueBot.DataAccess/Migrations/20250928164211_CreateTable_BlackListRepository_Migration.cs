using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LabsQueueBot.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable_BlackListRepository_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlackListRepository",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false),
                    UnbanDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlackListRepository", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRepository_Username",
                table: "UserRepository",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlackListRepository_UserId_SubjectId",
                table: "BlackListRepository",
                columns: new[] { "UserId", "SubjectId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlackListRepository");

            migrationBuilder.DropIndex(
                name: "IX_UserRepository_Username",
                table: "UserRepository");
        }
    }
}
