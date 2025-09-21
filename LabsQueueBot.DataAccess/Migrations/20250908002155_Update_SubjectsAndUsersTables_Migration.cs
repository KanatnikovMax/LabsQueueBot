using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabsQueueBot.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Update_SubjectsAndUsersTables_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StateModifiedAt",
                table: "UserRepository",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long[]>(
                name: "Queue",
                table: "SubjectRepository",
                type: "bigint[]",
                nullable: false,
                defaultValue: new long[0]);

            migrationBuilder.AddColumn<long[]>(
                name: "Waiting",
                table: "SubjectRepository",
                type: "bigint[]",
                nullable: false,
                defaultValue: new long[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StateModifiedAt",
                table: "UserRepository");

            migrationBuilder.DropColumn(
                name: "Queue",
                table: "SubjectRepository");

            migrationBuilder.DropColumn(
                name: "Waiting",
                table: "SubjectRepository");
        }
    }
}
