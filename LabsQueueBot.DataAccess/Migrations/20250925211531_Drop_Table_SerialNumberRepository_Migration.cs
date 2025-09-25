using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LabsQueueBot.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Drop_Table_SerialNumberRepository_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SerialNumberRepository");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SerialNumberRepository",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubjectId = table.Column<int>(type: "integer", nullable: false),
                    QueueIndex = table.Column<int>(type: "integer", nullable: false),
                    TgUserIndex = table.Column<long>(type: "bigint", nullable: false)
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
    }
}
