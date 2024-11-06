using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LabsQueueBot.Migrations
{
    /// <inheritdoc />
    public partial class queue_bot_db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubjectRepository",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubjectName = table.Column<string>(type: "text", nullable: false),
                    CourseNumber = table.Column<byte>(type: "smallint", nullable: false),
                    GroupNumber = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectRepository", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRepository",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CourseNumber = table.Column<byte>(type: "smallint", nullable: false),
                    GroupNumber = table.Column<byte>(type: "smallint", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    IsNotifyNeeded = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRepository", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SerialNumberRepository",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QueueIndex = table.Column<int>(type: "integer", nullable: false),
                    TgUserIndex = table.Column<long>(type: "bigint", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialNumberRepository", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SerialNumberRepository_SubjectRepository_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "SubjectRepository",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumberRepository_SubjectId",
                table: "SerialNumberRepository",
                column: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SerialNumberRepository");

            migrationBuilder.DropTable(
                name: "UserRepository");

            migrationBuilder.DropTable(
                name: "SubjectRepository");
        }
    }
}
