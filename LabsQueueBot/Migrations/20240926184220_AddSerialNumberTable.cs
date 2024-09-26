using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabsQueueBot.Migrations
{
    /// <inheritdoc />
    public partial class AddSerialNumberTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SerialNumberRepository",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false),
                    UserIndex = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerialNumberRepository", x => new { x.Id, x.SubjectId });
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
        }
    }
}
