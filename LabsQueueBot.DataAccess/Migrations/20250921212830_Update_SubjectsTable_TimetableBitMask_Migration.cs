using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabsQueueBot.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Update_SubjectsTable_TimetableBitMask_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvenWeekTimetable",
                table: "SubjectRepository");

            migrationBuilder.DropColumn(
                name: "OddWeekTimetable",
                table: "SubjectRepository");

            migrationBuilder.AlterColumn<string>(
                name: "SubjectName",
                table: "SubjectRepository",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "DenWeekTimetableMask",
                table: "SubjectRepository",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumWeekTimetableMask",
                table: "SubjectRepository",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DenWeekTimetableMask",
                table: "SubjectRepository");

            migrationBuilder.DropColumn(
                name: "NumWeekTimetableMask",
                table: "SubjectRepository");

            migrationBuilder.AlterColumn<string>(
                name: "SubjectName",
                table: "SubjectRepository",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

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
    }
}
