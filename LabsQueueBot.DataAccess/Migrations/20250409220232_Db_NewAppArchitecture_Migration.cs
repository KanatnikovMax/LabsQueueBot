using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LabsQueueBot.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Db_NewAppArchitecture_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRoleRepository");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "UserRepository",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EvenWeekTimetable",
                table: "SubjectRepository",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OddWeekTimetable",
                table: "SubjectRepository",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "UserRepository");

            migrationBuilder.DropColumn(
                name: "EvenWeekTimetable",
                table: "SubjectRepository");

            migrationBuilder.DropColumn(
                name: "OddWeekTimetable",
                table: "SubjectRepository");

            migrationBuilder.CreateTable(
                name: "UserRoleRepository",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsersRole = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleRepository", x => x.Id);
                });
        }
    }
}
